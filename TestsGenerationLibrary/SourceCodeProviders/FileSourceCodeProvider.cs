using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestsGenerationLibrary.SourceCodeProviders
{
    public class FileSourceCodeProvider : ISourceCodeProvider
    {
        private readonly IEnumerable<string> _filePaths;
        private readonly ParallelOptions _maxReadingTasksCount;

        public FileSourceCodeProvider(IEnumerable<string> filePaths)
            : this(filePaths, -1)
        {
        }

        public FileSourceCodeProvider(IEnumerable<string> filePaths, int maxReadingTasksCount)
        {
            _filePaths = filePaths;

            _maxReadingTasksCount = new ParallelOptions { MaxDegreeOfParallelism = maxReadingTasksCount };
        }

        public IEnumerable<string> Provide()
        {
            List<string> sourceCodesBuffer = new List<string>();
            List<Exception> exceptions = new List<Exception>();

            Parallel.ForEach(_filePaths, _maxReadingTasksCount, filePath => {
                try
                {
                    sourceCodesBuffer.Add(File.ReadAllText(filePath));
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            });

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }

            return sourceCodesBuffer;
        }
    }
}
