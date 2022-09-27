# Corvus.UriTemplates
Low-allocation URI Template parsing and resolution, supporting the [Tavis.UriTemplates](https://github.com/tavis-software/Tavis.UriTemplates) API

This is a netstandard2.1 and net7.0+ implementation of the [URI Template Spec RFC6570](http://tools.ietf.org/html/rfc6570). 

The library implements Level 4 compliance and is tested against test cases from [UriTemplate test suite](https://github.com/uri-templates/uritemplate-test).

## Introduction

This library provides tools for low-allocation URI Template parameter extraction (via `IUriTemplateParser`) and URI Template resolution (via `UriTemplateResolver`).

We then implement a drop-in replacement for the API supported by [Tavis.UriTemplates](https://github.com/tavis-software/Tavis.UriTemplates), with lower allocations and higher performance.

## Performance

There is a standard benchmark testing basic parameter extraction and resolution for the original Tavis.UriTemplate, the updated Corvus.UriTemplates.TavisApi.UriTemplate and the underlying zero-allocation URI template parser.

As you can see, there is a significant benefit to using the Corvus implementation, even without dropping down the low-level zero allocation API.

### Apply parameters to a URI template to resolve a URI
|                Method |     Mean | Error | Ratio |   Gen0 | Allocated | Alloc Ratio |
|---------------------- |---------:|------:|------:|-------:|----------:|------------:|
|       ResolveUriTavis | 694.0 ns |    NA |  1.00 | 0.4377 |    1832 B |        1.00 |
| ResolveUriCorvusTavis | 640.5 ns |    NA |  0.92 | 0.0515 |     216 B |        0.12 |
|      ResolveUriCorvus | 214.9 ns |    NA |  0.31 |      - |         - |        0.00 |

### Extract parameters from a URI by using a URI template
|                       Method |     Mean | Error | Ratio |   Gen0 | Allocated | Alloc Ratio |
|----------------------------- |---------:|------:|------:|-------:|----------:|------------:|
|       ExtractParametersTavis | 980.6 ns |    NA |  1.00 | 0.2613 |    1096 B |        1.00 |
| ExtractParametersCorvusTavis | 495.2 ns |    NA |  0.50 | 0.1450 |     608 B |        0.55 |
|      ExtractParametersCorvus | 174.6 ns |    NA |  0.18 |      - |         - |        0.00 |

## Parameter Extraction

### Using the Tavis API

```csharp
UriTemplate template = new("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId={parentRequestId}{&hash,callback}");

Uri uri = new ("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId=123232323&hash=23ADE34FAE&callback=http%3A%2F%2Fexample.com%2Fcallback");

IDictionary<string, object?>? result = template.GetParameters(uri);
```

### Using the low-allocation API directly

The lowest-level access makes use of a callback, which is fed the parameters as they are found.

If the `reset` flag is set, you should disregard any parameters that have previously been sent, and start again. (This is typically the case where a partial match fails, and is restarted.)

In order to manage the cache/reset process for you, we provide a `ParameterCache` type. You can rent an instance, and use it to accumulate the results for you. You can then enumerate the result set, and return the resource that have been rented for you.

```csharp
var state = ParameterCache.Rent(5);
IUriTemplateParser corvusTemplate = UriTemplateParserFactory.CreateParser("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId={parentRequestId}{&hash,callback}");

corvusTemplate!.ParseUri("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId=123232323&hash=23ADE34FAE&callback=http%3A%2F%2Fexample.com%2Fcallback", ParameterCache.HandleParameters, ref state);

state.EnumerateParameters(HandleFinalParameterSet);

state.Return();

void HandleFinalParameterSet(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
{
    if (name.SequenceEqual("parentRequestId"))
    {
        Assert.True(value.SequenceEqual("123232323"), $"parentRequestId was {value}");
        count++;
    }
    else if (name.SequenceEqual("hash"))
    {
        Assert.True(value.SequenceEqual("23ADE34FAE"), $"hash was {value}");
        count++;
    }
    else if (name.SequenceEqual("callback"))
    {
        Assert.True(value.SequenceEqual("http%3A%2F%2Fexample.com%2Fcallback"), $"callback was {value}");
        count++;
    }
    else
    {
        Assert.True(false, $"Unexpected parameter: (name: '{name}', value: '{value}')");
    }
}
```


## URI Resolution

### Using the Tavis API

Replacing a path segment parameter,

```csharp
[Fact]
public void UpdatePathParameter()
{
    var url = new UriTemplate("http://example.org/{tenant}/customers")
        .AddParameter("tenant", "acmé")
        .Resolve();

    Assert.Equal("http://example.org/acm%C3%A9/customers", url);
}
```

Setting query string parameters,

```csharp
[Fact]
public void ShouldResolveUriTemplateWithNonStringParameter()
{
    var url = new UriTemplate("http://example.org/location{?lat,lng}")
        .AddParameters(new { lat = 31.464, lng = 74.386 })
        .Resolve();

    Assert.Equal("http://example.org/location?lat=31.464&lng=74.386", url);
}
```


Resolving a URI when parameters are not set will simply remove the parameters,

```csharp
[Fact]
public void SomeParametersFromAnObject()
{
    var url = new UriTemplate("http://example.org{/environment}{/version}/customers{?active,country}")
        .AddParameters(new
        {
            version = "v2",
            active = "true"
        })
        .Resolve();

    Assert.Equal("http://example.org/v2/customers?active=true", url);
}
```

You can even pass lists as parameters

```csharp
[Fact]
public void ApplyParametersObjectWithAListofInts()
{
    var url = new UriTemplate("http://example.org/customers{?ids,order}")
        .AddParameters(new
        {
            order = "up",
            ids = new[] {21, 75, 21}
        })
        .Resolve();

    Assert.Equal("http://example.org/customers?ids=21,75,21&order=up", url);
}
```

And dictionaries,

```csharp
[Fact]
public void ApplyDictionaryToQueryParameters()
{
    var url = new UriTemplate("http://example.org/foo{?coords*}")
        .AddParameter("coords", new Dictionary<string, string>
        {
            {"x", "1"},
            {"y", "2"},
        })
        .Resolve();

    Assert.Equal("http://example.org/foo?x=1&y=2", url);
}
```

We also handle all the complex URI encoding rules automatically.

```csharp
[Fact]
public void TestExtremeEncoding()
{
    var url = new UriTemplate("http://example.org/sparql{?query}")
            .AddParameter("query", "PREFIX dc: <http://purl.org/dc/elements/1.1/> SELECT ?book ?who WHERE { ?book dc:creator ?who }")
            .Resolve();
    Assert.Equal("http://example.org/sparql?query=PREFIX%20dc%3A%20%3Chttp%3A%2F%2Fpurl.org%2Fdc%2Felements%2F1.1%2F%3E%20SELECT%20%3Fbook%20%3Fwho%20WHERE%20%7B%20%3Fbook%20dc%3Acreator%20%3Fwho%20%7D", url);
}
```

### Using the low-allocation API directly

The low-allocation library provides a generic class `UriTemplateResolver<TParameterProvider, TParameterPayload>` for URI template resolution.

The `TParameterProvider` is a type which implements `ITemplateParameterProvider<TParameterPayload>`, to process a parameter payload according to a variable specification.

This allows you to process parameters as efficiently as possible, based on the types you need to support.

The benchmarks contain an example built over the low-allocation [Corvus.JsonSchema.ExtendedTypes](https://github.com/corvus-dotnet/Corvus.JsonSchema) called `JsonTemplateParameterProvider` that takes a parameter set based on a JSON object, supporting all JSON element types as parameter values.

```csharp
object? nullState = default;
JsonUriTemplateResolver.TryResolveResult(UriTemplate.AsSpan(), false, JsonValues, HandleResult, ref nullState);
static void HandleResult(ReadOnlySpan<char> resolvedTemplate, ref object? state)
{
    Do what you want with the resolved template!
}
```

There are also overloads of `TryResolveResult` which will write to an `IBufferWriter<char>` instead of providing the `ReadOnlySpan<char>` to a callback.


## Build and test

As well as having a set of regular usage tests, this library also executes tests based on a standard test suite. This test suite is pulled in as a Git Submodule, therefore when cloning this repo, you will need use the `--recursive` switch.

The `./uritemplate-test` folder is a submodule pointing to that test suite repo.

When cloning this repository it is important to clone submodules, because test projects in this repository depend on that submodule being present. If you've already cloned the project, and haven't yet got the submodules, run this command:

```
git submodule update --init --recursive
```

Note that git pull does not automatically update submodules, so if git pull reports that any submodules have changed, you can use the preceding command again, used to update the existing submodule reference.

When updating to newer versions of the test suite, we can update the submodule reference thus:

```
cd uritemplate-test
git fetch
git merge origin/master
cd ..
git commit - "Updated to latest URI Template Test Suite"
```

(Or you can use `git submodule update --remote` instead of cding into the submodule folder and updating from there.)

