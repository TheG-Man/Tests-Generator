using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary
{
    public interface ITestsGenerator
    {
        void Generate(IEnumerable<string> filePaths);
    }
}
