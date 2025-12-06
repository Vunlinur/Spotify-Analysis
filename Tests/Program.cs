using BenchmarkDotNet.Running;
using System;
using Tests.GetDataTests;

namespace Tests {
    class Program {
        static void Main(string[] args) {
            BenchmarkRunner.Run<GetDataBenchmark>();
            Console.WriteLine((char)7);
        }
    }
}
