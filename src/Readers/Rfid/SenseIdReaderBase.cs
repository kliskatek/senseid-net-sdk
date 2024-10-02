using Kliskatek.SenseId.Sdk.Readers.Common;

using Serilog;

namespace Kliskatek.SenseId.Sdk.Readers.Rfid
{
    public abstract class SenseIdReaderBase : ISenseIdReader
    {
        private static readonly object CommandExecutionLock = new object();
        protected ReaderConnectionStatus ConnectionStatus = ReaderConnectionStatus.Disconnected;
        protected ReaderStatus ReaderStatus = ReaderStatus.Idle;
        protected SenseIdReaderInfo ReaderInfo = new SenseIdReaderInfo();
        protected float TxPower = 0;
        protected bool[] AntennaConfig = new bool[] { false };

        private bool[] GetInitialAntennaConfig()
        {
            bool[] initialAntennaConfig = new bool[ReaderInfo.AntennaCount];
            initialAntennaConfig[0] = true;
            return initialAntennaConfig;
        }

        public bool Connect(string connectionString)
        {
            lock (CommandExecutionLock)
            {
                try
                {
                    if (ConnectionStatus == ReaderConnectionStatus.Connected)
                    {
                        Log.Information("Reader connection discarded: a connection with the reader was already established.");
                        return false;
                    }

                    if (!ConnectLowLevel(connectionString))
                        return false;
                    if (!GetReaderInfo())
                        return false;
                    SetTxPowerLowLevel(ReaderInfo.MaxTxPower);
                    AntennaConfig = new bool[ReaderInfo.AntennaCount];
                    SetAntennaConfigLowLevel(GetInitialAntennaConfig());
                    ConnectionStatus = ReaderConnectionStatus.Connected;
                    ReaderStatus = ReaderStatus.Idle;
                    return true;
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Exception thrown while connecting to reader");
                    return false;
                }
            }
        }

        public bool Disconnect()
        {
            lock (CommandExecutionLock)
            {
                try
                {
                    if (ConnectionStatus == ReaderConnectionStatus.Disconnected)
                    {
                        Log.Information("Reader disconnection discarded: no previous connection with the reader was established");
                        return false;
                    }

                    if (!DisconnectLowLevel())
                        return false;
                    ConnectionStatus = ReaderConnectionStatus.Disconnected;
                    ReaderStatus = ReaderStatus.Idle;
                    return true;
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Exception thrown while connecting to reader");
                    return false;
                }
            }
        }

        public SenseIdReaderInfo GetInfo()
        {
            lock (CommandExecutionLock)
                return ReaderInfo;
        }

        public float GetTxPower()
        {
            lock (CommandExecutionLock)
                return TxPower;
        }

        public bool SetTxPower(float txPower)
        {
            lock (CommandExecutionLock)
            {
                if (ConnectionStatus == ReaderConnectionStatus.Disconnected)
                {
                    Log.Warning("Can not change TX power of an unconnected reader");
                    return false;
                }
                //if (ReaderStatus != ReaderStatus.BusyInventory) return SetTxPowerLowLevel(txPower);
                //Log.Warning("Reader must be idle before changing TX power");
                //return false;
                return SetTxPowerLowLevel(txPower);
            }
        }

        public bool[] GetAntennaConfig()
        {
            lock (CommandExecutionLock)
                return AntennaConfig;
        }

        public bool SetAntennaConfig(bool[] antennaConfigArray)
        {
            lock (CommandExecutionLock)
            {
                if (ConnectionStatus == ReaderConnectionStatus.Disconnected)
                {
                    Log.Warning("Can not change antenna configuration of an unconnected reader");
                    return false;
                }
                //if (ReaderStatus != ReaderStatus.BusyInventory) return SetAntennaConfigLowLevel(antennaConfigArray);
                //Log.Warning("Reader must be idle before changing antenna configuration");
                //return false;
                return SetAntennaConfigLowLevel(antennaConfigArray);
            }
        }

        public bool StartDataAcquisitionAsync(SenseIdReaderCallback callback)
        {
            lock (CommandExecutionLock)
            {
                if (ConnectionStatus == ReaderConnectionStatus.Disconnected)
                {
                    Log.Warning("Reader is disconnected");
                    return false;
                }
                if (ReaderStatus == ReaderStatus.BusyInventory)
                {
                    Log.Warning("Data acquisition was already enabled");
                    return false;
                }
                if (!StartDataAcquisitionAsyncLowLevel(callback))
                    return false;
                ReaderStatus = ReaderStatus.BusyInventory;
                return true;
            }
        }

        public bool StopDataAcquisitionAsync()
        {
            lock (CommandExecutionLock)
            {
                if (ConnectionStatus == ReaderConnectionStatus.Disconnected)
                {
                    Log.Warning("Reader is disconnected");
                    return false;
                }
                if (ReaderStatus == ReaderStatus.Idle)
                {
                    Log.Warning("Data acquisition had not started.");
                    return false;
                }
                if (!StopDataAcquisitionAsyncLowLevel())
                    return false;
                ReaderStatus = ReaderStatus.Idle;
                return true;
            }
        }

        protected abstract bool ConnectLowLevel(string connectionString);
        protected abstract bool DisconnectLowLevel();
        protected abstract bool GetReaderInfo();
        protected abstract bool SetTxPowerLowLevel(float txPower);
        protected abstract bool SetAntennaConfigLowLevel(bool[] antennaConfigArray);
        protected abstract bool StartDataAcquisitionAsyncLowLevel(SenseIdReaderCallback callback);
        protected abstract bool StopDataAcquisitionAsyncLowLevel();
    }
}

