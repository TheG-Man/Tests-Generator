namespace TestsGenerationLibrary
{
    public sealed class TestsGeneratorRestrictions
    {
        public int MaxProcessingTasksCount { get; }
        public int MaxWritingTasksCount { get; }

        public TestsGeneratorRestrictions(int maxProcessingTasksCount, int maxWritingTasksCount)
        {
            MaxProcessingTasksCount = maxProcessingTasksCount;
            MaxWritingTasksCount = maxWritingTasksCount;
        }
    }
}
