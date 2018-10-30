using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestsGenerationLibrary
{
    class FileConsumer : IConsumer
    {
        private readonly string _outputDirectoryPath;

        public FileConsumer(string outputDirectoryPath)
        {
            _outputDirectoryPath = outputDirectoryPath;
        }

        public async Task ConsumeAsync(ISourceBlock<string> testTextsBuffer)
        {
            while (await testTextsBuffer.OutputAvailableAsync())
            {
                Console.WriteLine(testTextsBuffer.Receive() + "   Thread: " + Thread.CurrentThread.ManagedThreadId);
            }
        }
    }
}
