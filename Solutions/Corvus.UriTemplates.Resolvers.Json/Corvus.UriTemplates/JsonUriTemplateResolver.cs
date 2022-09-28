// <copyright file="JsonUriTemplateResolver.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Corvus.UriTemplates;

/// <summary>
/// A wrapper around <see cref="UriTemplateResolver{TParameterProvider, TParameterPayload}"/>
/// for a <see cref="JsonTemplateParameterProvider"/>.
/// </summary>
public static class JsonUriTemplateResolver
{
#if NET6_0
    private static readonly JsonTemplateParameterProvider ParameterProvider = new();
#endif

    /// <summary>
    /// Resolve the template into an output result.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the callback.</typeparam>
    /// <param name="template">The template to resolve.</param>
    /// <param name="resolvePartially">If <see langword="true"/> then partially resolve the result.</param>
    /// <param name="parameters">The parameters to apply to the template.</param>
    /// <param name="parameterNameCallback">An optional callback which is provided each parameter name as they are discovered.</param>
    /// <param name="callback">The callback which is provided with the resolved template.</param>
    /// <param name="state">The state passed to the callback(s).</param>
    /// <returns><see langword="true"/> if the URI matched the template, and the parameters were resolved successfully.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryResolveResult<TState>(ReadOnlySpan<char> template, bool resolvePartially, in JsonElement parameters, ParameterNameCallback<TState>? parameterNameCallback, ResolvedUriTemplateCallback<TState> callback, ref TState state)
    {
#if NET6_0
        return UriTemplateResolver<JsonTemplateParameterProvider, JsonElement>.TryResolveResult(ParameterProvider, template, resolvePartially, parameters, callback, parameterNameCallback, ref state);
#else
        return UriTemplateResolver<JsonTemplateParameterProvider, JsonElement>.TryResolveResult(template, resolvePartially, parameters, callback, parameterNameCallback, ref state);
#endif
    }

    /// <summary>
    /// Resolve the template into an output result.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the callback.</typeparam>
    /// <param name="template">The template to resolve.</param>
    /// <param name="resolvePartially">If <see langword="true"/> then partially resolve the result.</param>
    /// <param name="parameters">The parameters to apply to the template.</param>
    /// <param name="callback">The callback which is provided with the resolved template.</param>
    /// <param name="state">The state passed to the callback(s).</param>
    /// <returns><see langword="true"/> if the URI matched the template, and the parameters were resolved successfully.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryResolveResult<TState>(ReadOnlySpan<char> template, bool resolvePartially, in JsonElement parameters, ResolvedUriTemplateCallback<TState> callback, ref TState state)
    {
#if NET6_0
        return UriTemplateResolver<JsonTemplateParameterProvider, JsonElement>.TryResolveResult(ParameterProvider, template, resolvePartially, parameters, callback, null, ref state);
#else
        return UriTemplateResolver<JsonTemplateParameterProvider, JsonElement>.TryResolveResult(template, resolvePartially, parameters, callback, null, ref state);
#endif
    }

    /// <summary>
    /// Resolve the template into an output result.
    /// </summary>
    /// <param name="template">The template to resolve.</param>
    /// <param name="output">The output buffer into which to resolve the template.</param>
    /// <param name="resolvePartially">If <see langword="true"/> then partially resolve the result.</param>
    /// <param name="parameters">The parameters to apply to the template.</param>
    /// <returns><see langword="true"/> if the URI matched the template, and the parameters were resolved successfully.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryResolveResult(ReadOnlySpan<char> template, IBufferWriter<char> output, bool resolvePartially, in JsonElement parameters)
    {
        object? nullState = default;
#if NET6_0
        return UriTemplateResolver<JsonTemplateParameterProvider, JsonElement>.TryResolveResult(ParameterProvider, template, output, resolvePartially, parameters, null, ref nullState);
#else
        return UriTemplateResolver<JsonTemplateParameterProvider, JsonElement>.TryResolveResult(template, output, resolvePartially, parameters, null, ref nullState);
#endif
    }

    /// <summary>
    /// Get the parameter names from the template.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the callback.</typeparam>
    /// <param name="template">The template for the callback.</param>
    /// <param name="callback">The callback provided with the parameter names.</param>
    /// <param name="state">The state for the callback.</param>
    /// <returns><see langword="true"/> if the URI matched the template, and the parameters were resolved successfully.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetParameterNames<TState>(ReadOnlySpan<char> template, ParameterNameCallback<TState> callback, ref TState state)
    {
#if NET6_0
        return UriTemplateResolver<JsonTemplateParameterProvider, JsonElement>.TryResolveResult(ParameterProvider, template, true, default, Nop, callback, ref state);
#else
        return UriTemplateResolver<JsonTemplateParameterProvider, JsonElement>.TryResolveResult(template, true, default, Nop, callback, ref state);
#endif

        static void Nop(ReadOnlySpan<char> value, ref TState state)
        {
        }
    }
}