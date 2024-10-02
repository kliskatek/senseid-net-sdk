
namespace Kliskatek.SenseId.Sdk.Parsers.Common
{
    public static partial class SharedLogic
    {

        private static double TransformFromDouble(double dataIn, SenseIdDataDefinitions dataDef)
        {
            switch (dataDef.transform)
            {
                case TransformEnum.Linear:
                    return LinearTransformation(dataIn, dataDef);
                case TransformEnum.ThermistorBeta:
                    return ThermistorBetaTransformation(dataIn, dataDef);
                default:
                    throw new ArgumentException("Unsupported transform " + dataDef.transform);
            }
        }

        private static double LinearTransformation(double dataIn, SenseIdDataDefinitions dataDef)
        {
            return dataIn * dataDef.coefficients[1] + dataDef.coefficients[0];
        }

        private static double ThermistorBetaTransformation(double dataIn, SenseIdDataDefinitions dataDef)
        {
            var raw = dataIn * 1000 / (4095 - dataIn);
            var beta = dataDef.coefficients[0];
            var r0 = dataDef.coefficients[1];
            var t0 = dataDef.coefficients[2] + 273.15;
            return 1 / (1 / t0 + 1 / beta * Math.Log(raw / r0)) - 273.15;
        }
    }
}