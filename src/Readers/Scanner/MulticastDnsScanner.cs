using Kliskatek.SenseId.Sdk.Readers.Common;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.MDns;

namespace Kliskatek.SenseId.Sdk.Readers.Scanner
{
    public class MulticastDnsScanner : IScanner
    {
        private readonly ServiceBrowser _serviceBrowser = new ServiceBrowser();
        private readonly List<string> _serviceType = new List<string> { "_llrp._tcp" };
        // private List<ReaderFoundNotificationEventArgs> _foundReaders = [];

        private ConcurrentDictionary<string, ReaderFoundNotificationEventArgs> _foundReaders = new();

        private readonly object _dictionaryAccess = new();

        public bool StartDiscovery()
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

        public bool StopDiscovery()
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

        public event EventHandler<ReaderFoundNotificationEventArgs>? NewReaderFound;
        public List<ReaderFoundNotificationEventArgs> GetFoundReaders()
        {
            lock (_dictionaryAccess)
            {
                var returnList = new List<ReaderFoundNotificationEventArgs>();
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

            ReaderFoundNotificationEventArgs readerFoundArguments = new ReaderFoundNotificationEventArgs
            {
                ReaderLibrary = ReaderLibraries.Octane,
                ReaderName = "Speedway",
                ConnectionString = dev.Addresses[0].ToString()
            };
            lock (_dictionaryAccess)
            {
                if (!_foundReaders.TryAdd(readerFoundArguments.ConnectionString, readerFoundArguments))
                    return;
            }
            NewReaderFound?.Invoke(this, readerFoundArguments);
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
