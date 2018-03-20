using System;
using System.IO;
using System.Reflection;
using Taxi.DataAccess.Censore;
using Taxi.DataAccess.RegionInfo;

namespace Taxi.Presentation.Console
{
    class Program
    {
        private const string trafficFilePath = @"c:\trafic-log.txt";

        static void Main(string[] args)
        {
            StartBlock();
        }

        private static void StartBlock()
        {
            System.Console.WriteLine("Type 'traffic' to get data about traffic.");
            System.Console.WriteLine("Type 'censore' to try Censore feature.");

            var userInput = System.Console.ReadLine();

            if (userInput.IndexOf("traffic", StringComparison.OrdinalIgnoreCase) >= 0)
                CallTrafficBlock();

            if (userInput.IndexOf("censore", StringComparison.OrdinalIgnoreCase) >= 0)
                CallCensoreBlock();

            FinishBlock();
        }

        private static void FinishBlock()
        {
            System.Console.WriteLine("\nPress R to start again or any other key to close.");

            var userInput = System.Console.ReadLine();

            if (userInput == "R" || userInput == "r")
            {
                System.Console.WriteLine("\r\n");
                StartBlock();
            }
            else
                Environment.Exit(0);
        }

        private static void CallTrafficBlock()
        {
            System.Console.WriteLine("Use fake data (Y/N)?");
            var userInputForFakeData = System.Console.ReadLine();

            System.Console.WriteLine("Type region code.");

            var userInputRegionCode = System.Console.ReadLine();
            int regionCode = 0;

            if (int.TryParse(userInputRegionCode, out regionCode))
            {
                //set parametr useFakeData for manager to true if you don't have access to yandex API
                var result = new RegionInfoManager((userInputForFakeData == "Y" || userInputForFakeData == "y") ? GetFakeDataPath() : null).GetTraffic(regionCode);

                try
                {
                    using (var sw = File.AppendText(trafficFilePath))
                    {
                        sw.WriteLine($"{result.RegionName} - {result.RegionId} {result.Comment} - {result.ResultMessage}");
                    }
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return;
                }

                if (result.HasError)
                    System.Console.WriteLine($"Error: {result.ResultMessage}");

                System.Console.WriteLine($"File stored in {trafficFilePath}");
            }
            else
                System.Console.WriteLine("Can't convert input into region code.");
        }

        private static void CallCensoreBlock()
        {
            System.Console.WriteLine("Type text to validate.");

            var userInput = System.Console.ReadLine();

            System.Console.WriteLine("\nResult:");
            System.Console.WriteLine(userInput.FilterText());
        }

        private static string GetFakeDataPath()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return currentFolder + @"\FakeReginfo.xml";
        }
    }
}
