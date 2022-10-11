// <copyright file="UriTemplateParameterSetting.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Text.Json;
using BenchmarkDotNet.Attributes;

namespace Corvus.UriTemplates.Benchmarking;

/// <summary>
/// Construct elements from a JSON element.
/// </summary>
[MemoryDiagnoser]
public class UriTemplateParameterSetting
{
    private const string UriTemplate = "http://example.org/location{?value*}";
    private static readonly Dictionary<string, string> Value = new() { { "foo", "bar" }, { "bar", "baz" }, { "baz", "bob" } };
    private static readonly Dictionary<string, object?> Parameters = new() { { "value", Value } };

    private readonly JsonDocument jsonValues = JsonDocument.Parse("{\"value\": { \"foo\": \"bar\", \"bar\": \"baz\", \"baz\": \"bob\" }}");
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
        this.jsonValues.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Resolve a URI from a template and parameter values using Tavis.UriTemplate.
    /// </summary>
    [Benchmark(Baseline = true)]
    public void ResolveUriTavis()
    {
        this.tavisTemplate!.SetParameter("value", Value);
        this.tavisTemplate!.Resolve();
    }

    /// <summary>
    /// Resolve a URI from a template and parameter values using Corvus.UriTemplates.TavisApi.UriTemplate.
    /// </summary>
    [Benchmark]
    public void ResolveUriCorvusTavis()
    {
        this.corvusTavisTemplate!.SetParameter("value", Value);
        this.corvusTavisTemplate!.Resolve();
    }

    /// <summary>
    /// Resolve a URI from a template and parameter values using Corvus.UriTemplateResolver.
    /// </summary>
    [Benchmark]
    public void ResolveUriCorvusJson()
    {
        object? nullState = default;
        JsonUriTemplateResolver.TryResolveResult(UriTemplate.AsSpan(), false, this.jsonValues.RootElement, HandleResult, ref nullState);
        static void HandleResult(ReadOnlySpan<char> resolvedTemplate, ref object? state)
        {
            // NOP
        }
    }

    /// <summary>
    /// Resolve a URI from a template and parameter values using Corvus.UriTemplateResolver.
    /// </summary>
    [Benchmark]
    public void ResolveUriCorvusDictionary()
    {
        object? nullState = default;
        DictionaryUriTemplateResolver.TryResolveResult(UriTemplate.AsSpan(), false, Parameters, HandleResult, ref nullState);
        static void HandleResult(ReadOnlySpan<char> resolvedTemplate, ref object? state)
        {
            // NOP
        }
    }
}