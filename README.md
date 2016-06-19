# YGOSharp

A C# implementation of an ygopro duel server, using the ocgcore library.

### How to use:

* Compile the native OCGCore library using CMake and a C++11 compiler.

* Compile YGOSharp.sln using Visual Studio or Mono.

* Put _cards.cdb_, _lflist.conf_, _ocgcore.dll_ and the _script_ directory next to the compiled YGOSharp.exe.

* Run the executable with or without parameters. The default configuration will host a single duel on the port 7911.

* Enjoy.

## Configuration options

### Server

* `ConfigFile` (default: none)

* `ClientVersion` (default: `0x1339`)

* `Port` (default: `7911`)

### Files

* `BanlistFile` (default: `lflist.conf`)

* `RootPath` (default: `.`)

* `ScriptDirectory` (default: `script`)

* `DatabaseFile` (default: `cards.cdb`)

### Game

* `Rule` (default: `0`)

* `Mode` (default: `0`)

* `Banlist` (default: `0`)

* `StartLp` (default: `8000`)

* `StartHand` (default: `5`)

* `DrawCount` (default: `1`)

* `GameTimer` (default: `240`)

### Deck

* `NoCheckDeck` (default: `false`)

* `NoShuffleDeck` (default: `false`)

* `MainDeckMinSize` (default: `40`)

* `MainDeckMaxSize` (default: `60`)

* `ExtraDeckMaxSize` (default: `15`)

* `SideDeckMaxSize` (default: `15`)

### Legacy

* `EnablePriority` (default: `false`)

### Addons

* `StandardStreamProtocol`  (default: `false`)

