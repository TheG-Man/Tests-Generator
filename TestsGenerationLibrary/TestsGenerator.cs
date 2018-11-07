using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestsGenerationLibrary.Consumers;
using TestsGenerationLibrary.SourceCodeProviders;
using TestsGenerationLibrary.MembersInfo;

namespace TestsGenerationLibrary
{
    public class TestsGenerator : ITestsGenerator
    {
        private readonly TestsGeneratorRestrictions _testsGeneratorRestrictions;
        private BufferBlock<TestClassInMemoryInfo> _additionalProducerBuffer = new BufferBlock<TestClassInMemoryInfo>();

        public TestsGenerator()
            : this(new TestsGeneratorRestrictions(-1, -1))
        {
        }

        public TestsGenerator(TestsGeneratorRestrictions testsGeneratorRestrictions)
        {
            _testsGeneratorRestrictions = testsGeneratorRestrictions;
        }

        public IEnumerable<ConsumerResult<TResultPayload>> Generate<TResultPayload>(ISourceCodeProvider dataProvider, IConsumer<TResultPayload> consumer)
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var processingTaskRestriction = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _testsGeneratorRestrictions.MaxProcessingTasksCount };
            var outputTaskRestriction = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _testsGeneratorRestrictions.MaxWritingTasksCount };

            var producerBuffer = new TransformBlock<string, TestClassInMemoryInfo>(new Func<string, TestClassInMemoryInfo>(Produce), processingTaskRestriction);
            var commonGeneratedTestsBuffer = new JoinBlock<TestClassInMemoryInfo, TestClassInMemoryInfo>(new GroupingDataflowBlockOptions{ Greedy = false });
            var consumerResultsBuffer = new BufferBlock<ConsumerResult<TResultPayload>>();
            var mergeBuffers = new ActionBlock<Tuple<TestClassInMemoryInfo, TestClassInMemoryInfo>>(data =>
                {
                    if (data.Item1 != null)
                    {
                        consumerResultsBuffer.Post(consumer.Consume(data.Item1));
                    }
                    if (data.Item2 != null)
                    {
                        consumerResultsBuffer.Post(consumer.Consume(data.Item2));
                    }
                }, outputTaskRestriction);

            producerBuffer.LinkTo(commonGeneratedTestsBuffer.Target1, linkOptions);
            _additionalProducerBuffer.LinkTo(commonGeneratedTestsBuffer.Target2, linkOptions);
            commonGeneratedTestsBuffer.LinkTo(mergeBuffers, linkOptions);
            mergeBuffers.Completion.ContinueWith(delegate { consumerResultsBuffer.Complete(); });

            var consumerResults = Task.Run(async delegate {
                List<ConsumerResult<TResultPayload>> consumerResultsList = new List<ConsumerResult<TResultPayload>>();

                while (await consumerResultsBuffer.OutputAvailableAsync())
                {
                    consumerResultsList.Add(consumerResultsBuffer.Receive());
                }

                return consumerResultsList;
            });

            Parallel.ForEach(dataProvider.Provide(), async dataInMemory => {
                await producerBuffer.SendAsync(dataInMemory);
            });

            producerBuffer.Complete();
            consumerResults.Wait();

            return consumerResults.Result;
        }

        private TestClassInMemoryInfo Produce(string sourceCode)
        {
            SyntaxTreeInfoBuilder syntaxTreeInfoBuilder = new SyntaxTreeInfoBuilder(sourceCode);
            SyntaxTreeInfo syntaxTreeInfo = syntaxTreeInfoBuilder.GetSyntaxTreeInfo();

            TestTemplatesGenerator testTemplatesGenerator = new TestTemplatesGenerator(syntaxTreeInfo);
            List<TestClassInMemoryInfo> testTemplates = testTemplatesGenerator.GetTestTemplates().ToList();

            if (testTemplates.Count > 1)
            {
                for (int i = 1; i < testTemplates.Count; ++i)
                {
                    _additionalProducerBuffer.Post(new TestClassInMemoryInfo(testTemplates.ElementAt(i).TestClassName, testTemplates.ElementAt(i).TestClassData));
                }
            }

            return new TestClassInMemoryInfo(testTemplates.First().TestClassName, testTemplates.First().TestClassData);
        }
    }
}
