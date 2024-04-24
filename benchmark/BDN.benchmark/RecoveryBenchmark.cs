﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using Embedded.perftest;
using Garnet.server;

namespace BDN.benchmark
{
    public class CustomConfig : ManualConfig
    {
        public CustomConfig()
        {
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Median);
            AddColumn(StatisticColumn.P90);
            AddColumn(StatisticColumn.P95);
        }
    }

    [Config(typeof(CustomConfig))]
    public class RecoveryBenchmark
    {
        [ParamsSource(nameof(CommandLineArgsProvider))]
        public string LogDir { get; set; }

        public IEnumerable<string> CommandLineArgsProvider()
        {
            // Return the command line arguments as an enumerable
            return Environment.GetCommandLineArgs().Skip(1);
        }

        [Params("100m")]
        public string MemorySize { get; set; }

        EmbeddedRespServer server;

        [IterationSetup]
        public void Setup()
        {
            Console.WriteLine($"LogDir: {LogDir}");
            server = new EmbeddedRespServer(new GarnetServerOptions()
            {
                EnableStorageTier = true,
                LogDir = LogDir,
                CheckpointDir = LogDir,
                IndexSize = "1m",
                DisableObjects = true,
                MemorySize = MemorySize,
                PageSize = "32k",
            });
        }

        [IterationCleanup]
        public void Cleanup()
        {
            server.Dispose();
        }

        [Benchmark]
        public void Recover()
        {
            server.StoreWrapper.RecoverCheckpoint();
        }
    }
}