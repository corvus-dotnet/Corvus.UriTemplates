// <copyright file="UriTemplateParameterExtraction.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using BenchmarkDotNet.Attributes;

namespace Corvus.UriTemplates.Benchmarking;

/// <summary>
/// Construct elements from a JSON element.
/// </summary>
[MemoryDiagnoser]
public class UriTemplateParameterExtraction
{
    private const string UriWithQuery = "http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId=123232323&hash=23ADE34FAE&callback=http%3A%2F%2Fexample.com%2Fcallback";
    private const string UriTemplateQuery = "http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId={parentRequestId}{&hash,callback}";
    private const string UriWithMultiplePathSegments = "http://example.com/foo/bar/Glimpse.axd";
    private const string UriTemplateNonExplodedPathSegments = "http://example.com/{foo}/{bar}/Glimpse.axd";
    private const string UriTemplateExplodedPathSegment = "http://example.com/{/foo*}/Glimpse.axd";
    private static readonly Uri TavisUriWithQuery = new(UriWithQuery);
    private static readonly Uri TavisUriWithMultiplePathSegments = new(UriTemplateQuery);
    private Tavis.UriTemplates.UriTemplate? tavisQueryTemplate;
    private Tavis.UriTemplates.UriTemplate? tavisNonExplodedPathSegmentsTemplate;
    private TavisApi.UriTemplate? corvusTavisQueryTemplate;
    private TavisApi.UriTemplate? corvusTavisNonExplodedPathSegmentsTemplate;
    private IUriTemplateParser? corvusQueryTemplate;
    private IUriTemplateParser? corvusNonExplodedPathSegmentsTemplate;
    private IUriTemplateParser? corvusExplodedPathSegmentTemplate;

    /// <summary>
    /// Global setup.
    /// </summary>
    /// <returns>A <see cref="Task"/> which completes once cleanup is complete.</returns>
    [GlobalSetup]
    public Task GlobalSetup()
    {
        this.tavisQueryTemplate = new(UriTemplateQuery);
        this.tavisNonExplodedPathSegmentsTemplate = new(UriTemplateNonExplodedPathSegments);
        this.corvusTavisQueryTemplate = new(UriTemplateQuery);
        this.corvusTavisNonExplodedPathSegmentsTemplate = new(UriTemplateNonExplodedPathSegments);
        this.corvusQueryTemplate = UriTemplateParserFactory.CreateParser(UriTemplateQuery);
        this.corvusNonExplodedPathSegmentsTemplate = UriTemplateParserFactory.CreateParser(UriTemplateNonExplodedPathSegments);
        this.corvusExplodedPathSegmentTemplate = UriTemplateParserFactory.CreateParser(UriTemplateExplodedPathSegment);

        // A manual warm-up
        this.ExtractQueryParametersTavis();
        this.ExtractPathParametersTavis();
        this.ExtractQueryParametersCorvusTavis();
        this.ExtractPathParametersCorvusTavis();
        this.ExtractQueryParametersCorvus();
        this.ExtractPathParametersCorvus();
        this.ExtractExplodedPathParameterCorvus();

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
    /// Extract query parameters with a URI template using Tavis types.
    /// </summary>
    [Benchmark(Baseline = true)]
    public void ExtractQueryParametersTavis()
    {
        this.tavisQueryTemplate!.GetParameters(TavisUriWithQuery);
    }

    /// <summary>
    /// Extract path parameters from a URI template using Tavis types.
    /// </summary>
    [Benchmark]
    public void ExtractPathParametersTavis()
    {
        this.tavisQueryTemplate!.GetParameters(TavisUriWithMultiplePathSegments);
    }

    /// <summary>
    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public IDictionary<string, object>? ExtractQueryParametersCorvusTavis()
    {
        return this.corvusTavisQueryTemplate!.GetParameters(TavisUriWithQuery);
    }

    /// <summary>
    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public IDictionary<string, object>? ExtractPathParametersCorvusTavis()
    {
        return this.corvusTavisNonExplodedPathSegmentsTemplate!.GetParameters(TavisUriWithMultiplePathSegments);
    }

    /// <summary>
    /// Extract query parameters from a URI template using the Corvus implementation with parameter caching.
    /// </summary>
    [Benchmark]
    public void ExtractQueryParametersCorvusWithParameterCache()
    {
        int state = 0;

        if (this.corvusQueryTemplate!.EnumerateParameters(UriWithQuery, HandleParameters, ref state))
        {
            // We can use the state
        }
        else
        {
            // We can't use the state
        }

#pragma warning disable RCS1163 // Unused parameter.
        static void HandleParameters(ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
#pragma warning restore RCS1163 // Unused parameter.
        {
            state++;
        }
    }

    /// <summary>
    /// Extract individual path parameters from a URI template using the Corvus implementation with parameter caching.
    /// </summary>
    [Benchmark]
    public void ExtractPathParametersCorvusWithParameterCache()
    {
        int state = 0;

        if (this.corvusNonExplodedPathSegmentsTemplate!.EnumerateParameters(UriWithMultiplePathSegments, HandleParameters, ref state))
        {
            // We can use the state
        }
        else
        {
            // We can't use the state
        }

#pragma warning disable RCS1163 // Unused parameter.
        static void HandleParameters(ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
#pragma warning restore RCS1163 // Unused parameter.
        {
            state++;
        }
    }

    /// <summary>
    /// Extract individual path parameters from a URI template using the Corvus implementation with parameter caching.
    /// </summary>
    [Benchmark]
    public void ExtractExplodedPathParameterCorvusWithParameterCache()
    {
        int state = 0;

        if (this.corvusExplodedPathSegmentTemplate!.EnumerateParameters(UriWithMultiplePathSegments, HandleParameters, ref state))
        {
            // We can use the state
        }
        else
        {
            // We can't use the state
        }

#pragma warning disable RCS1163 // Unused parameter.
        static void HandleParameters(ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
#pragma warning restore RCS1163 // Unused parameter.
        {
            state++;
        }
    }

    /// <summary>
    /// Extract query parameters from a URI template using Corvus types.
    /// </summary>
    [Benchmark]
    public void ExtractQueryParametersCorvus()
    {
        int state = 0;
        this.corvusQueryTemplate!.ParseUri(UriWithQuery, HandleParameterMatching, ref state);

#pragma warning disable RCS1163 // Unused parameter.
        static void HandleParameterMatching(bool reset, ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
#pragma warning restore RCS1163 // Unused parameter.
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

    /// <summary>
    /// Extract parameters from a URI template using Corvus types.
    /// </summary>
    [Benchmark]
    public void ExtractPathParametersCorvus()
    {
        int state = 0;
        this.corvusNonExplodedPathSegmentsTemplate!.ParseUri(UriWithMultiplePathSegments, HandleParameterMatching, ref state);

#pragma warning disable RCS1163 // Unused parameter.
        static void HandleParameterMatching(bool reset, ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
#pragma warning restore RCS1163 // Unused parameter.
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

    /// <summary>
    /// Extract parameters from a URI template using Corvus types.
    /// </summary>
    [Benchmark]
    public void ExtractExplodedPathParameterCorvus()
    {
        int state = 0;
        this.corvusExplodedPathSegmentTemplate!.ParseUri(UriWithMultiplePathSegments, HandleParameterMatching, ref state);

#pragma warning disable RCS1163 // Unused parameter.
        static void HandleParameterMatching(bool reset, ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
#pragma warning restore RCS1163 // Unused parameter.
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