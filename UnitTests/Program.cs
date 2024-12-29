using BenchmarkDotNet.Running;
using System;
using Tests.GetDataTests;

namespace UnitTests {
    class Program {
        static void Main(string[] args) {
            BenchmarkRunner.Run<GetDataBenchmark>();
        }
    }
}
