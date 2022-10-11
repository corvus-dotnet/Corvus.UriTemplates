using System.Text.Json;
using Corvus.UriTemplates;

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
