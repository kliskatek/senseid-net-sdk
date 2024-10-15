namespace Kliskatek.SenseId.Sdk.Readers.Scanner
{
    public interface IScanner
    {
        bool StartScan();

        bool StopScan();

        event EventHandler<FoundReaderEventArgs> NewReaderFound;

        List<FoundReaderEventArgs> GetFoundReaders();
    }
}
