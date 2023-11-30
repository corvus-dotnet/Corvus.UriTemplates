using Corvus.UriTemplates.Benchmarking;

UriTemplateTableMatching uriTemplateTableMatching = new();

await uriTemplateTableMatching.GlobalSetup();

await Task.Delay(2000);

uriTemplateTableMatching.MatchCorvus();

await Task.Delay(1000);

await uriTemplateTableMatching.GlobalCleanup();