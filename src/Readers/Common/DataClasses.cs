namespace Kliskatek.SenseId.Sdk.Readers.Common
{
    public class SenseIdReaderInfo
    {
        public string ModelName { get; set; }
        public string Region { get; set; }
        public string FirmwareVersion { get; set; }
        public int AntennaCount { get; set; }
        public float MinTxPower { get; set; }
        public float MaxTxPower { get; set; }
    }
}
