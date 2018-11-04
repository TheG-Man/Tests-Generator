using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary.MembersInfo
{
    class ConstructorInfo : MethodInfo
    {
        public IEnumerable<ParameterInfo> InterfaceParameters { get; }

        public ConstructorInfo(IEnumerable<ParameterInfo> interfaceParameters, IEnumerable<ParameterInfo> parameters)
            : base(null, null, parameters)
        {
            InterfaceParameters = interfaceParameters;
        }
    }
}
