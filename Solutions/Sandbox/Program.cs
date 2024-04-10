////using Corvus.UriTemplates.Benchmarking;

////UriTemplateTableMatching uriTemplateTableMatching = new();

////await uriTemplateTableMatching.GlobalSetup();

////await Task.Delay(10000);

////uriTemplateTableMatching.MatchCorvus();

////await Task.Delay(10000);

////await uriTemplateTableMatching.GlobalCleanup();

using Corvus.UriTemplates.Benchmarking;
using Microsoft.CodeAnalysis.Operations;

UriTemplateParameterSetting uriTemplateParameterSetting = new();

await uriTemplateParameterSetting.GlobalSetup();

Console.WriteLine("Start now");

await Task.Delay(3000);

for(int i = 0; i < 10000; ++i)
{
    uriTemplateParameterSetting.ResolveUriCorvusJson();
}

Console.WriteLine("Stop now");

await Task.Delay(10000);

await uriTemplateParameterSetting.GlobalCleanup();