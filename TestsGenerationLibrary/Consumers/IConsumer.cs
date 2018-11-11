namespace TestsGenerationLibrary.Consumers
{
    public interface IConsumer<TResultPayload>
    {
        ConsumerResult<TResultPayload> Consume(TestClassInMemoryInfo testClassInMemoryInfo);
    }
}
