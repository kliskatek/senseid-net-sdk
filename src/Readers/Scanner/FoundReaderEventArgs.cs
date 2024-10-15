using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kliskatek.SenseId.Sdk.Readers.Common;

namespace Kliskatek.SenseId.Sdk.Readers.Scanner
{
    public class FoundReaderEventArgs : EventArgs
    {
        public SupportedReaderLibraries ReaderType;
        public string ConnectionString;
    }
}
