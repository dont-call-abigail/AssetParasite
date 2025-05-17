// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Core;
using Newtonsoft.Json;

Console.WriteLine("Hello, World!");
string sceneFolder = @"C:\Users\jack\Downloads\RippedDisco\ExportedProject\Assets\Scenes";

// potential args: required scene assets refs vs all assets, scene folder

List<SceneAssetReferenceMap> sceneAssets = new();

foreach (var scenePath in Directory.GetFiles(sceneFolder, "*.unity"))
{
    var parser = new SceneParser(scenePath);
    sceneAssets.Add(parser.BuildAssetReferenceMap());
}

foreach (var asset in sceneAssets)
{
    var path = $"{sceneFolder}/{asset.sceneName}.assetmap";
    File.WriteAllText(path, JsonConvert.SerializeObject(asset));
}

var manifest = ManifestGenerator.GenerateAllAssetManifest(sceneAssets.ToArray());
File.WriteAllText($"{sceneFolder}/scene_dresser_manifest.assetmap", System.Text.Json.JsonSerializer.Serialize(manifest, new JsonSerializerOptions(){WriteIndented = true}));
