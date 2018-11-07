using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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

        public ConsumerResult Consume(TestClassInMemoryInfo testClassInMemoryInfo)
        {
            string filePath = filePath = $"{_outputDirectoryPath}\\{testClassInMemoryInfo.TestClassName}";

            Exception error = null;
            try
            {
                File.WriteAllText(filePath, testClassInMemoryInfo.TestClassData);
            }
            catch (Exception exception)
            {
                error = exception;
            }

            return new ConsumerResult(error == null, filePath);
        }
    }
}
