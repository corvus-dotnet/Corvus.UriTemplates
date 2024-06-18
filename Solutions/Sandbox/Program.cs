using Corvus.UriTemplates;
using Corvus.UriTemplates.Benchmarking;


IUriTemplateParser template = UriTemplateParserFactory.CreateParser(
    "{scheme}://{host}/");
template.IsMatch("http://example.com/");

UriTemplateTableMatching uriTemplateTableMatching = new();

await uriTemplateTableMatching.GlobalSetup();

await Task.Delay(2000);

uriTemplateTableMatching.MatchCorvus();

await Task.Delay(1000);

await uriTemplateTableMatching.GlobalCleanup();