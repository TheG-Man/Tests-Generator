using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary.MembersInfo
{
    class ParameterInfo
    {
        public string TypeName { get; }
        public string Name { get; }

        public ParameterInfo(string typeName, string name)
        {
            TypeName = typeName;
            Name = name;
        }
    }
}
