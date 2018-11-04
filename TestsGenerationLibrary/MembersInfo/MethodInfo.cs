using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary.MembersInfo
{
    class MethodInfo
    {
        public string Name { get; }
        public string ReturnTypeName { get; }
        public IEnumerable<ParameterInfo> Parameters { get; }

        public MethodInfo(string name, string returnTypeName, IEnumerable<ParameterInfo> parameters)
        {
            Name = name;
            ReturnTypeName = returnTypeName;
            Parameters = parameters;
        }
    }
}
