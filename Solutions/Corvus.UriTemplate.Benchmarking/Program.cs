// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Perfolizer.Mathematics.OutlierDetection;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(
        config:
            ManualConfig.Create(DefaultConfig.Instance)
            .AddJob(Job.Default
                .WithBaseline(true)
                .WithRuntime(CoreRuntime.Core80)
                .WithOutlierMode(OutlierMode.RemoveAll)
                .WithStrategy(RunStrategy.Throughput))
            .AddJob(Job.Default
                .WithRuntime(ClrRuntime.Net481)
                .WithOutlierMode(OutlierMode.RemoveAll)
                .WithStrategy(RunStrategy.Throughput)));