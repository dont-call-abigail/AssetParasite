using VYaml.Parser;

namespace AssetParasite;

public class FileParser
{
    public FileParser(string path)
    {
        var bytes = File.ReadAllBytesAsync(path).Result;
        var parser = YamlParser.FromBytes(bytes);
    }

    public void Run(byte[] yamlBytes)
    {
        var parser = YamlParser.FromBytes(yamlBytes);
        
        parser.SkipHeader();
    }
}