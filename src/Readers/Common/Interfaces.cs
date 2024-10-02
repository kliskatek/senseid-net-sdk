namespace Kliskatek.SenseId.Sdk.Readers.Common
{
    public interface ISenseIdReader
    {
        bool Connect(string connectionString);

        bool Disconnect();

        SenseIdReaderInfo GetInfo();

        float GetTxPower();

        bool SetTxPower(float txPower);

        bool[] GetAntennaConfig();

        bool SetAntennaConfig(bool[] antennaConfigArray);

        bool StartDataAcquisitionAsync(SenseIdReaderCallback callback);

        bool StopDataAcquisitionAsync();
    }
}
