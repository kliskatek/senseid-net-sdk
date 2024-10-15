using Kliskatek.SenseId.Sdk.Readers.Common;
using NurApiDotNet;
using Serilog;

namespace Kliskatek.SenseId.Sdk.Readers.Rfid
{
    public class SenseIdNurApiReader : SenseIdReaderBase
    {
        private NurApi _reader = null;
        protected NurApi.InventoryExFilter[] _inventoryFilterArray;
        private NurApi.InventoryExParams _inventoryParameters;
        private SenseIdReaderCallback _callback;
        private AutoResetEvent mConnectedEvent = new AutoResetEvent(false);

        public SenseIdNurApiReader()
        {
            // Add serial port support. 
            // NOTE: Needs NordicID.NurApi.SerialTransport reference
            NurApiDotNet.SerialTransport.Support.Init();
        }

        protected override bool ConnectLowLevel(string connectionString)
        {
            try
            {
                _reader = new NurApi();
                _reader.ConnectionStatusEvent += OnConnectionStatusEvent;
                _reader.Connect(connectionString);
                if (!mConnectedEvent.WaitOne(1000))
                {
                    Log.Warning("Could not connect to reader");
                    _reader.Disconnect();
                    return false;
                }

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
                _reader.ConnectionStatusEvent -= OnConnectionStatusEvent;
                _reader.Disconnect();
                return !_reader.IsConnected();
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
                var info = _reader.GetReaderInfo();
                var version = _reader.GetVersions();
                var caps = _reader.GetDeviceCaps();

                ReaderInfo = new SenseIdReaderInfo();
                ReaderInfo.AntennaCount = info.numAntennas;
                ReaderInfo.FirmwareVersion = $"{info.swVerMajor}.{info.swVerMinor}";

                ReaderInfo.MaxTxPower = caps.maxTxdBm;
                ReaderInfo.MinTxPower = ReaderInfo.MaxTxPower - caps.txAttnStep * (caps.txSteps - 1);
                ReaderInfo.ModelName = info.name;
                ReaderInfo.Region = _reader.Region.ToString();
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

                var setup = _reader.GetModuleSetup();
                var caps = _reader.GetDeviceCaps();
                setup.txLevel = (int)(ReaderInfo.MaxTxPower - txPower * caps.txAttnStep);
                _reader.SetModuleSetup(NurApi.SETUP_PERANTPOWER | NurApi.SETUP_TXLEVEL, ref setup);
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
                var setup = _reader.GetModuleSetup();
                byte antennaMask = 0;
                for (int i = 0; i < ReaderInfo.AntennaCount; i++)
                {
                    if (i >= antennaConfigArray.Length)
                        break;
                    if (antennaConfigArray[i])
                        antennaMask |= (byte)(1 << i);
                }

                setup.selectedAntenna = NurApi.ANTENNAID_AUTOSELECT;
                setup.antennaMask = antennaMask;
                _reader.SetModuleSetup(NurApi.SETUP_ANTMASK | NurApi.SETUP_SELECTEDANTENNA, ref setup);

                for (int i = 0; i < EnabledAntennas.Length; i++)
                    EnabledAntennas[i] = i < antennaConfigArray.Length && antennaConfigArray[i];
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
                _reader.InventoryExEvent += OnInventoryStreamEvent;
                _callback = callback;
                _reader.StartInventoryEx(ref _inventoryParameters, _inventoryFilterArray);
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                // TODO: review new line
                //_reader.InventoryExEvent -= OnInventoryStreamEvent;
                return false;
            }
        }

        protected override bool StopDataAcquisitionAsyncLowLevel()
        {
            try
            {
                _reader.StopInventoryEx();
                _reader.InventoryExEvent -= OnInventoryStreamEvent;
                _reader.ClearTagsEx();
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
                var setup = _reader.GetModuleSetup();
                setup.inventoryQ = 5;
                setup.inventoryTarget = NurApi.INVTARGET_A;
                setup.inventorySession = 1;
                setup.rxDecoding = NurApi.RXDECODING_M4;
                setup.txModulation = NurApi.TXMODULATION_PRASK;

                _reader.SetModuleSetup(NurApi.SETUP_ALL, ref setup);

                // Set filtering
                _inventoryFilterArray = new NurApi.InventoryExFilter[1];
                byte[] mask = NurApi.HexStringToBin("E2");
                _inventoryFilterArray[0].action = NurApi.FACTION_0;
                _inventoryFilterArray[0].address = 0;
                _inventoryFilterArray[0].bank = NurApi.BANK_TID;
                _inventoryFilterArray[0].maskBitLength = 8;
                _inventoryFilterArray[0].maskData = new byte[NurApi.MAX_SELMASK];
                Buffer.BlockCopy(mask, 0, _inventoryFilterArray[0].maskData, 0, mask.Length);
                // TODO: Review this line
                _inventoryFilterArray[0].target = NurApi.SESSION_S1;

                // Inventory parameters
                _inventoryParameters = new NurApi.InventoryExParams();
                _inventoryParameters.inventorySelState = NurApi.SELSTATE_ALL;
                _inventoryParameters.inventoryTarget = NurApi.INVTARGET_A;
                _inventoryParameters.session = NurApi.SESSION_S1;
                _inventoryParameters.Q = 5;
                _inventoryParameters.rounds = _reader.InventoryRounds;
                _inventoryParameters.transitTime = 0;
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Exception thrown");
                return false;
            }
        }

        private void OnInventoryStreamEvent(object sender, NurApi.InventoryStreamEventArgs e)
        {
            if (e.data.tagsAdded >= 0)
            {
                foreach (NurApi.Tag t in _reader.GetTagStorage())
                    _callback(t.GetEpcString());
            }
            _reader.ClearTags();

            if (e.data.stopped)
            {
                // Restart inventory
            }
        }

        private void OnConnectionStatusEvent(object? sender, NurTransportStatus e)
        {
            if (e == NurTransportStatus.Connected)
                mConnectedEvent.Set();
        }
    }
}
