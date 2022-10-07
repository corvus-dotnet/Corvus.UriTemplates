// <copyright file="ParameterCache.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;
using CommunityToolkit.HighPerformance;

namespace Corvus.UriTemplates;

/// <summary>
/// A cache for parameters extracted from a URI template.
/// </summary>
internal struct ParameterCache
{
    private readonly int bufferIncrement;
    private CacheEntry[] items;
    private int written;
    private bool returned = false;

    private ParameterCache(int initialCapacity)
    {
        this.bufferIncrement = initialCapacity;
        this.items = ArrayPool<CacheEntry>.Shared.Rent(initialCapacity);
        this.written = 0;
    }

    /// <summary>
    /// Enumerate the parameters in the parser.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the callback.</typeparam>
    /// <param name="parser">The parser to use.</param>
    /// <param name="uri">The uri to parse.</param>
    /// <param name="initialCapacity">The initial cache size, which should be greater than or equal to the expected number of parameters.
    /// It also provides the increment for the cache size should it be exceeded.</param>
    /// <param name="callback">The callback to recieve the enumerated parameters.</param>
    /// <param name="state">The state for the callback.</param>
    /// <returns><see langword="true"/> if the parser was successful, otherwise <see langword="false"/>.</returns>
    public static bool EnumerateParameters<TState>(IUriTemplateParser parser, ReadOnlySpan<char> uri, int initialCapacity, EnumerateParametersCallback<TState> callback, ref TState state)
    {
        ParameterCache cache = Rent(initialCapacity);
        if (parser.ParseUri(uri, HandleParameters, ref cache))
        {
            for (int i = 0; i < cache.written; ++i)
            {
                CacheEntry item = cache.items[i];
                callback(item.Name, item.Value, ref state);
            }

            cache.Return();
            return true;
        }

        cache.Return();
        return false;
    }

    /// <summary>
    /// Rent an instance of a parameter cache.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity of the cache.</param>
    /// <returns>An instance of a parameter cache.</returns>
    /// <remarks>When you have finished with the cache, call <see cref="Return()"/> to relinquish any internal resources.</remarks>
    private static ParameterCache Rent(int initialCapacity)
    {
        return new(initialCapacity);
    }

    /// <summary>
    /// A parameter handler for <see cref="IUriTemplateParser"/>.
    /// </summary>
    /// <param name="reset">Indicates whether to reset the parameter cache, ignoring any parameters that have been seen.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <param name="state">The parameter cache.</param>
    /// <remarks>Pass this to <see cref="IUriTemplateParser.ParseUri{TState}(ReadOnlySpan{char}, ParameterCallback{TState}, ref TState)"/>, as the callback.</remarks>
    private static void HandleParameters(bool reset, ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref ParameterCache state)
    {
        if (!reset)
        {
            state.Add(name, value);
        }
        else
        {
            state.Reset();
        }
    }

    /// <summary>
    /// Add a parameter to the cache.
    /// </summary>
    /// <param name="name">The name of the parameter to add.</param>
    /// <param name="value">The value of the parameter to add.</param>
    private void Add(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        char[] entryArray = name.Length + value.Length > 0 ? ArrayPool<char>.Shared.Rent(name.Length + value.Length) : Array.Empty<char>();
        name.CopyTo(entryArray);
        value.CopyTo(entryArray.AsSpan(name.Length));

        if (this.written == this.items.Length)
        {
            ArrayPool<CacheEntry>.Shared.Resize(ref this.items, this.items.Length + this.bufferIncrement);
        }

        this.items[this.written] = new(entryArray, name.Length, value.Length);
        this.written++;
    }

    /// <summary>
    /// Reset the items written.
    /// </summary>
    private void Reset()
    {
        this.ResetItems();
        this.written = 0;
    }

    /// <summary>
    /// Return the resources used by the cache.
    /// </summary>
    private void Return()
    {
        if (!this.returned)
        {
            this.ResetItems();
            ArrayPool<CacheEntry>.Shared.Return(this.items);
            this.returned = true;
        }
    }

    private void ResetItems()
    {
        for (int i = 0; i < this.written; ++i)
        {
            this.items[i].Return();
        }
    }

    private readonly struct CacheEntry
    {
        private readonly char[] entry;
        private readonly int nameLength;
        private readonly int valueLength;

        public CacheEntry(in char[] entry, int nameLength, int valueLength)
        {
            this.entry = entry;
            this.nameLength = nameLength;
            this.valueLength = valueLength;
        }

        public Span<char> Name => this.nameLength > 0 ? this.entry.AsSpan(0, this.nameLength) : Span<char>.Empty;

        public Span<char> Value => this.valueLength > 0 ? this.entry.AsSpan(this.nameLength, this.valueLength) : Span<char>.Empty;

        public void Return()
        {
            if (this.entry.Length > 0)
            {
                ArrayPool<char>.Shared.Return(this.entry);
            }
        }
    }
}