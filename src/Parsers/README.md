# SenseID.Parsers

![NuGet Version](https://img.shields.io/nuget/v/SenseID.Parsers)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SenseID.Parsers.svg)](https://www.nuget.org/packages/SenseID.Parsers/)

A library to parse data from SenseID tags

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
  - [Rain tags](#rain-tags)
- [License](#license)

## Installation

The library is installed from [NuGet](https://www.nuget.org/packages/SenseID.Parsers)

```
dotnet add package SenseID.Parsers
```

## Usage

SenseID tag data is parsed into objects of class _SenseIdTag_

```csharp
public class SenseIdTag
{
    public Technologies? Technology { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public SenseIdData[]? Data { get; set; }
    public byte FirmwareVersion { get; set; }
}
```


### Rain tags

EPC values read from Rain tags can be parsed into objects of class _RainSenseIdTag_, which inherits from class _SenseIdTag_. EPC can be either an hexadecimal string or a byte array. To obtain SenseID data from an EPC, three methods are available:

* Call the EpcConverter.ParseEpc() method

```csharp
using Kliskatek.SenseId.Sdk.Parsers.Rain;

var rainEpcByteArray = new byte[] { 0x00, 0x00, 0x00, 0xF1, 0xD3, 0x01, 0x01, 0x00, 0x00, 0x01, 0x23, 0x01 };
var rainEpcHexString = SharedLogic.ByteArrayToHexString(rainEpcByteArray);

var dataFromByteArrayEpc = EpcConverter.ParseEpc(rainEpcByteArray);
var dataFromHexStringEpc = EpcConverter.ParseEpc(rainEpcHexString);
```

* Pass the EPC to the RainSenseIdTag class constructor

```csharp
using Kliskatek.SenseId.Sdk.Parsers.Rain;

var rainEpcByteArray = new byte[] { 0x00, 0x00, 0x00, 0xF1, 0xD3, 0x01, 0x01, 0x00, 0x00, 0x01, 0x23, 0x01 };
var rainEpcHexString = SharedLogic.ByteArrayToHexString(rainEpcByteArray);

var dataFromByteArrayEpc = new RainSenseIdTag(rainEpcByteArray);
var dataFromHexStringEpc = new RainSenseIdTag(rainEpcHexString);
```

* Use the extension methods defined for strings and byte arrays

```csharp
using Kliskatek.SenseId.Sdk.Parsers.Rain;

var rainEpcByteArray = new byte[] { 0x00, 0x00, 0x00, 0xF1, 0xD3, 0x01, 0x01, 0x00, 0x00, 0x01, 0x23, 0x01 };
var rainEpcHexString = SharedLogic.ByteArrayToHexString(rainEpcByteArray);

var dataFromByteArrayEpc = rainEpcByteArray.ToRainSenseIdTag();
var dataFromHexStringEpc = rainEpcHexString.ToRainSenseIdTag();
```

If the parsed EPC does not belong to a SenseID tag, then the _Data_ property of the object of type RainSenseIdTag will be empty.

## License

SenseID.Parsers is distributed under the terms of the [MIT](https://spdx.org/licenses/MIT.html) license.

