## Unity Asset Parasite
This project empowers modders of Unity games to reference base game assets without having to distribute copyrighted material with their mods.


> [!IMPORTANT]  
> If you are using Unity 2022 or newer, strongly consider using [UnityDataTools](https://github.com/Unity-Technologies/UnityDataTools/). It is far more robust and better maintained than this tool.

Given a selection of Unity yaml asset files, this tool will:
- Find assets (materials, textures, etc) referenced within those files
- Store the property information of the object containing the reference
- Build a path from a root GameObject to the object containing the property, via Transform hierarchy

Using this data, you can assign asset references to your objects by grabbing a reference to the same asset from an object in the base game. It is meant for projects where base game assets are not otherwise accessible using AssetBundles.

Prerequisites to make maximum use of this project:
- Ripped base game assets in YAML form using [AssetRipper](https://github.com/AssetRipper/AssetRipper)
- Including MonoScript files (using the `-s` argument) is strongly encouraged

## Release information

AssetParasite releases take two forms:
- An executable for the YAML parser that produces the asset reference database. Runs on Windows, Linux, using .NET 8.
- A folder containing DLLs for the AssetParasite, if you prefer a library to an executable. Targets .NET Standard 2.0 for compatibility with Unity 2020.3+.
  - Of the DLLs, `DatabaseOps.dll` can be used over `AssetParasite.dll` if you only need to fetch manifest data (i.e. at game runtime)

## Usage
You can provide a folder OR a path to a text file containing a list of asset paths.
```
Usage: AssetParasite (asset folder|asset path list file) [options]
Options:
-o, --output-file         Path to database file for catalog. Defaults to ""database.db"".
-m, --mod-guid            Guid to identify this mod. If not provided, assets are presumed to belong to the base game.
-f, --fresh               Clears the database entries for a given mod. Database path should be provided before this arg.
                              If a mod guid is not provided, all assets will be cleared.
-a, --find-all            Find all occurances of an asset, instead of the first.
-o, --only-matches        Only add asset references where a match exists in the ""basegame"" collection.
-p, --search-pattern      Used to determine which asset filetypes are included. The default is *. 
                              The * and ? characters are supported. Regular expressions are not supported.
-e, --exclude-assets      Comma-seperated asset GUIDs to ignore.
-s, --script-path         Path to a folder containing MonoScripts and associated .meta files.
-u, --scenes              Only process scene files. Intended for ingesting references from base game.
-v, --verbose             Extra logging information for debugging.
```

## AssetParasite proudly uses the following projects
- [VYaml](https://github.com/hadashiA/VYaml), for superfast YAML parsing
- [UnityDataTools](https://github.com/Unity-Technologies/UnityDataTools/), as a reference
- [Microsoft.Data.SQlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/?tabs=net-cli), as our SQLite engine
