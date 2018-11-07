using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.IO;
using TestsGenerationLibrary;
using TestsGenerationLibrary.Consumers;
using TestsGenerationLibrary.SourceCodeProviders;

namespace TestTemplatesGenerator
{
    class Program
    {
        private const int MinParametersCount = 4;
        private const string UsageInfo = 
            "Usage: <source_file_path> [<source_file_path>] <target_dir_path>"
            + "<max_count_tasks_to_read> <max_count_tasks_to_process> <max_count_tasks_to_write>";
        private const string SuccessedMessage = "Tests have been generated successfully and saved to:";
        private const string ErrorPromt = "The errors have been occurred:";

        static int Main(string[] args)
        {
            if (args.Length < MinParametersCount)
            {
                Console.WriteLine(UsageInfo);
                Console.ReadKey();
                return 1;
            }

            List<string> filePaths = new List<string>();
            string targetDirectoryPath = args.ElementAt(args.Length - 3);
            int maxCountTasksToProcess = int.Parse(args.ElementAt(args.Length - 2));
            int maxCountTasksToWrite = int.Parse(args.ElementAt(args.Length - 1));
            var restrictions = new TestsGeneratorRestrictions(maxCountTasksToProcess, maxCountTasksToWrite);

            for (int i = 0; i < args.Length - 3; ++i)
            {
                filePaths.Add(args[i]);
            }

            IEnumerable<string> generatedFilePaths = GetGeneratedFilesPaths(targetDirectoryPath, restrictions, filePaths).ToList();
            WriteResults(generatedFilePaths);

            GenerateTestsAndWriteToConsole(restrictions, filePaths);

            Console.ReadKey();

            return 0;
        } 

        private static IEnumerable<string> GetGeneratedFilesPaths(string targetDirectoryPath, TestsGeneratorRestrictions restrictions, IEnumerable<string> filePaths)
        {
            List<ConsumerResult<string>> generatedFilePaths = new List<ConsumerResult<string>>();

            try
            {
                var fileSourceCodeProvider = new FileSourceCodeProvider(filePaths);
                var fileConsumer = new FileConsumer(targetDirectoryPath);

                var testsGenerator = new TestsGenerator(restrictions);
                generatedFilePaths = testsGenerator.Generate(fileSourceCodeProvider, fileConsumer).ToList();
            }
            catch (AggregateException aggregateException)
            {
                Console.WriteLine(ErrorPromt);
                foreach (Exception exception in aggregateException.InnerExceptions)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            catch (ArgumentOutOfRangeException argOutOfRangeException)
            {
                Console.WriteLine(ErrorPromt);
                Console.WriteLine(argOutOfRangeException.Message);
            }

            return generatedFilePaths.Select(x => x.Result).Cast<string>();
        }

        private static void WriteResults(IEnumerable<string> generatedFilePaths)
        {
            if (generatedFilePaths.Any())
            {
                Console.WriteLine(SuccessedMessage);

                for (int i = 0; i < generatedFilePaths.Count(); ++i)
                {
                    string filePath = generatedFilePaths.ElementAt(i);

                    Console.WriteLine($"Test template {i}:");
                    Console.WriteLine($"\tName: {Path.GetFileName(filePath)}");
                    Console.WriteLine($"\tPath: {filePath}");
                }
            }
        }

        private static void GenerateTestsAndWriteToConsole(TestsGeneratorRestrictions restrictions, IEnumerable<string> filePaths)
        {
            try
            {
                var fileSourceCodeProvider = new FileSourceCodeProvider(filePaths);
                var consoleConsumer = new ConsoleConsumer();

                var testsGenerator = new TestsGenerator(restrictions);
                testsGenerator.Generate(fileSourceCodeProvider, consoleConsumer);
            }
            catch (AggregateException aggregateException)
            {
                Console.WriteLine(ErrorPromt);
                foreach (Exception exception in aggregateException.InnerExceptions)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            catch (ArgumentOutOfRangeException argOutOfRangeException)
            {
                Console.WriteLine(ErrorPromt);
                Console.WriteLine(argOutOfRangeException.Message);
            }
        }
    }
}
