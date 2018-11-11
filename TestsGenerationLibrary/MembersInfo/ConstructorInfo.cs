using System.Collections.Generic;

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
