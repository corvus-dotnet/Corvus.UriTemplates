using System.Text.Json;
using Corvus.UriTemplates;

const string uriTemplate = "http://example.org/location{?value*}";

using JsonDocument jsonValues = JsonDocument.Parse("{\"value\": { \"foo\": \"bar\", \"bar\": 3.4, \"baz\": \"bob\" }}");

object? nullState = default;

JsonUriTemplateResolver.TryResolveResult(uriTemplate.AsSpan(), false, jsonValues.RootElement, HandleResult, ref nullState);

static void HandleResult(ReadOnlySpan<char> resolvedTemplate, ref object? state)
{
    Console.WriteLine(resolvedTemplate.ToString());
}
