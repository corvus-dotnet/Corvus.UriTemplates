// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Perfolizer.Mathematics.OutlierDetection;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll(
        ManualConfig.Create(DefaultConfig.Instance)
        .AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core70)
            .WithOutlierMode(OutlierMode.RemoveAll)
            .WithStrategy(RunStrategy.Throughput))
        .AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core80)
            .WithOutlierMode(OutlierMode.RemoveAll)
            .WithStrategy(RunStrategy.Throughput)));