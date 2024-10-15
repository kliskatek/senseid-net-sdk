using Impinj.OctaneSdk;
using Kliskatek.SenseId.Sdk.Readers.Common;
using Serilog;

namespace Kliskatek.SenseId.Sdk.Readers.Rfid
{
    public class SenseIdOctaneReader : SenseIdReaderBase
    {
        private ImpinjReader _reader = null;
        private Settings _settings = null;
        private SenseIdReaderCallback _callback;

        protected override bool ConnectLowLevel(string connectionString)
        {
            try
            {
                _reader = new ImpinjReader();
                _reader.Connect(connectionString);

                if (!_reader.IsConnected)
                {
                    Log.Warning("Could not connect to reader");
                    return false;
                }

                _settings = _reader.QueryDefaultSettings();
                if (!ConfigureReader())
                {
                    Log.Warning("Could not configure reader");
                    return false;
                }
                Log.Information("Reader connected");
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        protected override bool DisconnectLowLevel()
        {
            try
            {
                _reader.Disconnect();
                if (!_reader.IsConnected)
                {
                    Log.Information("Reader disconnected");
                    return true;
                }
                Log.Warning("Could not disconnect from reader");
                return false;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        protected override bool GetReaderInfo()
        {
            try
            {
                FeatureSet fs = _reader.QueryFeatureSet();
                ReaderInfo = new SenseIdReaderInfo();
                ReaderInfo.AntennaCount = (int)fs.AntennaCount;
                ReaderInfo.FirmwareVersion = fs.FirmwareVersion;
                ReaderInfo.MaxTxPower = (float)fs.TxPowers.Last().Dbm;
                ReaderInfo.MinTxPower = (float)fs.TxPowers.First().Dbm;
                ReaderInfo.ModelName = fs.ModelName;
                ReaderInfo.Region = fs.CommunicationsStandard.ToString();
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        protected override bool SetTxPowerLowLevel(float txPower)
        {
            try
            {
                if (txPower > ReaderInfo.MaxTxPower)
                {
                    Log.Error($"Requested power {txPower} exceeds reader's upper power limit ({ReaderInfo.MaxTxPower})");
                    return false;
                }

                if (txPower < ReaderInfo.MinTxPower)
                {
                    Log.Error($"Requested power {txPower} is lower than the reader's lower power limit ({ReaderInfo.MinTxPower})");
                    return false;
                }

                for (int i = 0; i < ReaderInfo.AntennaCount; i++)
                {
                    _settings.Antennas.GetAntenna((ushort)(i + 1)).TxPowerInDbm = txPower;
                }
                _reader.ApplySettings(_settings);
                TxPower = txPower;
                return true;

            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        protected override bool SetEnabledAntennasLowLevel(bool[] antennaConfigArray)
        {
            try
            {
                for (int i = 0; i < ReaderInfo.AntennaCount; i++)
                    _settings.Antennas.GetAntenna((ushort)(i + 1)).IsEnabled =
                        i < antennaConfigArray.Length && antennaConfigArray[i];
                _reader.ApplySettings(_settings);

                for (int i = 0; i < EnabledAntennas.Length; i++)
                    EnabledAntennas[i] = i < antennaConfigArray.Length && antennaConfigArray[i];

                Log.Information("Enabled antennas updated");
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        private bool ConfigureReader()
        {
            try
            {
                _settings.Report.Mode = ReportMode.Individual;
                _settings.Report.IncludeAntennaPortNumber = true;
                _settings.Report.IncludeCrc = true;
                _settings.Report.IncludeChannel = true;
                _settings.Report.IncludePeakRssi = true;
                _settings.Report.IncludePhaseAngle = true;
                _settings.Report.IncludePcBits = true;
                _settings.Report.IncludeSeenCount = true;

                //_settings.SearchMode = SearchMode.ReaderSelected;
                _settings.SearchMode = SearchMode.DualTarget;

                _settings.Session = 1;
                _settings.ReaderMode = ReaderMode.DenseReaderM4; // TODO: obsolete
                _settings.TagPopulationEstimate = 32;

                //_settings.Filters.TagFilter1.MemoryBank = MemoryBank.Tid;
                //_settings.Filters.TagFilter1.BitPointer = 0;
                //_settings.Filters.TagFilter1.BitCount = 8;
                //_settings.Filters.TagFilter1.TagMask = "E2";
                //_settings.Filters.TagFilter1.FilterOp = TagFilterOp.Match;
                //_settings.Filters.Mode = TagFilterMode.OnlyFilter1;

                _settings.Antennas.RxSensitivityMax = true;


                //_settings.RfMode = 2;
                _reader.ApplySettings(_settings);
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        protected override bool StartDataAcquisitionAsyncLowLevel(SenseIdReaderCallback callback)
        {
            try
            {
                _reader.DeleteAllOpSequences();

                _reader.TagsReported += OnInventoryTagsReported;
                _callback = callback;
                _reader.Start();
                Log.Information("Data acquisition started");
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        protected override bool StopDataAcquisitionAsyncLowLevel()
        {
            try
            {
                _reader.Stop();
                _reader.DeleteAllOpSequences();
                _reader.TagsReported -= OnInventoryTagsReported;
                Log.Information("Data acquisition stopped");
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }


        private void OnInventoryTagsReported(ImpinjReader reader, TagReport tagReport)
        {
            foreach (var t in tagReport.Tags)
                _callback(t.Epc.ToHexString());
        }
    }
}

