////using Corvus.UriTemplates.Benchmarking;

////UriTemplateTableMatching uriTemplateTableMatching = new();

////await uriTemplateTableMatching.GlobalSetup();

////await Task.Delay(10000);

////uriTemplateTableMatching.MatchCorvus();

////await Task.Delay(10000);

////await uriTemplateTableMatching.GlobalCleanup();

using Corvus.UriTemplates.Benchmarking;

UriTemplateParameterSetting uriTemplateParameterSetting = new();

await uriTemplateParameterSetting.GlobalSetup();

await Task.Delay(10000);

Console.WriteLine("Start now");

await Task.Delay(3000);

uriTemplateParameterSetting.ResolveUriCorvusTavis();

await Task.Delay(10000);

await uriTemplateParameterSetting.GlobalCleanup();