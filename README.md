## Unity Asset Parasite
This project empowers modders of Unity games to reference base game assets without having to distribute copyrighted material with their mods.


> [!IMPORTANT]  
> If you are using Unity 2022 or newer, consider using [UnityDataTools](https://github.com/Unity-Technologies/UnityDataTools/). It is far more robust and better maintained than this tool.

It analyzes Unity YAML files by.

Prerequisites to make maximum use of this project:
- Ripped base game assets in YAML form using [AssetRipper]()
- Some mapping of MonoBehaviour script GUIDs to type names. This project does not resolve those automatically (yet).

## Release information

AssetParasite releases have two fundamental artifacts:
- `AssetParasite.exe` is the executable for the YAML parser that produces the asset reference database. It targets .NET 8.
- `DatabaseOps.dll` is a library for interfacing with the asset reference database. It targets .NET Standard 2.0/2.1 and .NET 6.0/8.0.

## Usage

```
Usage: AssetParasite (asset folder) [options]
Options:
-o, --output-file         Path to database file for catalog. Defaults to "database.db".
-f, --fresh               Clears the entire database before running. Database path should be provided before this arg.
-m, --mod-guid            Guid to identify this mod. If not provided, assets are presumed to belong to the base game.
-a, --find-all            Finds all occurances of an asset, instead of the first.
-p, --search-pattern      Used to determine which asset filetypes are included. The default is *. The * and ? characters are supported. Regular expressions are not supported.
-e, --exclude-assets      Comma-seperated asset GUIDs to ignore.
-s, --script-names        Path to a file which includes a line by line, comma seperated, mapping of asset GUIDs to 
                            MonoBehaviour type names. Example line: fe2f6beebe0c93d45b047e3a99c9e426,PlayerCamera
-v, --version             Print program version and exit.
```