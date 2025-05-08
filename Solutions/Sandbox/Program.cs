using Corvus.UriTemplates;
using Corvus.UriTemplates.Benchmarking;


IUriTemplateParser template2 = UriTemplateParserFactory.CreateParser("/weather{/rest*}");
Console.WriteLine(template2.IsMatch("/weather"));
Console.WriteLine(template2.IsMatch("/weather/"));
Console.WriteLine(template2.IsMatch("/weather/one"));
Console.WriteLine(template2.IsMatch("/weather/weather/or/not"));
Console.WriteLine(template2.IsMatch("/weatherone")); // Doesn't match, because of the / in {/rest*}

IUriTemplateParser template = UriTemplateParserFactory.CreateParser(
    "{scheme}://{host}/");
template.IsMatch("http://example.com/");

UriTemplateTableMatching uriTemplateTableMatching = new();

await uriTemplateTableMatching.GlobalSetup();

await Task.Delay(2000);

uriTemplateTableMatching.MatchCorvus();

await Task.Delay(1000);

await uriTemplateTableMatching.GlobalCleanup();