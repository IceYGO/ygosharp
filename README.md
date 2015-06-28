# YGOSharp

A C# implementation of an ygopro duel server, using the ocgcore library.

### How to use:

* Compile the native OCGCore library using CMake and a C++11 compiler.

* Compile YGOSharp.sln using Visual Studio or Mono.

* Put _cards.cdb_, _lflist.conf_, _ocgcore.dll_ and the _script_ directory next to the compiled YGOSharp.exe.

* Run the executable using valid parameters, e.g. _000 12345_ to host an unranked single duel on the port 12345.

* Enjoy.
