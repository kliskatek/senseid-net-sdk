namespace Kliskatek.SenseId.Sdk.Parsers.Common
{
    public static class ExtensionMethods
    {
        public static SenseIdTag ToSenseIdTag(this string? hexString, Technologies technology)
        {
            switch (technology)
            {
                case Technologies.Rain:
                    return Rain.ExtensionMethods.ToRainSenseIdTag(hexString);
                default:
                    throw new NotImplementedException($"Technology {technology} is not implemented yet");
            }
        }

        public static SenseIdTag ToSenseIdTag(this byte[] byteArray, Technologies technology)
        {
            switch (technology)
            {
                case Technologies.Rain:
                    return Rain.ExtensionMethods.ToRainSenseIdTag(byteArray);
                default:
                    throw new NotImplementedException($"Technology {technology} is not implemented yet");
            }
        }
    }
}
