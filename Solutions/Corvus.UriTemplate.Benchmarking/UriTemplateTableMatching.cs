// <copyright file="UriTemplateTableMatching.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using BenchmarkDotNet.Attributes;

namespace Corvus.UriTemplates.Benchmarking;

/// <summary>
/// Matches tables.
/// </summary>
[MemoryDiagnoser]
public class UriTemplateTableMatching
{
    private const string Uri = "/baz/fod/blob";
    private static readonly Uri TavisUri = new(Uri, UriKind.RelativeOrAbsolute);
    private Tavis.UriTemplates.UriTemplateTable? tavisTemplateTable;
    private TavisApi.UriTemplateTable? corvusTavisTemplateTable;
    private UriTemplateTable<string>? corvusTemplateTable;

    /// <summary>
    /// Global setup.
    /// </summary>
    /// <returns>A <see cref="Task"/> which completes once cleanup is complete.</returns>
    [GlobalSetup]
    public Task GlobalSetup()
    {
        UriTemplateTable<string>.Builder builder = UriTemplateTable.CreateBuilder<string>();
        this.corvusTavisTemplateTable = new TavisApi.UriTemplateTable();
        this.tavisTemplateTable = new Tavis.UriTemplates.UriTemplateTable();

        this.corvusTavisTemplateTable.Add("root", new TavisApi.UriTemplate("/"));
        this.tavisTemplateTable.Add("root", new Tavis.UriTemplates.UriTemplate("/"));
        builder.Add("/", "root");

        for (int i = 0; i < 10_000; ++i)
        {
            string guid = Guid.NewGuid().ToString();
            string uri1 = $"/{guid}/{{bar}}";
            string uri2 = $"/baz/{guid}";
            string uri3 = $"/{{goo}}/{{bar}}/{guid}";

            this.corvusTavisTemplateTable.Add(guid, new TavisApi.UriTemplate(uri1));
            this.corvusTavisTemplateTable.Add($"baz_{guid}", new TavisApi.UriTemplate(uri2));
            this.corvusTavisTemplateTable.Add($"goo_{guid}", new TavisApi.UriTemplate(uri3));
            this.tavisTemplateTable.Add(guid, new Tavis.UriTemplates.UriTemplate(uri1));
            this.tavisTemplateTable.Add($"baz_{guid}", new Tavis.UriTemplates.UriTemplate(uri2));
            this.tavisTemplateTable.Add($"goo_{guid}", new Tavis.UriTemplates.UriTemplate(uri3));
            builder.Add(uri1, guid);
            builder.Add(uri2, $"baz_{guid}");
            builder.Add(uri3, $"goo_{guid}");
        }

        // Add the real matches
        this.corvusTavisTemplateTable.Add("blob", new TavisApi.UriTemplate("/baz/{bar}/blob"));
        this.tavisTemplateTable.Add("blob", new Tavis.UriTemplates.UriTemplate("/baz/{bar}/blob"));
        builder.Add("/baz/{bar}/blob", "blob");

        this.corvusTemplateTable = builder.ToTable();

        // Warm up to create all the parsers etc.
        this.MatchTavis();
        this.MatchCorvusTavis();
        this.MatchCorvus();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Global cleanup.
    /// </summary>
    /// <returns>A <see cref="Task"/> which completes once cleanup is complete.</returns>
    [GlobalCleanup]
    public Task GlobalCleanup()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Extract parameters from a URI template using Tavis types.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark(Baseline = true)]
    public bool MatchTavis()
    {
        Tavis.UriTemplates.TemplateMatch? result = this.tavisTemplateTable!.Match(TavisUri);
        return result?.Key is not null;
    }

    /// <summary>
    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public bool MatchCorvusTavis()
    {
        TavisApi.TemplateMatch? result = this.corvusTavisTemplateTable!.Match(TavisUri);
        return result?.Key is not null;
    }

    /// <summary>
    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public bool MatchCorvus()
    {
        if (this.corvusTemplateTable!.TryMatch(Uri, out TemplateMatchResult<string> match))
        {
            int count = 0;
            match.Parser.ParseUri(Uri, Count, ref count);
            return true;
        }

        return false;

        static void Count(bool reset, ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
        {
            if (reset)
            {
                state = 0;
            }
            else
            {
                state++;
            }
        }
    }
}