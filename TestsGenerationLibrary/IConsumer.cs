using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestsGenerationLibrary
{
    public interface IConsumer
    {
        Task ConsumeAsync(ISourceBlock<string> testTextsBuffer);
    }
}
