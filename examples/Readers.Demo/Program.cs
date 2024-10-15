using Kliskatek.SenseId.Sdk.Parsers.Rain;
using Kliskatek.SenseId.Sdk.Readers.Common;
using Kliskatek.SenseId.Sdk.Readers.Rfid.Nordic;
using Kliskatek.SenseId.Sdk.Readers.Rfid.Impinj;
using Kliskatek.SenseId.Sdk.Readers.Rfid.Phychips;
using Kliskatek.SenseId.Sdk.Readers.Scanner;
using Microsoft.Extensions.DependencyInjection;

namespace Kliskatek.SenseId.Sdk.Readers.Demo
{
    // Adapted from https://github.com/NordicID/nur_sample_csharp
    internal class Program
    {
        private static bool _runProgram = true;
        // List of operations
        // Tuple:
        //  - Item1 = char to start op
        //  - Item2 = string to print
        //  - Item3 = function to call
        //  - Item4 = if true, this op is allowed only when connected
        private static List<Tuple<char, string, Action, bool>> _operations =
        [
            Tuple.Create('1', "Scan for readers", ReaderScan, false),
            // Tuple.Create('2', "Stop scanning for readers", StopScan, false),
            Tuple.Create('2', "List found readers", ListFoundReaders, false),
            Tuple.Create('3', "Connect to reader manually", ManualConnectReader, false),
            Tuple.Create('4', "Connect to found reader", ConnectFoundReader, false),
            Tuple.Create('5', "Start data acquisition", StartDataAcquisition, true),
            Tuple.Create('6', "Disconnect from reader", DisconnectReader, true),
            Tuple.Create('x', "Exit program", ExitProgram, false)
        ];

        private static ServiceCollection _services = new ServiceCollection();
        private static ServiceProvider _provider;

        private static bool _isReaderConnected = false;
        private static ISenseIdReader? _reader;

        private static Dictionary<SupportedReaderLibraries, ISenseIdReader> _diDictionary = new();
        
        private static ReaderScanner _scanner = new ReaderScanner();

