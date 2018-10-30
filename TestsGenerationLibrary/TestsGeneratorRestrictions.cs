using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary
{
    public sealed class TestsGeneratorRestrictions
    {
        private const int MaxConsumersCount = 8;

        public int MaxReadingTasksCount { get; set; }
        public int MaxProcessingTasksCount { get; set; }
        public int MaxWritingTasksCount { get; set; }

        public TestsGeneratorRestrictions(int maxReadingTasksCount, int maxProcessingTasksCount, int maxWritingTasksCount)
        {
            MaxReadingTasksCount = maxReadingTasksCount;
            MaxProcessingTasksCount = maxProcessingTasksCount;

            if (maxWritingTasksCount < 1)
            {
                MaxWritingTasksCount = MaxConsumersCount;
            }
            else
            {
                MaxWritingTasksCount = maxWritingTasksCount;
            }
        }
    }
}
