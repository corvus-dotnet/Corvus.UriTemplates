// <copyright file="ParameterCache.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;
using CommunityToolkit.HighPerformance;

namespace Corvus.UriTemplates;

/// <summary>
/// A cache for parameters extracted from a URI template.
/// </summary>
public struct ParameterCache
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
    /// A callback for enumerating parameters from the cache.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    public delegate void ParameterCacheCallback(ReadOnlySpan<char> name, ReadOnlySpan<char> value);

    /// <summary>
    /// Rent an instance of a parameter cache.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity of the cache.</param>
    /// <returns>An instance of a parameter cache.</returns>
    /// <remarks>When you have finished with the cache, call <see cref="Return()"/> to relinquish any internal resources.</remarks>
    public static ParameterCache Rent(int initialCapacity)
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
    public static void HandleParameters(bool reset, ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref ParameterCache state)
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
    /// Reset the items written.
    /// </summary>
    public void Reset()
    {
        this.ResetItems();
        this.written = 0;
    }

    /// <summary>
    /// Enumerate the parameters in the cache.
    /// </summary>
    /// <param name="callback">The callback to recieve the enumerated parameters.</param>
    public void EnumerateParameters(ParameterCacheCallback callback)
    {
        for (int i = 0; i < this.written; ++i)
        {
            CacheEntry item = this.items[i];
            callback(item.Name.AsSpan(0, item.NameLength), item.Value.AsSpan(0, item.ValueLength));
        }
    }

    /// <summary>
    /// Add a parameter to the cache.
    /// </summary>
    /// <param name="name">The name of the parameter to add.</param>
    /// <param name="value">The value of the parameter to add.</param>
    public void Add(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        char[] nameArray = name.Length > 0 ? ArrayPool<char>.Shared.Rent(name.Length) : Array.Empty<char>();
        name.CopyTo(nameArray);
        char[] valueArray = value.Length > 0 ? ArrayPool<char>.Shared.Rent(value.Length) : Array.Empty<char>();
        value.CopyTo(valueArray);

        if (this.written == this.items.Length)
        {
            ArrayPool<CacheEntry>.Shared.Resize(ref this.items, this.items.Length + this.bufferIncrement);
        }

        this.items[this.written] = new(nameArray, name.Length, valueArray, value.Length);
        this.written++;
    }

    /// <summary>
    /// Return the resources used by the cache.
    /// </summary>
    public void Return()
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
            CacheEntry item = this.items[i];
            if (item.Name.Length > 0)
            {
                ArrayPool<char>.Shared.Return(item.Name, true);
            }

            if (item.Value.Length > 0)
            {
                ArrayPool<char>.Shared.Return(item.Value, true);
            }
        }
    }

    private readonly struct CacheEntry
    {
        public CacheEntry(in char[] name, int nameLength, in char[] value, int valueLength)
        {
            this.Name = name;
            this.NameLength = nameLength;
            this.Value = value;
            this.ValueLength = valueLength;
        }

        public char[] Name { get; }

        public int NameLength { get; }

        public char[] Value { get; }

        public int ValueLength { get; }

        public static implicit operator (char[] Name, int NameLength, char[] Value, int ValueLength)(CacheEntry value)
        {
            return (value.Name, value.NameLength, value.Value, value.ValueLength);
        }

        public static implicit operator CacheEntry((char[] Name, int NameLength, char[] Value, int ValueLength) value)
        {
            return new CacheEntry(value.Name, value.NameLength, value.Value, value.ValueLength);
        }
    }
}