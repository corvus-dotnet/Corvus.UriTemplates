﻿// <copyright file="ParameterCache.cs" company="Endjin Limited">
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
    private bool rented = false;

    private ParameterCache(int initialCapacity)
    {
        this.bufferIncrement = initialCapacity;
        this.items = ArrayPool<CacheEntry>.Shared.Rent(initialCapacity);
        this.written = 0;
        this.rented = true;
    }

    /// <summary>
    /// Gets the parameter with the given name.
    /// </summary>
    /// <param name="name">The name of the parameter to acquire.</param>
    /// <returns>The value of the named parameter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A parameter of the given name was not found in the collection.</exception>
    internal readonly ReadOnlySpan<char> this[ReadOnlySpan<char> name]
    {
        get
        {
            for (int i = 0; i < this.written; ++i)
            {
                ref CacheEntry item = ref this.items[i];
                if (item.Name.Equals(name, StringComparison.Ordinal))
                {
                    return item.Value;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(name), "A parameter of the given name was not found.");
        }
    }

    /// <summary>
    /// Enumerate the parameters in the parser.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the callback.</typeparam>
    /// <param name="parser">The parser to use.</param>
    /// <param name="uri">The uri to parse.</param>
    /// <param name="initialCapacity">The initial cache size, which should be greater than or equal to the expected number of parameters.
    /// It also provides the increment for the cache size should it be exceeded.</param>
    /// <param name="callback">The callback to receive the enumerated parameters.</param>
    /// <param name="state">The state for the callback.</param>
    /// <returns><see langword="true"/> if the parser was successful, otherwise <see langword="false"/>.</returns>
    public static bool EnumerateParameters<TState>(IUriTemplateParser parser, ReadOnlySpan<char> uri, int initialCapacity, EnumerateParametersCallback<TState> callback, ref TState state)
    {
        ParameterCache cache = Rent(initialCapacity);
        try
        {
            if (parser.ParseUri(uri, HandleParameters, ref cache))
            {
                cache.EnumerateParameters(callback, ref state);
                return true;
            }
        }
        finally
        {
            cache.Return();
        }

        return false;
    }

    /// <summary>
    /// Rent an instance of a parameter cache.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity of the cache.</param>
    /// <returns>An instance of a parameter cache.</returns>
    /// <remarks>When you have finished with the cache, call <see cref="Return()"/> to relinquish any internal resources.</remarks>
    internal static ParameterCache Rent(int initialCapacity)
    {
        return new(initialCapacity);
    }

    /// <summary>
    /// Tries to match the URI using the parser.
    /// </summary>
    /// <param name="parser">The uri template parser with which to match.</param>
    /// <param name="uri">The URI to match.</param>
    /// <returns><see langword="true"/> if the uri matches the template.</returns>
    /// <remarks>
    /// The parameter cache will contain the matched parameters if the parser matched successfully.
    /// </remarks>
    internal bool TryMatch(IUriTemplateParser parser, ReadOnlySpan<char> uri)
    {
        return parser.ParseUri(uri, HandleParameters, ref this);
    }

    /// <summary>
    /// Try to get a parameter from the cache.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <returns><see langword="true"/> if the parameter exists, otherwise false.</returns>
    internal bool TryGetParameter(ReadOnlySpan<char> name, out ReadOnlySpan<char> value)
    {
        for (int i = 0; i < this.written; ++i)
        {
            ref CacheEntry item = ref this.items[i];
            if (item.Name.Equals(name, StringComparison.Ordinal))
            {
                value = item.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reset the items written.
    /// </summary>
    internal void Reset()
    {
        this.ResetItems();
        this.written = 0;
    }

    /// <summary>
    /// Enumerate the parameters in the cache.
    /// </summary>
    /// <typeparam name="TState">The type of the state for enumeration.</typeparam>
    /// <param name="callback">The callback that will be passed the parameters to enumerate.</param>
    /// <param name="state">The initial state.</param>
    internal void EnumerateParameters<TState>(EnumerateParametersCallback<TState> callback, ref TState state)
    {
        for (int i = 0; i < this.written; ++i)
        {
            ref CacheEntry item = ref this.items[i];
            callback(item.Name, item.Value, ref state);
        }
    }

    /// <summary>
    /// Return the resources used by the cache.
    /// </summary>
    internal void Return()
    {
        if (this.rented)
        {
            this.ResetItems();
            ArrayPool<CacheEntry>.Shared.Return(this.items);
            this.rented = false;
        }
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

        public ReadOnlySpan<char> Name => this.nameLength > 0 ? this.entry.AsSpan(0, this.nameLength) : Span<char>.Empty;

        public ReadOnlySpan<char> Value => this.valueLength > 0 ? this.entry.AsSpan(this.nameLength, this.valueLength) : Span<char>.Empty;

        public void Return()
        {
            if (this.entry.Length > 0)
            {
                ArrayPool<char>.Shared.Return(this.entry);
            }
        }
    }
}