using Kliskatek.SenseId.Sdk.Readers.Common;

namespace Kliskatek.SenseId.Sdk.Readers.Scanner
{
    public class FoundReaderEventArgs : EventArgs
    {
        public SupportedReaderLibraries ReaderType;
        public string ConnectionString;
    }
}
