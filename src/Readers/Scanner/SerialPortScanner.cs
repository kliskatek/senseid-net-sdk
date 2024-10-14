using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Kliskatek.SenseId.Sdk.Readers.Common;
using Serilog;

namespace Kliskatek.SenseId.Sdk.Readers.Scanner
{
    public class SerialPortScanner : IScanner
    {
        private Task _scanTask;
        private CancellationTokenSource _scanCancellationTokenSource;
        private CancellationToken _scanCancellationToken;

        private ConcurrentDictionary<string, ReaderFoundNotificationEventArgs> _foundReaders = new();
        private readonly object _dictionaryAccess = new();


        public bool StartDiscovery()
        {
            _scanCancellationTokenSource = new CancellationTokenSource();
            _scanCancellationToken = _scanCancellationTokenSource.Token;
            //_scanTask = Task.Factory.StartNew(ScanSerialPort, _scanCancellationToken);
            _scanTask = ScanSerialPort(_scanCancellationToken);
            return true;
        }

        public bool StopDiscovery()
        {
            _scanCancellationTokenSource.Cancel();
            return true;
        }
        
        private async Task ScanSerialPort(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    SerialPortScanRound();
                    await Task.Delay(5000, token);
                }
                catch (OperationCanceledException e)
                {
                    Log.Information("Serial port scan task cancelled");
                }
            }
        }

        private void SerialPortScanRound()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows specific code
                try
                {
                    // As seen in https://stackoverflow.com/questions/2837985/getting-serial-port-information
                    using (var searcher = new ManagementObjectSearcher
                               ("SELECT * FROM WIN32_SerialPort"))
                    {
                        var portnames = SerialPort.GetPortNames();
                        var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                        var tList = (from n in portnames
                            join p in ports on n equals p["DeviceID"].ToString()
                            select n + " - " + p["Caption"]).ToList();

                        foreach (var port in tList)
                        {
                            string[] components = port.Split(" - ");
                            if (components.Length != 2)
                                continue;
                            var portName = components[0];
                            var portDescription = components[1];

                            if (portDescription.Contains("Silicon Lab"))
                                TryAddNewFoundReader(ReaderLibraries.RedRcp, portName, "RED4S");
                            if (portDescription.Contains("NUR Module"))
                                TryAddNewFoundReader(ReaderLibraries.NurApi, portName, "Stix");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private void TryAddNewFoundReader(ReaderLibraries readerLibrary, string serialPort, string readerName)
        {
            var connectionString = string.Empty;
            // convert serial port to URI in case of NurApi
            switch (readerLibrary)
            {
                case ReaderLibraries.NurApi:
                    connectionString = "ser://" + serialPort.ToLower();
                    break;
                default:
                    connectionString = serialPort;
                    break;
            }
            var readerFoundArguments = new ReaderFoundNotificationEventArgs
            {
                ReaderLibrary = readerLibrary,
                ReaderName = readerName,
                ConnectionString = connectionString
            };
            lock (_dictionaryAccess)
            {
                if (!_foundReaders.TryAdd(connectionString, readerFoundArguments))
                    return;
            }
            NewReaderFound?.Invoke(this, readerFoundArguments);
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
    }
}
