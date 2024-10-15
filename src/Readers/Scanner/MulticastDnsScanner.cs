using Kliskatek.SenseId.Sdk.Readers.Common;
using Serilog;
using System.Collections.Concurrent;
using Tmds.MDns;

namespace Kliskatek.SenseId.Sdk.Readers.Scanner
{
    public class MulticastDnsScanner : IScanner
    {
        private readonly ServiceBrowser _serviceBrowser = new ServiceBrowser();
        private readonly List<string> _serviceType = new List<string> { "_llrp._tcp" };
        // private List<FoundReaderEventArgs> _foundReaders = [];

        private ConcurrentDictionary<string, FoundReaderEventArgs> _foundReaders = new();

        private readonly object _dictionaryAccess = new();

        public bool StartScan()
        {
            try
            {
                _foundReaders.Clear();
                _serviceBrowser.ServiceAdded += OnServiceAdded;
                _serviceBrowser.ServiceChanged += OnServiceChanged;
                _serviceBrowser.ServiceRemoved += OnServiceRemoved;
                _serviceBrowser.StartBrowse(_serviceType);
                Log.Information("Multicast DNS scanner started.");
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        public bool StopScan()
        {
            try
            {
                _serviceBrowser.ServiceAdded -= OnServiceAdded;
                _serviceBrowser.ServiceChanged -= OnServiceChanged;
                _serviceBrowser.ServiceRemoved -= OnServiceRemoved;
                _serviceBrowser.StopBrowse();
                Log.Information("Multicast DNS scanner stopped.");
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        public event EventHandler<FoundReaderEventArgs>? NewReaderFound;
        public List<FoundReaderEventArgs> GetFoundReaders()
        {
            lock (_dictionaryAccess)
            {
                var returnList = new List<FoundReaderEventArgs>();
                foreach (var key in _foundReaders.Keys)
                    returnList.Add(_foundReaders[key]);
                return returnList;
            }
        }

        private void OnServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            var dev = e.Announcement;
            var name = dev.Instance;
            if (!name.ToUpper().Contains("SPEEDWAY"))
                return;

            FoundReaderEventArgs foundReaderFoundArguments = new FoundReaderEventArgs
            {
                ReaderType = SupportedReaderLibraries.Octane,
                ConnectionString = dev.Addresses[0].ToString()
            };
            lock (_dictionaryAccess)
            {
                if (!_foundReaders.TryAdd(foundReaderFoundArguments.ConnectionString, foundReaderFoundArguments))
                    return;
            }
            NewReaderFound?.Invoke(this, foundReaderFoundArguments);
        }

        private void OnServiceChanged(object sender, ServiceAnnouncementEventArgs e)
        {
            Log.Information("Tmds.MDns service changed: hostname {hostname} address {address}",
                e.Announcement.Hostname,
                e.Announcement.Addresses[0]);
        }
        private void OnServiceRemoved(object sender, ServiceAnnouncementEventArgs e)
        {
            Log.Information("Tmds.MDns service removed: hostname {hostname} address {address}",
                e.Announcement.Hostname,
                e.Announcement.Addresses[0]);
        }
    }
}
