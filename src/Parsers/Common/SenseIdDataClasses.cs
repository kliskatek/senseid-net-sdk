namespace Kliskatek.SenseId.Sdk.Parsers.Common
{
    public class SenseIdData
    {
        public string? Magnitude { get; set; }
        public string? UnitLong { get; set; }
        public string? UnitShort { get; set; }
        public float Value { get; set; }
    }

    public class SenseIdTag
    {
        public Technologies? Technology { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public SenseIdData[]? Data { get; set; }
    }

    public class SenseIdTagType
    {
        public string name { get; set; }
        public string description { get; set; }
        public SenseIdDataDefinitions[] data_def { get; set; }
    }

    public class SenseIdDataDefinitions
    {
        public string magnitude { get; set; }
        public string unit_long { get; set; }
        public string unit_short { get; set; }
        public TypeEnum type { get; set; }
        public TransformEnum transform { get; set; }
        public double[] coefficients { get; set; }
    }
}
