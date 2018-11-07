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
        private readonly IConsumer _consumer;
        private readonly TestsGeneratorRestrictions _testsGeneratorRestrictions;

        private readonly TransformBlock<string, TestClassInMemoryInfo> _producerBuffer;
        private readonly TransformBlock<TestClassInMemoryInfo, ConsumerResult> _generatedTestsBuffer;

        public TestsGenerator(IConsumer consumer)
            : this(consumer, new TestsGeneratorRestrictions(-1, -1))
        {
        }

        public TestsGenerator(IConsumer consumer, TestsGeneratorRestrictions testsGeneratorRestrictions)
        {
            _consumer = consumer;
            _testsGeneratorRestrictions = testsGeneratorRestrictions;

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var processingTaskRestriction = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _testsGeneratorRestrictions.MaxProcessingTasksCount };
            var outputTaskRestriction = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _testsGeneratorRestrictions.MaxWritingTasksCount };

            _producerBuffer = new TransformBlock<string, TestClassInMemoryInfo>(new Func<string, TestClassInMemoryInfo>(Produce), processingTaskRestriction);
            _generatedTestsBuffer = new TransformBlock<TestClassInMemoryInfo, ConsumerResult>(new Func<TestClassInMemoryInfo, ConsumerResult>(_consumer.Consume), outputTaskRestriction);
            
            _producerBuffer.LinkTo(_generatedTestsBuffer, linkOptions);
        }

        public IEnumerable<ConsumerResult> Generate(ISourceCodeProvider dataProvider)
        {
            var consumerResults = GetConsumerResults();

            Parallel.ForEach(dataProvider.Provide(), async dataInMemory => {
                await _producerBuffer.SendAsync(dataInMemory);
            });

            _producerBuffer.Complete();
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
                    _generatedTestsBuffer.Post(new TestClassInMemoryInfo(testTemplates.ElementAt(i).TestClassName, testTemplates.ElementAt(i).TestClassData));
                }
            }

            return new TestClassInMemoryInfo(testTemplates.First().TestClassName, testTemplates.First().TestClassData);
        }

        private async Task<IEnumerable<ConsumerResult>> GetConsumerResults()
        {
            List<ConsumerResult> consumerResults = new List<ConsumerResult>();

            while (await _generatedTestsBuffer.OutputAvailableAsync())
            {
                consumerResults.Add(_generatedTestsBuffer.Receive());
            }

            return consumerResults;
        }
    }
}
