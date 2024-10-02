using System.ComponentModel.DataAnnotations;

namespace Kliskatek.SenseId.Sdk.Parsers.Common
{
    public enum TypeEnum
    {
        [Display(Name = @"int16", ShortName = @"int16", Description = @"16-bit signed integer")]
        Int16,
        [Display(Name = @"uint16", ShortName = @"uint16", Description = @"16-bit unsigned integer")]
        Uint16
    }

    public enum TransformEnum
    {
        [Display(Name = @"linear", ShortName = @"linear", Description = @"linear")]
        Linear,
        [Display(Name = @"thermistor-beta", ShortName = @"thermistor-beta", Description = @"thermistor-beta")]
        ThermistorBeta
    }

    public enum Technologies
    {
        Rain,
        Ble,
        Nfc
    }

    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }
}
