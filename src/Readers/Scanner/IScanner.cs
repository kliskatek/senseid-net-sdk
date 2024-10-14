using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kliskatek.SenseId.Sdk.Readers.Scanner
{
    public interface IScanner
    {
        bool StartDiscovery();

        bool StopDiscovery();

        event EventHandler<ReaderFoundNotificationEventArgs> NewReaderFound;

        List<ReaderFoundNotificationEventArgs> GetFoundReaders();
    }
}
