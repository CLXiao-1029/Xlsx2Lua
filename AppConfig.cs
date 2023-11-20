using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using OfficeOpenXml;

namespace Xlsx2Lua;

/// <summary>
/// 翻译工具的配置结构
/// </summary>
public struct LanguageData
{
    public string AppId { get; set; }
    public string SecretKey { get; set; }
    public string From { get; set; }
    public string[] Translations { get; set; }
    public string[] TranslationNames { get; set; }

    public LanguageData()
    {
        AppId = "";
        SecretKey = "";
        From = "zh";
        Translations = new string[] { "en", "jp" };
        TranslationNames = new string[] { "英语", "日语" };
    }
}

internal struct AppData
{
    public string? ConfigPath { get; set; }
    public string? OutputPath { get; set; }
    public string? OutputExt { get; set; }
    public long StartingRow { get; set; }
    public bool OutputArray { get; set; }
    public bool SummaryConfig { get; set; }
    public bool CommentFile { get; set; }
    public bool ShowTimelapse { get; set; }
    public bool Translation { get; set; }
    public bool RealtimeTrans { get; set; }
    public long ShowLogLevel { get; set; }
    public bool OutputLogFile { get; set; }
    public string[] Translations { get; set; }
    public string[] TranslationNames { get; set; }

    public AppData()
    {
        Default();
    }

    private void Default()
    {
        ConfigPath = "";
        OutputPath = "";
        OutputExt = "lua";
        StartingRow = 6;
        OutputArray = true;
        SummaryConfig = false;
        CommentFile = true;
        ShowTimelapse = true;
        Translation = true;
        RealtimeTrans = false;
        ShowLogLevel = 1;
        OutputLogFile = false;
        Translations = new string[] { "zh","en", "jp" };
        TranslationNames = new string[] { "中文","英语", "日语" };
    }

    private void ToAppData(Dictionary<string, object> data)
    {
        if (data.TryGetValue("ConfigPath", out object? configPath))
            ConfigPath = configPath.ToString();

        if (data.TryGetValue("OutputPath", out object? outputPath))
            OutputPath = outputPath.ToString();

        if (data.TryGetValue("OutputExt", out object? outputExt))
            OutputExt = (string)outputExt;

        if (data.TryGetValue("StartingRow", out object? startingRow))
            StartingRow = (long)startingRow;
        if (data.TryGetValue("OutputArray", out object? outputArray))
            OutputArray = (bool)outputArray;
        if (data.TryGetValue("SummaryConfig", out object? summaryConfig))
            SummaryConfig = (bool)summaryConfig;

        if (data.TryGetValue("CommentFile", out object? commentFile))
            CommentFile = (bool)commentFile;
        if (data.TryGetValue("ShowTimelapse", out object? showTimelapse))
            ShowTimelapse = (bool)showTimelapse;
        if (data.TryGetValue("Translation", out object? translation))
            Translation = (bool)translation;

        if (data.TryGetValue("RealtimeTrans", out object? realtimeTrans))
            RealtimeTrans = (bool)realtimeTrans;
        if (data.TryGetValue("ShowLogLevel", out object? showLogLevel))
            ShowLogLevel = (long)showLogLevel;
        if (data.TryGetValue("OutputLogFile", out object? outputLogFile))
            OutputLogFile = (bool)outputLogFile;
    }
}

/// <summary>
/// 需要翻译的多语言数据结构
/// </summary>
public struct MultilingualData
{
    public string Key { get; set; }
    public Dictionary<string, object> Values { get; set; }

    public void AddValue(string key, object obj)
    {
        if (!Values.ContainsKey(key))
            Values.Add(key, obj);
        else
            ReplaceValue(key, obj);
    }

    public object GetValue(string key)
    {
        if (Values.TryGetValue(key,out var data))
        {
            return data;
        }
        return null;
    }
    
    public void ReplaceValue(string key, object obj)
    {
        Values[key] = obj;
    }
    
    public bool Compare(string key, object target)
    {
        if (Values.TryGetValue(key,out var data))
        {
            return data == target;
        }
        return false;
    }
}

/// <summary>
/// 一张表的数据结构
/// </summary>
internal struct TableDataInfo
{
    /// <summary>
    /// 行数
    /// </summary>
    public int Rows;

    /// <summary>
    /// 列数
    /// </summary>
    public int Columns;

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName;

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName;

    /// <summary>
    /// 所属文件夹
    /// </summary>
    public string FolderName;

