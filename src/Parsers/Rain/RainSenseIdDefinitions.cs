namespace Kliskatek.SenseId.Sdk.Parsers.Rain
{
    public static class RainSenseIdDefinitions
    {
        public static SenseIdDefinitions Definitions => YamlDefinitionParser.Instance.GetDefinitions();
    }
}