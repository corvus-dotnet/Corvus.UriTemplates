// <copyright file="IUriTemplateParser.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.UriTemplates;

/// <summary>
/// A callback when a parameter is found, in which the match is identified with a <see cref="ReadOnlySpan{Char}"/>.
/// </summary>
/// <typeparam name="TState">The type of the state to pass.</typeparam>
/// <param name="reset">Whether to reset the parameters that we have seen so far.</param>
/// <param name="name">The name of the parameter.</param>
/// <param name="value">The string representation of the parameter.</param>
/// <param name="state">The state to pass.</param>
public delegate void ParameterCallback<TState>(bool reset, ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref TState state);

/// <summary>
/// A callback when a parameter is found, in which the match is identified with a <see cref="Range"/>.
/// </summary>
/// <typeparam name="TState">The type of the state to pass.</typeparam>
/// <param name="reset">Whether to reset the parameters that we have seen so far.</param>
/// <param name="nameHandle">Identifies the parameter name.</param>
/// <param name="valueRange">The range in the input URI string at which the parameter was found.</param>
/// <param name="state">The state to pass.</param>
public delegate void ParameterCallbackWithRange<TState>(bool reset, ParameterNameHandle nameHandle, Range valueRange, ref TState state);

/// <summary>
/// The interface implemented by an URI parser.
/// </summary>
public interface IUriTemplateParser
{
    // NEXT TIME 2024/06/18: implement this and all its consequences.

    /// <summary>
    /// Provides access to a <see cref="ReadOnlySpan{Char}"/> containing a parameter's name.
    /// </summary>
    /// <typeparam name="TState">Callback input type.</typeparam>
    /// <typeparam name="TResult">Callback result type.</typeparam>
    /// <param name="parameterNameHandle">Handle identifying the parameter.</param>
    /// <param name="process">The method that will receive the span.</param>
    /// <param name="state">An argument providing whatever state the callback requires.</param>
    /// <returns>The return value of the callback.</returns>
    /// <remarks>
    /// This enables us to avoid allocating individual strings for parameter names, while
    /// still allowing the application to discover parameter names dynamically if required.
    /// </remarks>
    TResult ProcessParameterName<TState, TResult>(
        ParameterNameHandle parameterNameHandle,
        ReadOnlySpanCallback<char, TState, TResult> process,
        TState state);

    /// <summary>
    /// Determines if the UriTemplate matches the given URI.
    /// </summary>
    /// <param name="uri">The URI to match.</param>
    /// <param name="requiresRootedMatch">If true, then the template requires a rooted match and will not ignore prefixes. This is more efficient when using a fully-qualified template.</param>
    /// <returns><see langword="true"/> if the template is a match for the URI.</returns>
    bool IsMatch(in ReadOnlySpan<char> uri, bool requiresRootedMatch = false);

    /// <summary>
    /// Parses the given URI, calling your parameter callback for each named parameter discovered.
    /// </summary>
    /// <typeparam name="TState">The type of the state to pass.</typeparam>
    /// <param name="uri">The URI to parse.</param>
    /// <param name="parameterCallback">Called by the parser for each parameter that is discovered.</param>
    /// <param name="state">The state to pass to the callback.</param>
    /// <param name="requiresRootedMatch">If true, then the template requires a rooted match and will not ignore prefixes. This is more efficient when using a fully-qualified template.</param>
    /// <returns><see langword="true"/> if the uri was successfully parsed, otherwise false.</returns>
    /// <remarks>
    /// <para>
    /// This is a low-allocation operation, but you should take care with your implementation of your
    /// <see cref="ParameterCallback{T}"/> if you wish to minimize allocation in your call tree.
    /// </para>
    /// <para>
    /// The parameter callbacks occur as the parameters are matched. If the parse operation ultimately fails,
    /// those parameters are invalid, and should be disregarded.
    /// </para>
    /// </remarks>
    bool ParseUri<TState>(in ReadOnlySpan<char> uri, ParameterCallback<TState> parameterCallback, ref TState state, in bool requiresRootedMatch = false);

