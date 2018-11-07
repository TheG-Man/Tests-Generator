using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestsGenerationLibrary.Consumers
{
    public interface IConsumer<TResultPayload>
    {
        ConsumerResult<TResultPayload> Consume(TestClassInMemoryInfo testClassInMemoryInfo);
    }
}
