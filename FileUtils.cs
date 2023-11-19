using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Xlsx2Lua;

public class FileUtils
{
    /// <summary>
    /// 获取指定目录下的所有文件（包含子目录）
    /// </summary>
    /// <param name="path"></param>
    /// <param name="files"></param>
    public static void GetDirectoryFiles(string path,ref List<FileInfo> files)
    {
        files.AddRange(getFiles(path));
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
        {
            GetDirectoryFiles(directory.FullName,ref files);
        }
    }

    /// <summary>
    /// 获取顶部目录下的所有文件（包含子目录），并以顶部目录名作为Key记录下来
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filesDic"></param>
    public static void GetTopDirectoryFiles(string path, ref Dictionary<string, List<FileInfo>> filesDic)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
        {
            string folder = directory.Name;
            List<FileInfo> files = new List<FileInfo>();
            GetDirectoryFiles(Path.Combine(path, folder), ref files);
            filesDic.Add(folder,files);
        }
    }

    private static FileInfo[] getFiles(string path,string folder = "")
    {
        if (folder != "")
            path = Path.Combine(path, folder);
        
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        var fileInfos = directoryInfo.GetFiles().OrderBy(file => file.Name);
        return fileInfos.ToArray();
    }

    private static void SafeCreateDirectory(string? path)
    {
        if (path == null)
            return;
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    
    #region 文件读写

    public static bool SaveJson(string fileName,object data)
    {
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        });
        return Save(fileName, json);
    }
    
    public static bool Save(string fileName, string content)
    {
        return Save(fileName, content, new UTF8Encoding(false));
    }

    public static bool Save(string fileName, StringBuilder content)
    {
        return Save(fileName, content, new UTF8Encoding(false));
    }

    private static bool Save(string fileName, object content, Encoding encoding)
    {
        try
        {
            SafeCreateDirectory(Path.GetDirectoryName(fileName));

            using (StreamWriter writer = new StreamWriter(fileName,false,encoding))
            {
                writer.Write(content);
                writer.Flush();
                writer.Close();
            }
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return false;
    }

    #endregion
}