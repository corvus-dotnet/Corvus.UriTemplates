# Corvus.UriTemplates
Low-allocation URI Template parsing and resolution, supporting the [Tavis.UriTemplates](https://github.com/tavis-software/Tavis.UriTemplates) API

This is a net8.0+ implementation of the [URI Template Spec RFC6570](http://tools.ietf.org/html/rfc6570). 

The library implements Level 4 compliance and is tested against test cases from [UriTemplate test suite](https://github.com/uri-templates/uritemplate-test).

## Introduction

This library provides tools for low-allocation URI Template parameter extraction (via `IUriTemplateParser`) and URI Template resolution (via `UriTemplateResolver`).

We then implement a drop-in replacement for the API supported by [Tavis.UriTemplates](https://github.com/tavis-software/Tavis.UriTemplates), with lower allocations and higher performance.

## Performance

There is a standard benchmark testing basic parameter extraction and resolution for the original Tavis.UriTemplate, the updated Corvus.UriTemplates.TavisApi.UriTemplate and the underlying zero-allocation URI template parser.

As you can see, there is a significant benefit to using the Corvus implementation, even without dropping down the low-level zero allocation API.

### Apply parameters to a URI template to resolve a URI
| Method                     | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| ResolveUriTavis            | 356.4 ns | 2.59 ns | 2.29 ns |  1.00 | 0.1459 |    1832 B |        1.00 |
| ResolveUriCorvusTavis      | 308.6 ns | 1.81 ns | 1.51 ns |  0.87 | 0.0172 |     216 B |        0.12 |
| ResolveUriCorvusJson       | 439.5 ns | 2.75 ns | 2.44 ns |  1.23 | 0.0076 |      96 B |        0.05 |
| ResolveUriCorvusDictionary | 197.5 ns | 1.48 ns | 1.38 ns |  0.55 | 0.0069 |      88 B |        0.05 |

### Extract parameters from a URI by using a URI template
| Method                                         | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| ExtractParametersTavis                         | 775.49 ns | 15.076 ns | 19.066 ns |  1.00 | 0.0873 |    1096 B |        1.00 |
| ExtractParametersCorvusTavis                   | 231.16 ns |  4.362 ns |  3.867 ns |  0.30 | 0.0482 |     608 B |        0.55 |
| ExtractParametersCorvusTavisWithParameterCache | 133.24 ns |  0.322 ns |  0.301 ns |  0.17 |      - |         - |        0.00 |
| ExtractParametersCorvus                        |  75.53 ns |  0.507 ns |  0.474 ns |  0.10 |      - |         - |        0.00 |

### Match a URI Template Table
| Method           | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| MatchTavis       | 48.65 us | 0.671 us | 0.560 us |  1.00 | 5.7983 |   72808 B |       1.000 |
| MatchCorvusTavis | 27.31 us | 0.255 us | 0.239 us |  0.56 |      - |     376 B |       0.005 |
| MatchCorvus      | 22.43 us | 0.297 us | 0.263 us |  0.46 |      - |         - |       0.000 |

## Tavis API
### Parameter Extraction

```csharp
UriTemplate template = new("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId={parentRequestId}{&hash,callback}");

Uri uri = new ("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId=123232323&hash=23ADE34FAE&callback=http%3A%2F%2Fexample.com%2Fcallback");

IDictionary<string, object?>? result = template.GetParameters(uri);
```

### URI Resolution

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

Our `Corvus.UriTemplates.TavisApi` implementation is built over an underlying low-allocation API.

## Low allocation API

### Extracting parameter values from a URI by matching it to a URI template

To create an instance of a parser for a URI template, call one of the `CreateParser()` overloads, passing it your URI template.

```csharp
IUriTemplateParser UriTemplateParserFactory.CreateParser(string uriTemplate);
```

or

```csharp
IUriTemplateParser UriTemplateParserFactory.CreateParser(ReadOnlySpan<char> uriTemplate);
```

You would typically have some initialization code that is called once to build your parsers from your templates (either derived statically or from some configuration)

```csharp
private const string UriTemplate = "http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId={parentRequestId}{&hash,callback}";

private static readonly IUriTemplateParser CorvusTemplate = CreateParser();

private static IUriTemplateParser CreateParser()
{
    return
        UriTemplateParserFactory.CreateParser(UriTemplate);
}
```

You can then make use of that parser to extract parameter values from a URI. `IUriTemplateParser` offers two mechanisms for this.

#### Callbacks

The parser offers a callback model to deliver the parameters to you (to avoid allocations). If you are used to low allocation code, you will probably recognize the pattern.




You call `EnumerateParameters()`, passing the URI you wish to parse (as a `ReadOnlySpan<char>`), a callback, and the initial value of a state object, which will be passed to that callback.

The callback itself is called by the parser each time a matched parameter is discovered.

It is given `ReadOnlySpan<char>` instances for the name and value pairs, along with the current version of the state object. This state is passed by `ref`, so you can update its value to keep track of whatever processing you are doing with the parameters you have been passed.

Here's an example that just counts the parameters it has seen.

```csharp
int state = 0;

CorvusTemplate.EnumerateParameters(Uri, HandleParameters, ref state);

static void HandleParameters(ReadOnlySpan<char> name, ReadOnlySpan<char> value, ref int state)
{
    state++;
}
```

> There is a defaulted optional parameter to this method that lets you specific an initial capacity for the cache; if you know how many parameters you are going to match, you can tune this to minimize the amount of re-allocation required.

#### Cacheable

To enable applications to separate the code that parses a URI from the code that uses the results (e.g., because parsing is done early on to choose between code paths), we offer an alternative model in which the parser returns all of the results of the parsing in an object you can retain:

```cs
public static UriTemplateParameters? GetParameters()
{
    if (CorvusTemplate.TryGetUriTemplateParameters(Uri, 3, out UriTemplateParameters? p))
    {
        return p;
    }

    return null;
}
```

The `UriTemplateParameters` object returned can then later be used to retrieve parameter values, e.g.:

```cs
Console.WriteLine($"hash is {(parameters.Has("hash") ? "present" : "absent")}");

if (parameters.TryGet("parentRequestId", out ParameterValue value))
{
    Console.WriteLine($"parentRequestId is {int.Parse(value.GetValue(Uri))}");
}
```

Note that when you retrieve values from `UriTemplateParameters` you must pass a `ReadOnlySpan<char>` to `GetValue`, because `TryGetUriTemplateParameters` does not make a copy of the original URI. (This is to avoid an unnecessary allocation to hold the copy.)


### Resolving a template by substituting parameter values and producing a URI

The other basic scenario is injecting parameter values into a URI template to produce a URI (or another URI template if we haven't replaced all the parameters in the template).

The underlying type that does the work is called `UriTemplateResolver<TParameterProvider,TParameterPayload>`.

The `TParameterProvider` is an `ITemplateParameterProvider<TParameterPayload>` - an interface implemented by types which convert from a source of parameter values (the `TParameterPayload`), on behalf of the `UriTemplateResolver`.

We offer two of these providers "out of the box" - the `JsonTemplateParameterProvider` (which adapts to a `JsonElement`) and the `DictionaryTemplateParameterProvider` (which adapts to an `IDictionary<string, object?>` and is used by the underlying Tavis-compatible API).

To save you having to work directly with the `UriTemplateResolver` plugging in all the necessary generic parameters, most `ITemplateParameterProvider` implements will offer a convenience type, and these are no exception.

`JsonUriTemplateResolver` and `DictionaryUriTemplateResolver` give you strongly typed `TryResolveResult` and `TryGetParameterNames` methods which you can use in your code.

Here's an example.

```csharp
const string uriTemplate = "http://example.org/location{?value*}";

using var jsonValues = JsonDocument.Parse("{\"value\": { \"foo\": \"bar\", \"bar\": 3.4, \"baz\": null }}");
Dictionary<string, string> value = new() { { "foo", "bar" }, { "bar", "baz" }, { "baz", "bob" } };
Dictionary<string, object?> parameters = new() { { "value", value } };

object? nullState = default;

JsonUriTemplateResolver.TryResolveResult(uriTemplate.AsSpan(), false, jsonValues.RootElement, HandleResult, ref nullState);
DictionaryUriTemplateResolver.TryResolveResult(uriTemplate.AsSpan(), false, parameters, HandleResult, ref nullState);

static void HandleResult(ReadOnlySpan<char> resolvedTemplate, ref object? state)
{
    Console.WriteLine(resolvedTemplate.ToString());
}
```

Notice how we can use the exact same callback that receives the resolved template, for both resolvers - the callback is not dependent on the particular parameter provider.

> The Dictionary provider is somewhat faster than the JSON provider, largely because it has less work to do to extract parameter names and values. However, the JSON parameter provider offers direct support for all JSON value kinds (including encoding serialized "deeply nested" JSON values).

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

