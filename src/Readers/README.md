# SenseID.Readers

![NuGet Version](https://img.shields.io/nuget/v/SenseID.Readers)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SenseID.Readers.svg)](https://www.nuget.org/packages/SenseID.Readers/)


A library that helps acquiring data from SenseID tags using common readers.

## Table of Contents

- [Installation](#installation)
- [ISenseIdReader interface](#isenseidreader-interface)
  - [Data acquisition example](#data-acquisition-example)
- [Reader scanner](#reader-scanner)
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
    bool[] GetEnabledAntennas();
    bool SetEnabledAntennas(bool[] enabledAntennaArray);
    bool StartDataAcquisitionAsync(SenseIdReaderCallback callback);
    bool StopDataAcquisitionAsync();
}
```

The following table lists the reader classes that implement _ISenseIdReader_ 

| Technology | Manufacturer | Reader API/protocol/library | SenseID.Readers class    |
|:----------:|:------------:|:---------------------------:|:------------------------:|
| Rain       | Impinj       | Octane                      | SenseIdOctaneReader      |
| Rain       | NordicID     | NurApi                      | SenseIdNurApiReader      |
| Rain       | Phychips     | REDRCP                      | SenseIdRedRcpRed4SReader |

### Data acquisition example

```csharp
using Kliskatek.SenseId.Sdk.Readers.Common;
using Kliskatek.SenseId.Sdk.Readers.Rfid;

namespace Kliskatek.SenseId.Sdk.Readers.Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Kliskatek SenseID.SDK.Readers demo");

            //var reader = (ISenseIdReader)new SenseIdOctaneReader();
            //if (!reader.Connect("192.168.17.246"))
            //    return;
            //var reader = (ISenseIdReader)new SenseIdNurApiReader();
            //if (!reader.Connect("ser://com9"))
            //    return;
            var reader = (ISenseIdReader)new SenseIdRedRcpReader();
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

## Reader scanner

The library contains a scanner class that helps find supported readers connected to the computer or in the same LAN.

The following code shows how to instantiate the scanner and start and stop the scanning process.

```csharp
using Kliskatek.SenseId.Sdk.Readers.Scanner;

var scanner = new ReaderScanner();
scanner.StartScan();
// user defined delay
scanner.StopScan();
```

The scanner invokes the _NewReaderFound_ event to notify that a new reader has been found.

```csharp
scanner.NewReaderFound += OnNewReaderFound;

private static void OnNewReaderFound(object sender, FoundReaderEventArgs e)
{
    Console.WriteLine($"New reader of type {e.ReaderType} found with connection string {e.ConnectionString}");
}
```

It is possible to get a list containing all found readers at any moment by calling the _GetFoundReaders_ method.

```csharp
var foundReaders = scanner.GetFoundReaders();
foreach (var foundReader in foundReaders)
    Console.WriteLine($" * Reader type {foundReader.ReaderType} found with connection string {foundReader.ConnectionString}");
```

## License

REDRCP is distributed under the terms of the [MIT](https://spdx.org/licenses/MIT.html) license.