    /// <summary>
    /// 表数据
    /// </summary>
    public ExcelRange Cells;
}

internal class AppConfig
{
    // 用于缩进lua table的字符串
    private static readonly string LUA_TABLE_INDENTATION_STRING = "\t";
    private static readonly string ConfigName = "Xlsx2Lua.cfg";
    private static readonly string LanguageName = "language.cfg";
    public static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// 多语言文件名
    /// </summary>
    public static readonly string MultilingualName = "TranslateMain.xlsx";

    public static AppData AppData = new AppData();

    /// <summary>
    /// 缩进值
    /// </summary>
    public static int IndentLevel = 1;
    
    /// <summary>
    /// 提交记录
    /// </summary>
    public static string CommitRecord = "";

    public static List<string> varInfos = new List<string>();
    public static List<string> varNames = new List<string>();
    public static List<string> varTypes = new List<string>();
    public static StringBuilder varTypeInfos = new StringBuilder();
    
    /// <summary>
    /// 配置表数据缓存
    /// </summary>
    public static Dictionary<string, TableDataInfo> DataTables = new Dictionary<string, TableDataInfo>();

    /// <summary>
    /// 读取配置数据
    /// </summary>
    public static void ReadConfigData()
    {
        string fileName = Path.Combine(AppDirectory, ConfigName);
        if (!File.Exists(fileName))
        {
            FileUtils.SaveJson(fileName, AppData);
        }
        string var = File.ReadAllText(fileName);
        AppData = JsonSerializer.Deserialize<AppData>(var);
    }

    public static void SaveAppData()
    {
        string cfgPath = Path.Combine(AppDirectory, ConfigName);
        Logger.LogWarning($"保存配置文件：{cfgPath}");
        if (File.Exists(cfgPath))
        {
            File.Delete(cfgPath);
        }
        
        FileUtils.SaveJson(cfgPath, AppData);
    }

    public static void SaveLogFile()
    {
        if (AppData.OutputLogFile)
        {
            string logPath = Path.Combine(AppDirectory,"log", $"{Assembly.GetExecutingAssembly().GetName().Name}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.log");
            FileUtils.Save(logPath, Logger.LogToString());
            Logger.LogWarning(logPath);
        }
    }

    public static void ResetTableItem()
    {
        IndentLevel = 1;
        varInfos.Clear();
        varNames.Clear();
        varTypes.Clear();
    }
    
    public static bool VarTypeIsNull(int index)
    {
        return string.IsNullOrEmpty(varTypes[index]) || string.IsNullOrWhiteSpace(varTypes[index]);
    }
    
    public static string IndentIndex()
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < IndentLevel; ++i)
            stringBuilder.Append(LUA_TABLE_INDENTATION_STRING);

        return stringBuilder.ToString();
    }

    public static void AddAnnotation(ExcelRange cells, int columns)
    {
        for (int j = 1; j <= columns; ++j)
        {
            var info = cells[1, j].Value;
            var name = cells[2, j].Value;
            var type = cells[3, j].Value;
            if (name != null && type != null)
            {
                varInfos.Add(info.ToString());
                varNames.Add(name.ToString());
                varTypes.Add(type.ToString());
            }
            else
            {
                varInfos.Add(null);
                varNames.Add(null);
                varTypes.Add(null);
            }
        }
    }

    public static string GetAnnotation(string name)
    {
        StringBuilder content = new StringBuilder();
        GenAnnotation(name, content);
        return content.ToString();
    }

    public static void GenAnnotation(string name, StringBuilder content)
    {
        content.AppendLine($"---@class Cfg_{name}");
        for (int i = 0; i < varTypes.Count; i++)
        {
            var type = varTypes[i];
            if (VarTypeIsNull(i))
            {
                continue;
            }

            type = ExportLuaHelper.AnalyzeType(type);
            if (varTypeInfos.Length != 0)
            {
                content.AppendLine($"---@field {varNames[i]} {type} {varInfos[i]} {varTypeInfos}");
            }
            else
            {
                content.AppendLine($"---@field {varNames[i]} {type} {varInfos[i]}");
            }
        }

        content.AppendLine();
    }
    
    /// <summary>
    /// 获取Config的提交记录
    /// </summary>
    public static void RefreshCommitRecord()
    {
        CommitRecord = TortoiseGitHelper.GetLatestCommitRecord(AppData.ConfigPath);
    }
}