        public static void Main(string[] args)
        {
            Console.WriteLine("Kliskatek SenseID.SDK.Readers demo");

            // Configure DI container
            _services
                .AddKeyedScoped<ISenseIdReader, OctaneReader>(SupportedReaderLibraries.Octane)
                .AddKeyedScoped<ISenseIdReader, NurApiReader>(SupportedReaderLibraries.NurApi)
                .AddKeyedScoped<ISenseIdReader, Red4SReader>(SupportedReaderLibraries.RedRcp);
            _provider = _services.BuildServiceProvider();

            // Configure scanner
            _scanner.NewReaderFound += OnNewReaderFound;
            
            while (_runProgram)
            {
                Console.WriteLine();
                Console.WriteLine("Select operation");
                foreach (var op in _operations)
                {
                    if ((op.Item4 != _isReaderConnected) & (!_isReaderConnected))
                        continue;
                    Console.WriteLine($"{op.Item1} - {op.Item2}");
                }

                Console.WriteLine(">>");

                try
                {
                    var line = Console.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var operationKey = line[0];
                        foreach (var op in _operations)
                        {
                            if ((op.Item4 != _isReaderConnected))
                                continue;
                            if (op.Item1 == operationKey)
                            {
                                op.Item3();
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception thrown : {e.Message}");
                }
            }
        }

        private static void OnNewReaderFound(object sender, FoundReaderEventArgs e)
        {
            Console.WriteLine($"New reader found : {e.ReaderType} @ {e.ConnectionString}");
        }


        private static void ReaderScan()
        {
            int scanTime = 0;
            while (true)
            {
                Console.WriteLine("Enter scanning time (milliseconds)");
                Console.WriteLine(">>");
                var line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    try
                    {
                        scanTime = Int32.Parse(line);
                        if (scanTime > 0)
                            break;
                        Console.WriteLine("Scanning time must be greater than 0 milliseconds");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            _scanner.StartScan();
            Thread.Sleep(scanTime);
            _scanner.StopScan();
        }

        private static void ListFoundReaders()
        {
            foreach (var foundReader in _scanner.GetFoundReaders())
                Console.WriteLine($" * {foundReader.ReaderType} @ {foundReader.ConnectionString}");
        }

        private static void ManualConnectReader()
        {
            List<Tuple<int, SupportedReaderLibraries, string>> readerTypeOptions = [];
            int counter = 1;
            foreach (var item in Enum.GetValues(typeof(SupportedReaderLibraries)))
                readerTypeOptions.Add(Tuple.Create(counter++, (SupportedReaderLibraries)item, ((SupportedReaderLibraries)item).ToString()));
            SupportedReaderLibraries readerType = SupportedReaderLibraries.NurApi;
            bool getReaderType = true;
            while (getReaderType)
            {
                Console.WriteLine("Enter reader type");
                foreach (var item in readerTypeOptions)
                    Console.WriteLine($"{item.Item1} - {item.Item3.ToString()}");
                Console.WriteLine(">>");
                var line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    try
                    {
                        var testOption = Int32.Parse(line);
                        foreach (var item in readerTypeOptions)
                        {
                            if (item.Item1 == testOption)
                            {
                                readerType = item.Item2;
                                getReaderType = false;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
            }
            Console.WriteLine("Enter reader connection string");
            Console.WriteLine(">>");
            var connectionString = Console.ReadLine();
            ConnectReader(readerType, connectionString);
        }

        private static void ConnectFoundReader()
        {
            var foundReaders = _scanner.GetFoundReaders();
            //bool selectFoundReader = true;
            while (true)
            {
                Console.WriteLine("Select reader to connect to");
                int index = 0;
                foreach (var foundReader in foundReaders)
                    Console.WriteLine($" {index++} - {foundReader.ReaderType} @ {foundReader.ConnectionString}");
                Console.WriteLine(">>");
                var line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    try
                    {
                        var selectedIndex = Int32.Parse(line);
                        var selectedReader = foundReaders.ElementAt(selectedIndex);
                        if (selectedReader != null)
                        {
                            ConnectReader(selectedReader.ReaderType, selectedReader.ConnectionString);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
        }

        private static void ConnectReader(SupportedReaderLibraries readerLibrary, string connectionString)
        {

            _reader = _provider.GetKeyedService<ISenseIdReader>(readerLibrary);
            _isReaderConnected = _reader.Connect(connectionString);
        }

        private static void StartDataAcquisition()
        {
            int dataAcquisitionTime = 0;
            while (true)
            {
                Console.WriteLine("Enter data acquisition time (milliseconds)");
                Console.WriteLine(">>");
                var line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    try
                    {
                        dataAcquisitionTime = Int32.Parse(line);
                        if (dataAcquisitionTime > 0)
                            break;
                        Console.WriteLine("Data acquisition time must be greater than 0 milliseconds");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            _reader.StartDataAcquisitionAsync(OnNewEpcReceived);
            Thread.Sleep(dataAcquisitionTime);
            _reader.StopDataAcquisitionAsync();
        }

        private static void DisconnectReader()
        {
            _isReaderConnected = !_reader.Disconnect();
        }

        private static void ExitProgram()
        {
            _runProgram = false;
        }

        private static void OnNewEpcReceived(string epc)
        {
            var parsedEpc = epc.ToRainSenseIdTag();
            Console.WriteLine($"New EPC received : {epc} [{parsedEpc.Name}]");

            if (!String.Equals(parsedEpc.Name, Kliskatek.SenseId.Sdk.Parsers.Rain.Constants.RegularTagName))
            {
                Console.WriteLine($"SenseID data report");
                Console.WriteLine($"  * Name = {parsedEpc.Name}");
                Console.WriteLine($"  * Description = {parsedEpc.Description}");
                Console.WriteLine($"  * ID = 0x{parsedEpc.Id}");
                foreach (var data in parsedEpc.Data)
                    Console.WriteLine($"    - [{data.Magnitude}] : {data.Value} {data.UnitLong}");
            }
        }
    }
}