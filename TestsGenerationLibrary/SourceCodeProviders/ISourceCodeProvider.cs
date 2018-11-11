using System.Collections.Generic;

namespace TestsGenerationLibrary.SourceCodeProviders
{
    public interface ISourceCodeProvider
    {
        IEnumerable<string> Provide();
    }
}
