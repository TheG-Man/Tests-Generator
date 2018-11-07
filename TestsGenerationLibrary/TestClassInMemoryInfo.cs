using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary
{
    public class TestClassInMemoryInfo
    {
        public string TestClassName { get; }
        public string TestClassData { get; }

        public TestClassInMemoryInfo(string testClassName, string testClassData)
        {
            TestClassName = testClassName;
            TestClassData = testClassData;
        }
    }
}
