using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary
{
    public sealed class TestsGeneratorRestrictions
    {
        private int _maxConsumersCount = Environment.ProcessorCount;

        public int MaxReadingTasksCount { get; set; }
        public int MaxProcessingTasksCount { get; set; }
        public int MaxWritingTasksCount { get; set; }

        public TestsGeneratorRestrictions(int maxReadingTasksCount, int maxProcessingTasksCount, int maxWritingTasksCount)
        {
            MaxReadingTasksCount = maxReadingTasksCount;
            MaxProcessingTasksCount = maxProcessingTasksCount;

            if (maxWritingTasksCount < 1 || maxWritingTasksCount > _maxConsumersCount)
            {
                MaxWritingTasksCount = _maxConsumersCount;
            }
            else
            {
                MaxWritingTasksCount = maxWritingTasksCount;
            }
        }
    }
}
