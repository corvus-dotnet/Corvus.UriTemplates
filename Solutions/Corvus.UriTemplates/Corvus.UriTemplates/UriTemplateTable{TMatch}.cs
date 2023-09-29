// <copyright file="UriTemplateTable{TMatch}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Immutable;

namespace Corvus.UriTemplates;

/// <summary>
/// Parses a URI against a table of URI templates.
/// </summary>
/// <typeparam name="TMatch">The type of the value to be matched.</typeparam>
public sealed class UriTemplateTable<TMatch>
{
    private readonly ParserAndMatch[] entries;

    private UriTemplateTable(ParserAndMatch[] entries)
    {
        this.entries = entries;
    }

    /// <summary>
    /// Gets the number of entries in the table.
    /// </summary>
    public int Length => this.entries.Length;

    /// <summary>
    /// Try to match the uri against the URI templates in the table.
    /// </summary>
    /// <param name="uri">The URI to match.</param>
    /// <param name="match">The matched result.</param>
    /// <returns><see langword="true"/> if the URI matched a value in the table.</returns>
    /// <remarks>
    /// <para>
    /// This will find the first match in the table.
    /// </para>
    /// <para>
    /// While the <paramref name="match"/> result is <see cref="IDisposable"/> you need only dispose it if the method returned <see langword="true"/>.
    /// It is, however, safe to dispose in either case.
    /// </para>
    /// </remarks>
    public bool TryMatch(ReadOnlySpan<char> uri, out TemplateMatchResult<TMatch> match)
    {
        return this.TryMatch(uri, 10, out match);
    }

    /// <summary>
    /// Try to match the uri against the URI templates in the table.
    /// </summary>
    /// <param name="uri">The URI to match.</param>
    /// <param name="initialParameterCount">The initial parameter count for the match.
    /// Ensure that this is greater than or equal to the maximum expected number of parameters to avoid re-allocation overheads.</param>
    /// <param name="match">The matched result.</param>
    /// <returns><see langword="true"/> if the URI matched a value in the table.</returns>
    /// <remarks>
    /// <para>
    /// This will find the first match in the table.
    /// </para>
    /// <para>
    /// While the <paramref name="match"/> result is <see cref="IDisposable"/> you need only dispose it if the method returned <see langword="true"/>.
    /// It is, however, safe to dispose in either case.
    /// </para>
    /// </remarks>
    public bool TryMatch(ReadOnlySpan<char> uri, int initialParameterCount, out TemplateMatchResult<TMatch> match)
    {
        for (int i = 0; i < this.entries.Length; ++i)
        {
            ref ParserAndMatch entry = ref this.entries[i];
            if (entry.Parser.IsMatch(uri))
            {
                match = new(entry.Match, entry.Parser);
                return true;
            }
        }

        // No result, so return the cache.
        match = default;
        return false;
    }

    /// <summary>
    /// A <see cref="IUriTemplateParser"/> and the instance to provide when it is matched.
    /// </summary>
    /// <param name="Parser">The parser.</param>
    /// <param name="Match">The corresponding match.</param>
    private readonly record struct ParserAndMatch(IUriTemplateParser Parser, TMatch Match);

    /// <summary>
    /// A builder for a <see cref="UriTemplateTable{TMatch}"/>.
    /// </summary>
    public class Builder
    {
        private readonly List<ParserAndMatch> builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="Builder"/> struct.
        /// </summary>
        internal Builder()
        {
            this.builder = new List<ParserAndMatch>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Builder"/> struct.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the table.</param>
        /// <remarks>
        /// Provide the initial capacity of the table if known. This helps minimize
        /// the overhead of re-allocation.
        /// </remarks>
        internal Builder(int initialCapacity)
        {
            this.builder = new List<ParserAndMatch>(initialCapacity);
        }

        /// <summary>
        /// Gets the length of the table builder.
        /// </summary>
        public int Count => this.builder.Count;

        /// <summary>
        /// Add a uri template and its corresponding match.
        /// </summary>
        /// <param name="uriTemplate">The URI template to add.</param>
        /// <param name="match">The corresponding match to provide if the parser matches.</param>
        public void Add(string uriTemplate, TMatch match)
        {
            this.builder.Add(new(UriTemplateParserFactory.CreateParser(uriTemplate), match));
        }

        /// <summary>
        /// Add a uri template and its corresponding match.
        /// </summary>
        /// <param name="uriTemplate">The URI template to add.</param>
        /// <param name="match">The corresponding match to provide if the parser matches.</param>
        public void Add(ReadOnlySpan<char> uriTemplate, TMatch match)
        {
            this.builder.Add(new(UriTemplateParserFactory.CreateParser(uriTemplate), match));
        }

        /// <summary>
        /// Add a parser and its corresponding match.
        /// </summary>
        /// <param name="parser">The parser to add.</param>
        /// <param name="match">The corresponding match to provide if the parser matches.</param>
        public void Add(IUriTemplateParser parser, TMatch match)
        {
            this.builder.Add(new(parser, match));
        }

        /// <summary>
        /// Convert the builder into a table.
        /// </summary>
        /// <returns>The resulting table.</returns>
        public UriTemplateTable<TMatch> ToTable()
        {
            return new([.. this.builder]);
        }
    }
}