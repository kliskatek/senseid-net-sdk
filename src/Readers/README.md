# SenseID.Readers

![NuGet Version](https://img.shields.io/nuget/v/SenseID.Readers)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SenseID.Readers.svg)](https://www.nuget.org/packages/SenseID.Readers/)


A library that helps acquiring data from SenseID tags using common readers.

## Table of Contents

- [Installation](#installation)
- [ISenseIdReader interface](#isenseidreader-interface)
  - [Data acquisition example](#data-acquisition-example)
- [License](#license)

## Installation

The library is installed from [NuGet](https://www.nuget.org/packages/SenseID.Readers)

```
dotnet add package SenseID.Readers
```

## ISenseIdReader interface

The library functionality is provided through the _ISenseIdReader_ interface.

```csharp
public interface ISenseIdReader
{
    bool Connect(string connectionString);  
    bool Disconnect();
    SenseIdReaderInfo GetInfo();
    float GetTxPower();
    bool SetTxPower(float txPower);
    bool[] GetAntennaConfig();
    bool SetAntennaConfig(bool[] antennaConfigArray);
    bool StartDataAcquisitionAsync(SenseIdReaderCallback callback);
    bool StopDataAcquisitionAsync();
}
```

The following table lists the classes that implement _ISenseIdReader_ 

| Technology | Manufacturer | Reader/SDK | SenseID.Readers class |
|:----------:|:------------:|:----------:|:---------------------:|
| Rain       | Impinj       | Octane     | OctaneReader          |
| Rain       | NordicID     | NurApi     | NurApiReader          |
| Rain       | Phychips     | RED4S      | Red4SReader           |

### Data acquisition example

```csharp
using Kliskatek.SenseId.Sdk.Readers.Common;
using Kliskatek.SenseId.Sdk.Readers.Rfid.Nordic;
using Kliskatek.SenseId.Sdk.Readers.Rfid.Impinj;
using Kliskatek.SenseId.Sdk.Readers.Rfid.Phychips;

namespace Kliskatek.SenseId.Sdk.Readers.Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Kliskatek SenseID.SDK.Readers demo");

            //var reader = (ISenseIdReader)new OctaneReader();
            //if (!reader.Connect("192.168.17.246"))
            //    return;
            //var reader = (ISenseIdReader)new NurApiReader();
            //if (!reader.Connect("ser://com9"))
            //    return;
            var reader = (ISenseIdReader)new Red4SReader();
            if (!reader.Connect("COM4"))
                return;

            reader.StartDataAcquisitionAsync(DelegateMethod);

            Thread.Sleep(4000);

            reader.StopDataAcquisitionAsync();

            reader.Disconnect();
        }

        public static void DelegateMethod(string epc)
        {
            Console.WriteLine($"New EPC received : {epc}");
        }
    }
}
```


## License

REDRCP is distributed under the terms of the [MIT](https://spdx.org/licenses/MIT.html) license.