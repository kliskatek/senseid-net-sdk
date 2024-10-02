using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Kliskatek.SenseId.Sdk.Parsers.Common;
using System.Globalization;

namespace Kliskatek.SenseId.Sdk.Parsers.Rain
{
    public class YamlDefinitionParser
    {
        private static readonly Lazy<YamlDefinitionParser> _lazyInstance =
            new Lazy<YamlDefinitionParser>(() => new YamlDefinitionParser());

        public static YamlDefinitionParser Instance => _lazyInstance.Value;

        protected SenseIdDefinitions? _definitions;

        public SenseIdDefinitions GetDefinitions()
        {
            return _definitions;
        }

        private YamlDefinitionParser()
        {
            _definitions = ParseSenseIdDefinitions(Common.Constants.RainSenseIdYaml);
        }

        protected SenseIdDefinitions ParseSenseIdDefinitions(string yamlFileName,
            string alternateSearchPath = Common.Constants.DefinitionFolderRelativePath)
        {
            string yamlPath = yamlFileName;
            if (!File.Exists(yamlPath))
                yamlPath = alternateSearchPath + yamlFileName;
            // RawSenseIdDataDefinitions rawData;
            string yamlText = File.ReadAllText(yamlPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var rawData = deserializer.Deserialize<RawSenseIdDefinitions>(yamlText);
            return RawDataToDefinitionFormat(rawData);
        }

        private SenseIdDefinitions RawDataToDefinitionFormat(RawSenseIdDefinitions rawData)
        {
            var tmpDefinitions = new SenseIdDefinitions();

            tmpDefinitions.version = rawData.version;
            // Parse time format
            var cultureInfo = new CultureInfo("en-US");
            tmpDefinitions.date = DateTime.Parse(rawData.date, cultureInfo);
            // Convert PEN byte list to hexadecimal string
            tmpDefinitions.pen_header = new byte[rawData.pen_header.Length];
            foreach (var item in rawData.pen_header.Select((value, i) => new { i, value }))
            {
                var intByte = item.value;
                var index = item.i;
                tmpDefinitions.pen_header[index] = Convert.ToByte(intByte);
            }
            tmpDefinitions.types = new Dictionary<byte[], SenseIdTagType>();
            foreach (var inputTypeData in rawData.types)
            {
                var originalKey = inputTypeData.Key;
                var originalValue = inputTypeData.Value;

                if (!SharedLogic.IsHexString(originalKey))
                    continue;
                var newKey = Convert.FromHexString(SharedLogic.RemoveHeadingText(originalKey, "0x").ToUpper());

                var newValue = new SenseIdTagType
                {
                    name = originalValue.name,
                    description = originalValue.description
                };

                var dataDefList = new List<SenseIdDataDefinitions>();
                foreach (var item in originalValue.data_def)
                {
                    var newDataDef = new SenseIdDataDefinitions
                    {
                        magnitude = item.magnitude,
                        unit_long = item.unit_long,
                        unit_short = item.unit_short,
                        coefficients = item.coefficients
                    };
                    // Try parse 'type' property
                    if (!SharedLogic.EnumShortNameMatch(item.type, out TypeEnum tmpTypeEnum))
                        continue;
                    newDataDef.type = tmpTypeEnum;
                    // Try parse 'Transform' property
                    if (!SharedLogic.EnumShortNameMatch(item.transform, out TransformEnum tmpTransformEnum))
                        continue;
                    newDataDef.transform = tmpTransformEnum;

                    dataDefList.Add(newDataDef);
                }

                newValue.data_def = dataDefList.ToArray();
                tmpDefinitions.types[newKey] = newValue;
            }

            return tmpDefinitions;
        }


    }
}