    /// <summary>
    /// Parses the given URI, calling your parameter callback for each named parameter discovered.
    /// </summary>
    /// <typeparam name="TState">The type of the state to pass.</typeparam>
    /// <param name="uri">The URI to parse.</param>
    /// <param name="parameterCallback">Called by the parser for each parameter that is discovered.</param>
    /// <param name="state">The state to pass to the callback.</param>
    /// <param name="requiresRootedMatch">If true, then the template requires a rooted match and will not ignore prefixes. This is more efficient when using a fully-qualified template.</param>
    /// <returns><see langword="true"/> if the uri was successfully parsed, otherwise false.</returns>
    /// <remarks>
    /// <para>
    /// This is a low-allocation operation, but you should take care with your implementation of your
    /// <see cref="ParameterCallback{T}"/> if you wish to minimize allocation in your call tree.
    /// </para>
    /// <para>
    /// The parameter callbacks occur as the parameters are matched. If the parse operation ultimately fails,
    /// those parameters are invalid, and should be disregarded.
    /// </para>
    /// </remarks>
    /// <exception cref="NotImplementedException">
    /// This method was added in 1.3, so libraries that depend on an older version, and which implement this
    /// interface will not have this method available. In most cases, the implementation of this interface
    /// will be supplied by this library, and so all methods will be available, but it is virtual to support
    /// the rare case where someone has implemented their own version against an older version of the library.
    /// This exception will be thrown if that is the case.
    /// </exception>
#if NET8_0_OR_GREATER
    virtual bool ParseUri<TState>(in ReadOnlySpan<char> uri, ParameterCallbackWithRange<TState> parameterCallback, ref TState state, in bool requiresRootedMatch = false) =>
        throw new NotImplementedException();
#else
    bool ParseUri<TState>(in ReadOnlySpan<char> uri, ParameterCallbackWithRange<TState> parameterCallback, ref TState state, in bool requiresRootedMatch = false);
#endif

#if !NET8_0_OR_GREATER
    /// <summary>
    /// Determines if the UriTemplate matches the given URI.
    /// </summary>
    /// <param name="uri">The URI to match.</param>
    /// <param name="requiresRootedMatch">If true, then the template requires a rooted match and will not ignore prefixes. This is more efficient when using a fully-qualified template.</param>
    /// <returns><see langword="true"/> if the template is a match for the URI.</returns>
    public bool IsMatch(string uri, bool requiresRootedMatch = false);

    /// <summary>
    /// Parses the given URI, calling your parameter callback for each named parameter discovered.
    /// </summary>
    /// <typeparam name="TState">The type of the state to pass.</typeparam>
    /// <param name="uri">The URI to parse.</param>
    /// <param name="parameterCallback">Called by the parser for each parameter that is discovered.</param>
    /// <param name="state">The state to pass to the callback.</param>
    /// <param name="requiresRootedMatch">If true, then the template requires a rooted match and will not ignore prefixes. This is more efficient when using a fully-qualified template.</param>
    /// <returns><see langword="true"/> if the uri was successfully parsed, otherwise false.</returns>
    /// <remarks>
    /// <para>
    /// This is a low-allocation operation, but you should take care with your implementation of your
    /// <see cref="ParameterCallback{T}"/> if you wish to minimize allocation in your call tree.
    /// </para>
    /// <para>
    /// The parameter callbacks occur as the parameters are matched. If the parse operation ultimately fails,
    /// those parameters are invalid, and should be disregarded.
    /// </para>
    /// </remarks>
    bool ParseUri<TState>(string uri, ParameterCallback<TState> parameterCallback, ref TState state, in bool requiresRootedMatch = false);

    /// <summary>
    /// Parses the given URI, calling your parameter callback for each named parameter discovered.
    /// </summary>
    /// <typeparam name="TState">The type of the state to pass.</typeparam>
    /// <param name="uri">The URI to parse.</param>
    /// <param name="parameterCallback">Called by the parser for each parameter that is discovered.</param>
    /// <param name="state">The state to pass to the callback.</param>
    /// <param name="requiresRootedMatch">If true, then the template requires a rooted match and will not ignore prefixes. This is more efficient when using a fully-qualified template.</param>
    /// <returns><see langword="true"/> if the uri was successfully parsed, otherwise false.</returns>
    /// <remarks>
    /// <para>
    /// This is a low-allocation operation, but you should take care with your implementation of your
    /// <see cref="ParameterCallback{T}"/> if you wish to minimize allocation in your call tree.
    /// </para>
    /// <para>
    /// The parameter callbacks occur as the parameters are matched. If the parse operation ultimately fails,
    /// those parameters are invalid, and should be disregarded.
    /// </para>
    /// </remarks>
    bool ParseUri<TState>(string uri, ParameterCallbackWithRange<TState> parameterCallback, ref TState state, in bool requiresRootedMatch = false);
#endif
}

/// <summary>
/// An opaque handle identifying a parameter name.
/// </summary>
public readonly struct ParameterNameHandle
{
    /// <summary>
    /// Creates a <see cref="ParameterNameHandle"/>.
    /// </summary>
    /// <param name="index">Parameter position.</param>
    internal ParameterNameHandle(int index)
    {
        this.Index = index;
    }

    /// <summary>
    /// Gets the position of this parameter.
    /// </summary>
    internal int Index { get; }
}