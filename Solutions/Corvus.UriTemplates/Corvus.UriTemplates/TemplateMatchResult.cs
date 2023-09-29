// <copyright file="TemplateMatchResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.UriTemplates;

/// <summary>
/// A result from matching a template in a template table.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public readonly struct TemplateMatchResult<T> : IDisposable
{
    private readonly ParameterCache parameterCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateMatchResult{T}"/> struct.
    /// </summary>
    /// <param name="result">The user-specified result for the match.</param>
    /// <param name="parameterCache">The parameter cache produced by the match.</param>
    internal TemplateMatchResult(T result, ParameterCache parameterCache)
    {
        this.Result = result;
        this.parameterCache = parameterCache;
    }

    /// <summary>
    /// Gets the result of the match.
    /// </summary>
    public T Result { get; }

    /// <summary>
    /// Gets the parameter with the given name.
    /// </summary>
    /// <param name="name">The name of the parameter to acquire.</param>
    /// <returns>The value of the named parameter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A parameter of the given name was not found in the collection.</exception>
    public readonly ReadOnlySpan<char> this[ReadOnlySpan<char> name]
    {
        get
        {
            return this.parameterCache[name];
        }
    }

    /// <summary>
    /// Enumerate the parameters in the cache.
    /// </summary>
    /// <typeparam name="TState">The type of the state for enumeration.</typeparam>
    /// <param name="callback">The callback that will be passed the parameters to enumerate.</param>
    /// <param name="state">The initial state.</param>
    public void EnumerateParameters<TState>(EnumerateParametersCallback<TState> callback, ref TState state)
    {
        this.parameterCache.EnumerateParameters(callback, ref state);
    }

    /// <summary>
    /// Try to get a parameter from the cache.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <returns><see langword="true"/> if the parameter exists, otherwise false.</returns>
    public bool TryGetParameter(ReadOnlySpan<char> name, out ReadOnlySpan<char> value)
    {
        return this.parameterCache.TryGetParameter(name, out value);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.parameterCache.Return();
    }
}