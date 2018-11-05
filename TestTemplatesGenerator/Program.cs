using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.IO;
using TestsGenerationLibrary;
using TestsGenerationLibrary.Consumers;

namespace TestTemplatesGenerator
{
    class Program
    {
        private const int MinParametersCount = 5;
        private const string UsageInfo = 
            "Usage: <source_file_path> [<source_file_path>] <target_dir_path>"
            + "<max_count_tasks_to_read> <max_count_tasks_to_process> <max_count_tasks_to_write>";
        private const string SuccessedMessage =
            "Tests have been generated successfully";

        static int Main(string[] args)
        {
            if (args.Length < MinParametersCount)
            {
                Console.WriteLine(UsageInfo);
                Console.ReadKey();
                return 1;
            }

            List<string> filePaths = new List<string>();
            string targetDirectoryPath = args.ElementAt(args.Length - 4);
            int maxCountTasksToRead = int.Parse(args.ElementAt(args.Length - 3));
            int maxCountTasksToProcess = int.Parse(args.ElementAt(args.Length - 2));
            int maxCountTasksToWrite = int.Parse(args.ElementAt(args.Length - 1));
            var restrictions = new TestsGeneratorRestrictions(maxCountTasksToRead, maxCountTasksToProcess, maxCountTasksToWrite);

            for (int i = 0; i < args.Length - 4; ++i)
            {
                filePaths.Add(args[i]);
            }
            
            var testsGenerator = new TestsGenerator(new FileConsumer(targetDirectoryPath), restrictions);

            try
            {
                testsGenerator.Generate(filePaths);
            }
            catch (AggregateException aggregateException)
            {
                foreach (Exception exception in aggregateException.InnerExceptions)
                {
                    Console.WriteLine(exception.Message);
                }
            }

            Console.WriteLine(SuccessedMessage);
            Console.ReadKey();

            return 0;
        } 
    }
}
