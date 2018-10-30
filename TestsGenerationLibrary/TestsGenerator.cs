using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestsGenerationLibrary
{
    public class TestsGenerator : ITestsGenerator 
    {
        private readonly TransformBlock<string, string> _testTextsBuffer;
        private readonly List<Task> _tasks = new List<Task>();

        private readonly TestsGeneratorRestrictions _testsGeneratorRestrictions;

        public TestsGenerator(TestsGeneratorRestrictions testsGeneratorRestrictions, IConsumer consumer)
        {
            _testsGeneratorRestrictions = testsGeneratorRestrictions;

            var dataflowBlockOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _testsGeneratorRestrictions.MaxProcessingTasksCount };
            _testTextsBuffer = new TransformBlock<string, string>(new Func<string, string>(Produce), dataflowBlockOptions);

            for (int i = 0; i < _testsGeneratorRestrictions.MaxWritingTasksCount; ++i)
            {
                _tasks.Add(Task.Factory.StartNew( delegate { consumer.ConsumeAsync(_testTextsBuffer); }));
            }
        }

        public TestsGenerator(string outDirPath, TestsGeneratorRestrictions testsGeneratorRestrictions)
            : this(testsGeneratorRestrictions, new FileConsumer(outDirPath))
        {
        }

        public void Generate(IEnumerable<string> filePaths)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = _testsGeneratorRestrictions.MaxReadingTasksCount };
            Parallel.ForEach(filePaths, parallelOptions, async filePath => {
                await _testTextsBuffer.SendAsync(File.ReadAllText(filePath));
            });

            _testTextsBuffer.Complete();

            Task.WaitAll(_tasks.ToArray());
        }

        private string Produce(string programText)
        {
            //var testTemplate = new TestTemplate(programText);

            return programText + " Vadim";
        }
    }
}
