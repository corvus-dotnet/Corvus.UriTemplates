﻿// <copyright file="UriTemplateParameterSetting.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using BenchmarkDotNet.Attributes;
using Corvus.Json;

namespace Corvus.UriTemplates.Benchmarking;

/// <summary>
/// Construct elements from a JSON element.
/// </summary>
[MemoryDiagnoser]
public class UriTemplateParameterSetting
{
    private const string UriTemplate = "http://example.org/location{?value*}";
    private static readonly Dictionary<string, string> Values = new() { { "foo", "bar" }, { "bar", "baz" }, { "baz", "bob" } };
    private static readonly JsonAny JsonValues = JsonAny.FromProperties(("foo", "bar"), ("bar", "baz"), ("baz", "bob")).AsJsonElementBackedValue();

    private Tavis.UriTemplates.UriTemplate? tavisTemplate;
    private TavisApi.UriTemplate? corvusTavisTemplate;

    /// <summary>
    /// Global setup.
    /// </summary>
    /// <returns>A <see cref="Task"/> which completes once cleanup is complete.</returns>
    [GlobalSetup]
    public Task GlobalSetup()
    {
        this.tavisTemplate = new(UriTemplate);
        this.corvusTavisTemplate = new(UriTemplate);
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
    /// Resolve a URI from a template and parameter values using Tavis.UriTemplate.
    /// </summary>
    [Benchmark(Baseline = true)]
    public void ResolveUriTavis()
    {
        this.tavisTemplate!.SetParameter("value", Values);
        this.tavisTemplate!.Resolve();
    }

    /// <summary>
    /// Resolve a URI from a template and parameter values using Corvus.UriTemplates.TavisApi.UriTemplate.
    /// </summary>
    [Benchmark]
    public void ResolveUriCorvusTavis()
    {
        this.corvusTavisTemplate!.SetParameter("value", Values);
        this.corvusTavisTemplate!.Resolve();
    }

    /// <summary>
    /// Resolve a URI from a template and parameter values using Corvus.UriTemplateResolver.
    /// </summary>
    [Benchmark]
    public void ResolveUriCorvus()
    {
        object? nullState = default;
        JsonUriTemplateResolver.TryResolveResult(UriTemplate.AsSpan(), false, JsonValues, HandleResult, ref nullState);
        static void HandleResult(ReadOnlySpan<char> resolvedTemplate, ref object? state)
        {
            // NOP
        }
    }
}