using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsGenerationLibrary;
using TestsGenerationLibrary.Consumers;

namespace TestTemplatesGenerator
{
    class ConsoleConsumer : IConsumer
    {
        public ConsumerResult Consume(TestClassInMemoryInfo testClassInMemoryInfo)
        {
            Console.WriteLine($"Test template name: {testClassInMemoryInfo.TestClassName}\n");
            Console.Write(testClassInMemoryInfo.TestClassData);

            return new ConsumerResult(true, null);
        }
    }
}
