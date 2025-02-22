using System.Runtime.CompilerServices;
using System.Text;

var assetsPath = Console.ReadLine() ?? "Assets";
var outputPath = "Outputs";

var systemDataPath = Path.Combine(assetsPath, "system.dat");
if (!File.Exists(systemDataPath))
    return;

var systemRead = new PRead(systemDataPath);

if (systemRead.Data("def/arcs.txt") is not {} arcsTxtBytes)
    return;
var arcs = Encoding.ASCII.GetString(arcsTxtBytes).Split("\r\n");
ExtractAllFiles(systemRead, outputPath);
foreach (var file in arcs)
{
    var filePath = Path.Combine(assetsPath, file);
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Data pkg not found: {filePath}, skip");
        continue;
    }
    var read = new PRead(filePath);
    ExtractAllFiles(read, outputPath);
}

Console.WriteLine("Done");
return;


static void ExtractAllFiles(PRead read, string basePath)
{
    var ti = GetPReadTiField(read);
    List<Task> writeTasks = new(ti.Keys.Count);
    foreach (var fileName in ti.Keys)
    {
        var data = read.Data(fileName);
        Console.WriteLine($"Extracting: {fileName}");
        if (data is null) continue;
        var savePath = Path.Combine(basePath, fileName);
        var directoryPath = Path.GetDirectoryName(savePath);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(Path.GetDirectoryName(savePath)))
            Directory.CreateDirectory(directoryPath);
        if (File.Exists(savePath))
        {
            var newFileName = Path.GetFileNameWithoutExtension(savePath) + "_" + Guid.NewGuid() + Path.GetExtension(savePath);
            savePath = directoryPath == null ? newFileName : Path.Combine(directoryPath, newFileName);
        }
        writeTasks.Add(File.WriteAllBytesAsync(savePath, data));
    }

    Task.WaitAll(writeTasks);
}

[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "ti")]
static extern ref Dictionary<string, PRead.fe> GetPReadTiField(PRead instance);