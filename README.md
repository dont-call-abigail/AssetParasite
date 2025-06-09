## Unity Asset Parasite
This project empowers modders of Unity games to reference base game assets without having to distribute copyrighted material with their mods.


> [!IMPORTANT]  
> If you are using Unity 2022 or newer, consider using [UnityDataTools](https://github.com/Unity-Technologies/UnityDataTools/). It is far more robust and better maintained than this tool.

Given a selection of Unity yaml asset files, this tool will:
- Find assets (materials, textures, etc) referenced within those files
- Store the property information of the object containing the reference
- Build a path from a root GameObject to the object containing the property, via Transform hierarchy

Using this data, you can assign asset references to your objects by grabbing a reference to the same asset from an object in the base game. It is meant for projects where base game assets are not otherwise accessible using AssetBundles.

Prerequisites to make maximum use of this project:
- Ripped base game assets in YAML form using [AssetRipper](https://github.com/AssetRipper/AssetRipper)
- Including scripts (using the `-s` argument) is strongly encouraged

## Release information

AssetParasite releases have two fundamental artifacts:
- `AssetParasite.exe` is the executable for the YAML parser that produces the asset reference database. It targets .NET 8.
- `DatabaseOps.dll` is a library for interfacing with the asset reference database. It targets .NET Standard 2.0/2.1 and .NET 6.0/8.0.

## Usage
You can provide a folder OR a path to a text file containing a list of asset paths.
```
Usage: AssetParasite (asset folder|asset path list file) [options]
Options:
-o, --output-file         Path to database file for catalog. Defaults to "database.db".
-m, --mod-guid            Guid to identify this mod. If not provided, assets are presumed to belong to the base game.
-f, --fresh               Clears the database entries for a given mod. Database path should be provided before this arg.
                             If a mod guid is not provided, all assets will be cleared.
-a, --find-all            Find all occurances of an asset, instead of the first.
-p, --search-pattern      Used to determine which asset filetypes are included. The default is *. The * and ? characters are supported. Regular expressions are not supported.
-e, --exclude-assets      Comma-seperated asset GUIDs to ignore.
-s, --script-path         Path to a folder containing MonoScripts and associated .meta files.
-u, --scenes              Only process scene files. Intended for ingesting references from base game.
-v, --verbose             Extra logging information for debugging.
```

## Dependencies
- [VYaml](https://github.com/hadashiA/VYaml), for superfast YAML parsing
- [UnityDataTools](https://github.com/Unity-Technologies/UnityDataTools/), as a reference
- [Microsoft.Data.SQlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/?tabs=net-cli), as our SQLite engine