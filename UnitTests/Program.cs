using BenchmarkDotNet.Running;
using System;

namespace UnitTests {
    class Program {
        static void Main(string[] args) {
            BenchmarkRunner.Run<GetDataBenchmark>();
        }
    }
}
