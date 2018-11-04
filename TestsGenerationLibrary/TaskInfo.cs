using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGenerationLibrary
{
    class TaskInfo
    {
        public bool IsGenerated { get; set; }
        public string FileName { get; set; }
        public string FileData { get; set; }

        public TaskInfo(bool isGenerated, string fileName, string fileData)
        {
            IsGenerated = isGenerated;
            FileName = fileName;
            FileData = fileData;
        }
    }
}
