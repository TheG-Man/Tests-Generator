using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary.Consumers
{
    public class ConsumerResult
    {
        public bool IsCompletedSuccessfully { get; }
        public object Result { get; }

        public ConsumerResult(bool isCompletedSuccessfully, object result)
        {
            IsCompletedSuccessfully = isCompletedSuccessfully;
            Result = result;
        }
    }
}
