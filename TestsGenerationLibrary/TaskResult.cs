using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary
{
    public class TaskResult
    {
        public string FileName { get; }
        public string FileData { get; }

        public TaskResult(string fileName, string fileData)
        {
            FileName = fileName;
            FileData = fileData;
        }
    }
}
