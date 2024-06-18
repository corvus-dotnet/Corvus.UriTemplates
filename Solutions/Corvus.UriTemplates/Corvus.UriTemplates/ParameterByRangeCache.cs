// <copyright file="ParameterByRangeCache.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;
using CommunityToolkit.HighPerformance;

namespace Corvus.UriTemplates;

/// <summary>
/// A cache for parameters extracted from a URI template.
/// </summary>
internal struct ParameterByRangeCache
{
    private readonly int bufferIncrement;
    private CacheEntry[] items;
    private int written;
    private bool rented = false;

    private ParameterByRangeCache(int initialCapacity)
    {
        this.bufferIncrement = initialCapacity;
        this.items = ArrayPool<CacheEntry>.Shared.Rent(initialCapacity);
        this.written = 0;
        this.rented = true;
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
    public static bool EnumerateParameters<TState>(IUriTemplateParser parser, ReadOnlySpan<char> uri, int initialCapacity, EnumerateParametersCallbackWithRange<TState> callback, ref TState state)
    {
        ParameterByRangeCache cache = Rent(initialCapacity);
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
    internal static ParameterByRangeCache Rent(int initialCapacity)
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
    /// <param name="uriTemplate">The URI template from which this cache was built.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="range">The range of the parameter.</param>
    /// <returns><see langword="true"/> if the parameter exists, otherwise false.</returns>
    internal readonly bool TryGetParameter(ReadOnlySpan<char> uriTemplate, ReadOnlySpan<char> name, out Range range)
    {
        for (int i = 0; i < this.written; ++i)
        {
            ref CacheEntry item = ref this.items[i];
            if (uriTemplate[item.NameRange].Equals(name, StringComparison.Ordinal))
            {
                range = item.ValueRange;
                return true;
            }
        }

        range = default;
        return false;
    }

    /// <summary>
    /// Reset the items written.
    /// </summary>
    internal void Reset()
    {
        this.written = 0;
    }

    /// <summary>
    /// Enumerate the parameters in the cache.
    /// </summary>
    /// <typeparam name="TState">The type of the state for enumeration.</typeparam>
    /// <param name="callback">The callback that will be passed the parameters to enumerate.</param>
    /// <param name="state">The initial state.</param>
    internal readonly void EnumerateParameters<TState>(EnumerateParametersCallbackWithRange<TState> callback, ref TState state)
    {
        for (int i = 0; i < this.written; ++i)
        {
            ref CacheEntry item = ref this.items[i];
            callback(item.NameRange, item.ValueRange, ref state);
        }
    }

    /// <summary>
    /// Return the resources used by the cache.
    /// </summary>
    internal void Return()
    {
        if (this.rented)
        {
            ArrayPool<CacheEntry>.Shared.Return(this.items);
            this.rented = false;
        }
    }

    /// <summary>
    /// A parameter handler for <see cref="IUriTemplateParser"/>.
    /// </summary>
    /// <param name="reset">Indicates whether to reset the parameter cache, ignoring any parameters that have been seen.</param>
    /// <param name="nameRange">The range of the parameter name.</param>
    /// <param name="range">The range of the parameter.</param>
    /// <param name="state">The parameter cache.</param>
    /// <remarks>Pass this to <see cref="IUriTemplateParser.ParseUri{TState}(in ReadOnlySpan{char}, ParameterCallback{TState}, ref TState, in bool)"/>, as the callback.</remarks>
    private static void HandleParameters(bool reset, Range nameRange, Range range, ref ParameterByRangeCache state)
    {
        if (!reset)
        {
            state.Add(nameRange, range);
        }
        else
        {
            state.Reset();
        }
    }

    /// <summary>
    /// Add a parameter to the cache.
    /// </summary>
    /// <param name="nameRange">The range of the name of the parameter to add.</param>
    /// <param name="range">The range of the parameter to add.</param>
    private void Add(Range nameRange, Range range)
    {
        if (this.written == this.items.Length)
        {
            ArrayPool<CacheEntry>.Shared.Resize(ref this.items, this.items.Length + this.bufferIncrement);
        }

        this.items[this.written] = new(nameRange, range);
        this.written++;
    }

    private readonly struct CacheEntry(Range nameRange, Range valueRange)
    {
        public Range NameRange => nameRange;

        public Range ValueRange => valueRange;
    }
}