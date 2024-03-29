﻿// <copyright file="UriTemplateParserExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.UriTemplates;

/// <summary>
/// A callback for enumerating parameters from the cache.
/// </summary>
/// <typeparam name="TState">The type of the state for the callback.</typeparam>
/// <param name="name">The name of the parameter.</param>
/// <param name="value">The value of the parameter.</param>
/// <param name="state">The state for the callback.</param>
public delegate void EnumerateParametersCallback<TState>(ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref TState state);

/// <summary>
/// Extension methods for <see cref="IUriTemplateParser"/>.
/// </summary>
public static class UriTemplateParserExtensions
{
    /// <summary>
    /// Enumerate the parameters in the parser.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the callback.</typeparam>
    /// <param name="parser">The parser to use.</param>
    /// <param name="uri">The uri to parse.</param>
    /// <param name="callback">The callback to receive the enumerated parameters.</param>
    /// <param name="state">The state for the callback.</param>
    /// <param name="initialCapacity">The initial cache size, which should be greater than or equal to the expected number of parameters.
    /// It also provides the increment for the cache size should it be exceeded.</param>
    /// <returns><see langword="true"/> if the parser was successful, otherwise <see langword="false"/>.</returns>
    public static bool EnumerateParameters<TState>(this IUriTemplateParser parser, ReadOnlySpan<char> uri, EnumerateParametersCallback<TState> callback, ref TState state, int initialCapacity = 10)
    {
        return ParameterCache.EnumerateParameters(parser, uri, initialCapacity, callback, ref state);
    }
}