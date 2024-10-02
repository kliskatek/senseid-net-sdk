using Kliskatek.SenseId.Sdk.Parsers.Common;

namespace Kliskatek.SenseId.Sdk.Parsers.Rain
{
    internal class RawSenseIdDefinitions
    {
        public string version { get; set; }
        public string date { get; set; }
        public int[] pen_header { get; set; }
        public Dictionary<string, RawSenseIdTagType> types { get; set; }
    }

    internal class RawSenseIdTagType
    {
        public string name { get; set; }
        public string description { get; set; }
        public RawSenseIdDataDefinitions[] data_def { get; set; }
    }

    internal class RawSenseIdDataDefinitions
    {
        public string magnitude { get; set; }
        public string unit_long { get; set; }
        public string unit_short { get; set; }
        public string type { get; set; }
        public string transform { get; set; }
        public double[] coefficients { get; set; }
    }

    public class SenseIdDefinitions
    {
        public string version { get; set; }
        public DateTime date { get; set; }
        public byte[] pen_header { get; set; }
        public Dictionary<byte[], SenseIdTagType> types { get; set; }
    }
}