using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestsGenerationLibrary.Consumers
{
    public class FileConsumer : IConsumer
    {
        private readonly string _outputDirectoryPath;

        public FileConsumer(string outputDirectoryPath)
        {
            _outputDirectoryPath = outputDirectoryPath;

            if (!Directory.Exists(_outputDirectoryPath))
            {
                Directory.CreateDirectory(_outputDirectoryPath);
            }
        }

        public void Consume(IReceivableSourceBlock<TaskResult> testTextsBuffer)
        {
            int thread = Thread.CurrentThread.ManagedThreadId;
            TaskResult taskResult;
            while (testTextsBuffer.TryReceive(out taskResult))
            {                
                File.WriteAllText($"{_outputDirectoryPath}\\{taskResult.FileName}", taskResult.FileData);
            }
        }
    }
}
