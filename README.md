# CraftingPasteModule

Mount & Blade II: Bannerlord mod that allows you to copy/paste designs in the crafting screen, similar to FaceGen and Banner editor screens.

# Installation

Git clone, build (see below), run the game launcher and enable the module.

# Usage

Enter smithy -> Forge, Ctrl-C will copy your current design to clipboard. A message "design copied to clipboard" should appear.

Paste it later using Ctrl-V, a message "design pasted from clipboard" should appear.

Don't forget to select matching weapon type before pasting.

# Development

Works fine with VS2019 Community Edition.

VSCode + .NET Core SDK + `dotnet build` @ powershell should be fine too.

Copy `env.example.xml` to `env.xml` and edit the settings according to your environment. Watch out for the ampersand in XML files.

The `PostBuild.ps1` script will auto execute on successful builds, and assemble the final distributable folder of the module inside the `.\dist` directory as well as install it to the game dir.

To build using CLI:
```ps1
PS C:\path-to-src\> dotnet build -c Debug # or Release
```

# Credits

Project structure inspired by https://github.com/haggen/bannerlord-module-template & https://github.com/Tyler-IN/MnB2-Bannerlord-CommunityPatch.

# Legal

© 2020 alejandro-y

This modification is not created by, affiliated with or sponsored by TaleWorlds Entertainment or its affiliates. The Mount & Blade II Bannerlord API and related logos are intelectual property of TaleWorlds Entertainment. All rights reserved.
