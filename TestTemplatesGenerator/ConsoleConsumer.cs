using System;
using TestsGenerationLibrary;
using TestsGenerationLibrary.Consumers;

namespace TestTemplatesGenerator
{
    class ConsoleConsumer : IConsumer<bool>
    {
        public ConsumerResult<bool> Consume(TestClassInMemoryInfo testClassInMemoryInfo)
        {
            Console.WriteLine($"Test template name: {testClassInMemoryInfo.TestClassName}\n");
            Console.Write(testClassInMemoryInfo.TestClassData);

            return new ConsumerResult<bool>(true, true);
        }
    }
}
