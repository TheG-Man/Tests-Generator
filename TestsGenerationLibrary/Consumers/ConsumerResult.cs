namespace TestsGenerationLibrary.Consumers
{
    public class ConsumerResult<TResultPayload>
    {
        public bool IsCompletedSuccessfully { get; }
        public TResultPayload Result { get; }

        public ConsumerResult(bool isCompletedSuccessfully, TResultPayload result)
        {
            IsCompletedSuccessfully = isCompletedSuccessfully;
            Result = result;
        }
    }
}
