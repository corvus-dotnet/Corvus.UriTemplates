// <copyright file="UriTemplateParameterExtraction.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

////using BenchmarkDotNet.Attributes;

////namespace Corvus.UriTemplates.Benchmarking;

/////// <summary>
/////// Construct elements from a JSON element.
/////// </summary>
////[MemoryDiagnoser]
////public class UriTemplateParameterExtraction
////{
////    private const string Uri = "http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId=123232323&hash=23ADE34FAE&callback=http%3A%2F%2Fexample.com%2Fcallback";
////    private const string UriTemplate = "http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId={parentRequestId}{&hash,callback}";
////    private static readonly Uri TavisUri = new(Uri);
////    private Tavis.UriTemplates.UriTemplate? tavisTemplate;
////    private TavisApi.UriTemplate? corvusTavisTemplate;
////    private IUriTemplateParser? corvusTemplate;

////    /// <summary>
////    /// Global setup.
////    /// </summary>
////    /// <returns>A <see cref="Task"/> which completes once cleanup is complete.</returns>
////    [GlobalSetup]
////    public Task GlobalSetup()
////    {
////        this.tavisTemplate = new(UriTemplate);
////        this.corvusTavisTemplate = new(UriTemplate);
////        this.corvusTemplate = UriTemplateParserFactory.CreateParser(UriTemplate);

////        // A manual warm-up
////        this.ExtractParametersTavis();
////        this.ExtractParametersCorvusTavis();
////        this.ExtractParametersCorvus();

////        return Task.CompletedTask;
////    }

////    /// <summary>
////    /// Global cleanup.
////    /// </summary>
////    /// <returns>A <see cref="Task"/> which completes once cleanup is complete.</returns>
////    [GlobalCleanup]
////    public Task GlobalCleanup()
////    {
////        return Task.CompletedTask;
////    }

////    /// <summary>
////    /// Extract parameters from a URI template using Tavis types.
////    /// </summary>
////    [Benchmark(Baseline = true)]
////    public void ExtractParametersTavis()
////    {
////        this.tavisTemplate!.GetParameters(TavisUri);
////    }

////    /// <summary>
////    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
////    /// </summary>
////    /// <returns>
////    /// A result, to ensure that the code under test does not get optimized out of existence.
////    /// </returns>
////    [Benchmark]
////    public IDictionary<string, object>? ExtractParametersCorvusTavis()
////    {
////        return this.corvusTavisTemplate!.GetParameters(TavisUri);
////    }

////    /// <summary>
////    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
////    /// </summary>
////    [Benchmark]
////    public void ExtractParametersCorvusTavisWithParameterCache()
////    {
////        int state = 0;

////        if (this.corvusTemplate!.EnumerateParameters(Uri, HandleParameters, ref state))
////        {
////            // We can use the state
////        }
////        else
////        {
////            // We can't use the state
////        }

////#pragma warning disable RCS1163 // Unused parameter.
////        static void HandleParameters(ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
////#pragma warning restore RCS1163 // Unused parameter.
////        {
////            state++;
////        }
////    }

////    /// <summary>
////    /// Extract parameters from a URI template using Corvus types.
////    /// </summary>
////    [Benchmark]
////    public void ExtractParametersCorvus()
////    {
////        int state = 0;
////        this.corvusTemplate!.ParseUri(Uri, HandleParameterMatching, ref state);

////#pragma warning disable RCS1163 // Unused parameter.
////        static void HandleParameterMatching(bool reset, ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
////#pragma warning restore RCS1163 // Unused parameter.
////        {
////            if (reset)
////            {
////                state = 0;
////            }
////            else
////            {
////                state++;
////            }
////        }
////    }
////}