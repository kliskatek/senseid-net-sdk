using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Kliskatek.SenseId.Sdk.Readers.Common
{
    [DefaultValue(Unknown)]
    public enum ReaderModels
    {
        [Display(Name = @"Unknown/default reader", ShortName = @"Unknown")]
        Unknown,
        [Display(Name = @"Nordic Sampo", ShortName = @"Sampo")]
        Sampo,
        [Display(Name = @"Nordic Stix", ShortName = @"Stix")]
        Stix,
        [Display(Name = @"Impinj Speedway", ShortName = @"Speedway")]
        Speedway
    }

    public enum ReaderConnectionStatus
    {
        Disconnected,
        Connected
    }

    public enum ReaderStatus
    {
        Idle,
        BusyInventory
    }

    public enum ReaderLibraries
    {
        Unknown,
        NurApi,
        Octane,
        RedRcp
    }
}
