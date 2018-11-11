using System.Collections.Generic;

namespace TestsGenerationLibrary.MembersInfo
{
    class SyntaxTreeInfo
    {
        public IEnumerable<ClassInfo> Classes { get; }

        public SyntaxTreeInfo(IEnumerable<ClassInfo> classes)
        {
            Classes = classes;
        }
    }
}
