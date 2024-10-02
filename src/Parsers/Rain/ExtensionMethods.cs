namespace Kliskatek.SenseId.Sdk.Parsers.Rain
{
    public static class ExtensionMethods
    {
        public static RainSenseIdTag ToRainSenseIdTag(this string? epc)
        {
            return new RainSenseIdTag(epc);
        }

        public static RainSenseIdTag ToRainSenseIdTag(this byte[] epc)
        {
            return new RainSenseIdTag(epc);
        }
    }
}