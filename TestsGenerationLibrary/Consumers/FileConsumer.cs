using System;
using System.IO;

namespace TestsGenerationLibrary.Consumers
{
    public class FileConsumer : IConsumer<string>
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

        public ConsumerResult<string> Consume(TestClassInMemoryInfo testClassInMemoryInfo)
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

            return new ConsumerResult<string>(error == null, filePath);
        }
    }
}
