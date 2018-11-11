using System.Collections.Generic;
using TestsGenerationLibrary.Consumers;
using TestsGenerationLibrary.SourceCodeProviders;

namespace TestsGenerationLibrary
{
    public interface ITestsGenerator
    {
        IEnumerable<ConsumerResult<TResultPayload>> Generate<TResultPayload>(ISourceCodeProvider dataProvider, IConsumer<TResultPayload> consumer);
    }
}
