using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestsGenerationLibrary.Consumers;
using TestsGenerationLibrary.MembersInfo;

namespace TestsGenerationLibrary
{
    public class TestsGenerator : ITestsGenerator 
    {
        private readonly IConsumer _consumer;
        private readonly TestsGeneratorRestrictions _testsGeneratorRestrictions;
        private readonly TransformBlock<TaskInfo, TaskResult> _taskResultsBuffer;
        private readonly TransformBlock<TaskInfo, TaskResult> _taskResultsAdditionalBuffer;
        private readonly List<Task> _tasks = new List<Task>();

        public TestsGenerator(string outputDirectoryPath)
            : this(new FileConsumer(outputDirectoryPath), new TestsGeneratorRestrictions(-1, -1, -1))
        {
        }

        public TestsGenerator(string outputDirectoryPath, TestsGeneratorRestrictions testsGeneratorRestrictions)
            : this(new FileConsumer(outputDirectoryPath), testsGeneratorRestrictions)
        {
        }

        public TestsGenerator(IConsumer consumer, TestsGeneratorRestrictions testsGeneratorRestrictions)
        {
            _consumer = consumer;
            _testsGeneratorRestrictions = testsGeneratorRestrictions;
           
            var dataflowBlockOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _testsGeneratorRestrictions.MaxProcessingTasksCount };
            _taskResultsBuffer = new TransformBlock<TaskInfo, TaskResult>(new Func<TaskInfo, TaskResult>(Produce), dataflowBlockOptions);
            _taskResultsAdditionalBuffer = new TransformBlock<TaskInfo, TaskResult>(new Func<TaskInfo, TaskResult>(SendTaskResultToBuffer), dataflowBlockOptions);
        }

        public void Generate(IEnumerable<string> filePaths)
        {
            Task consumer = ConsumeAsync();
            AggregateException error = null;

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = _testsGeneratorRestrictions.MaxReadingTasksCount };
            Parallel.ForEach(filePaths, parallelOptions, async filePath => {
                try
                {
                    await _taskResultsBuffer.SendAsync(new TaskInfo(isGenerated: false, fileName: "", fileData: File.ReadAllText(filePath)));
                }
                catch (Exception exception)
                {
                    if (error == null)
                    {
                        error = new AggregateException(exception);
                    }
                    else
                    {
                        error = new AggregateException(error, exception);
                    }
                }
            });

            if (error != null)
            {
                throw error;
            }

            _taskResultsBuffer.Completion.ContinueWith(delegate { _taskResultsAdditionalBuffer.Complete(); });
            _taskResultsBuffer.Complete();

            consumer.Wait();
        }

        private TaskResult Produce(TaskInfo taskInfo)
        {
            SyntaxTreeInfoBuilder syntaxTreeInfoBuilder = new SyntaxTreeInfoBuilder(taskInfo.FileData);
            SyntaxTreeInfo syntaxTreeInfo = syntaxTreeInfoBuilder.GetSyntaxTreeInfo();

            TestTemplatesGenerator testTemplatesGenerator = new TestTemplatesGenerator(syntaxTreeInfo);
            List<TaskResult> testTemplates = testTemplatesGenerator.GetTestTemplates().ToList();

            if (testTemplates.Count > 1)
            {
                for (int i = 1; i < testTemplates.Count; ++i)
                {
                    _taskResultsAdditionalBuffer.Post(new TaskInfo(true, testTemplates.ElementAt(i).FileName, testTemplates.ElementAt(i).FileData));
                }
            }

            return new TaskResult(testTemplates.First().FileName, testTemplates.First().FileData);
        }

        private TaskResult SendTaskResultToBuffer(TaskInfo taskInfo)
        {
            return new TaskResult(taskInfo.FileName, taskInfo.FileData);
        }

        private async Task ConsumeAsync()
        {
            using (Semaphore semaphore = new Semaphore(_testsGeneratorRestrictions.MaxWritingTasksCount, _testsGeneratorRestrictions.MaxWritingTasksCount))
            {
                while (await _taskResultsBuffer.OutputAvailableAsync())
                {
                    if (semaphore.WaitOne())
                    {
                        try
                        {
                            _tasks.Add(Task.Factory.StartNew(delegate { _consumer.Consume(_taskResultsBuffer); }));
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                }

                while (await _taskResultsAdditionalBuffer.OutputAvailableAsync())
                {
                    if (semaphore.WaitOne())
                    {
                        try
                        {
                            _tasks.Add(Task.Factory.StartNew(delegate { _consumer.Consume(_taskResultsAdditionalBuffer); }));
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                }
            }

            Task.WaitAll(_tasks.ToArray());
        }
    }
}
