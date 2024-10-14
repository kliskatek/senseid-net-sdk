using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kliskatek.SenseId.Sdk.Readers.Scanner
{
    public class ReaderScanner : IScanner
    {
        private List<IScanner> _scannerList = [];

        public ReaderScanner()
        {
            _scannerList.Add(new MulticastDnsScanner());
            _scannerList.Add(new SerialPortScanner());
        }

        public bool StartDiscovery()
        {
            foreach (var scanner in _scannerList)
            {
                scanner.NewReaderFound += OnNewReaderFound;
                scanner.StartDiscovery();
            }
            return true;
        }

        public bool StopDiscovery()
        {
            foreach (var scanner in _scannerList)
            {
                scanner.NewReaderFound -= OnNewReaderFound;
                scanner.StopDiscovery();
            }
            return true;
        }

        public event EventHandler<ReaderFoundNotificationEventArgs>? NewReaderFound;
        public List<ReaderFoundNotificationEventArgs> GetFoundReaders()
        {
            List<ReaderFoundNotificationEventArgs> returnList = [];
            foreach (var scanner in _scannerList)
                returnList.AddRange(scanner.GetFoundReaders());
            return returnList;
        }

        private void OnNewReaderFound(object sender, ReaderFoundNotificationEventArgs e)
        {
            NewReaderFound?.Invoke(this, e);
        }
    }
}
