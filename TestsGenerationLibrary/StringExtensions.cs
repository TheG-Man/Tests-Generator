using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary
{
    static class StringExtensions
    {
        public static string ToFieldName(this string target)
        {
            string fieldName = target.First().ToString().ToLower() + target.Substring(1, target.Length - 1);
            return $"_{fieldName}";
        }

        public static bool IsInterfaceName(this string target)
        {
            return (target != null && target?.Length > 1) ? target[0] == 'I' && char.IsUpper(target[1]) : false;
        }
    }
}
