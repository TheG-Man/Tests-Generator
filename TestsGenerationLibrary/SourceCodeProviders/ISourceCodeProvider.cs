using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary.SourceCodeProviders
{
    public interface ISourceCodeProvider
    {
        IEnumerable<string> Provide();
    }
}
