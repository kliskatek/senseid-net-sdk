using Kliskatek.SenseId.Sdk.Readers.Scanner;

namespace Kliskatek.SenseId.Sdk.Readers.ScannerDemo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Kliskatek SenseID.SDK.Readers scanner demo");

            var scanner = new ReaderScanner();

            scanner.NewReaderFound += OnNewReaderFound;
            scanner.StartScan();
            Thread.Sleep(1000);
            scanner.StopScan();
            scanner.NewReaderFound -= OnNewReaderFound;

            var foundReaders = scanner.GetFoundReaders();
            foreach (var foundReader in foundReaders)
                Console.WriteLine($" * Reader type {foundReader.ReaderType} found with connection string {foundReader.ConnectionString}");
        }

        private static void OnNewReaderFound(object sender, FoundReaderEventArgs e)
        {
            Console.WriteLine($"New reader of type {e.ReaderType} found with connection string {e.ConnectionString}");
        }

    }
}
