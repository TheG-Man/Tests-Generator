using System.Collections.Generic;

namespace TestsGenerationLibrary.MembersInfo
{
    class ClassInfo
    {
        public string NamespaceName { get; }
        public string Name { get; }
        public ConstructorInfo Constructor { get; }
        public IEnumerable<MethodInfo> Methods { get; }

        public ClassInfo(string namespaceName, string name, ConstructorInfo constructor, IEnumerable<MethodInfo> methods)
        {
            NamespaceName = namespaceName;
            Name = name;
            Constructor = constructor;
            Methods = methods;
        }
    }
}
