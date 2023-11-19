using System.Text;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Xlsx2Lua;

internal class ExportLuaHelper
{
    struct MultilingualData
    {
        /// <summary>
        /// 翻译主键
        /// </summary>
        public object Key;
        /// <summary>
        /// 语种类型和文本
        /// </summary>
        public Dictionary<object, object> Values;
    }
    private static readonly Dictionary<string, StringBuilder> Content = new Dictionary<string, StringBuilder>();
    private static readonly Dictionary<string, string> Comment = new Dictionary<string, string>();
    private static readonly Dictionary<object, List<object>> MultilingualList = new Dictionary<object, List<object>>();

    private static readonly Dictionary<object, MultilingualData> MultilingualDataDic =
        new Dictionary<object, MultilingualData>();
    private static readonly Dictionary<object, List<object>> MultilingualListTmp = new Dictionary<object, List<object>>();
    private static readonly List<object> MultilingualKeys = new List<object>();
    private static readonly List<object> MultilingualNames = new List<object>();

    private static void AppendLineContentIndent(StringBuilder content, object data)
    {
        content.AppendLine(AppConfig.IndentIndex() + data);
    }

    private static void AppendLineContent(StringBuilder content, object data)
    {
        content.AppendLine($"{data}");
    }

    private static void AppendContentIndent(StringBuilder content, object data)
    {
        content.Append(AppConfig.IndentIndex() + data);
    }

    public static void AppendContent(StringBuilder content, object data)
    {
        content.Append($"{data}");
    }

    private static bool IsNumber(string type)
    {
        type = type.ToLower();
        if (type.Equals("int")
            || type.Equals("long")
            || type.Equals("float")
            || type.Equals("double"))
            return true;
        return false;
    }

    private static bool IsString(string type)
    {
        return type.ToLower().Equals("string");
    }

    private static bool IsTable(string type)
    {
        return type.ToLower().Equals("table");
    }

    private static bool IsBoolean(string type)
    {
        type = type.ToLower();
        return type.Equals("bool") || type.Equals("boolean");
    }

    private static bool IsNumberArray(string type)
    {
        type = type.ToLower();
        if (type.Equals("int[]")
            || type.Equals("long[]")
            || type.Equals("float[]")
            || type.Equals("double[]")
            || type.Equals("array[int]")
            || type.Equals("array[long]")
            || type.Equals("array[float]")
            || type.Equals("array[double]"))
            return true;
        return false;
    }

    private static bool IsStringArray(string type)
    {
        type = type.ToLower();
        return type.Equals("string[]") || type.Equals("array[string]");
    }

    private static bool IsTableArray(string type)
    {
        type = type.ToLower();
        return type.Equals("table[]") || type.Equals("array[table]");
    }

    private static bool IsBooleanArray(string type)
    {
        type = type.ToLower();
        return type.Equals("bool[]") || type.Equals("boolean[]") || type.Equals("array[bool]") || type.Equals("array[boolean]");
    }

    private static bool IsArrayTable(string type)
    {
        type = type.ToLower();
        string pattern = @"(?i)(?<=\[)(.*)(?=\])";
        Match match = Regex.Match(type, pattern);
        
        return match.Success;
    }

    private static bool DataToBool(string data, out string error)
    {
        error = null;
        bool boolean = false;
        if (data.Equals("1") || data.Equals("true"))
            boolean = true;
        else if (data.Equals("0") || data.Equals("false"))
            boolean = false;
        else
            error = $"非法的Bool类型的值 : {data}";
        return boolean;
    }

    /// <summary>
    /// 类型转换默认值
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    private static object ParseDefaultValue(string type, string data, out string error)
    {
        type = type.ToLower();

        object result = null;

        error = null;

        switch (type)
        {
            case "int":
                int intVal;
                if (!int.TryParse(data, out intVal))
                {
                    error = $"非法的Int类型的值 {data}";
                    return result;
                }

                result = intVal;
                break;
            case "float":
                float floatVal;
                if (!float.TryParse(data, out floatVal))
                {
                    error = $"非法的Float类型的值 {data}";
                    return result;
                }

                result = floatVal;
                break;
            case "long":
                long longVal;
                if (!long.TryParse(data, out longVal))
                {
                    error = $"非法的Long类型的值 {data}";
                    return result;
                }

                result = longVal;
                break;
            case "double":
                double doubleVal;
                if (!double.TryParse(data,out doubleVal))
                {
                    error = $"非法的Double类型的值 {data}";
                    return result;
                }

                result = doubleVal;
                break;
            case "bool":
            case "boolean":
                bool booleanVal = DataToBool(data, out error);
                if (error != null)
                    return result;
                result = booleanVal.ToString().ToLower(); 
                    break;
            case "string":
                result = string.IsNullOrEmpty(data) ? null : $"\"{data}\"";
                break;
            case "table":
                result = string.IsNullOrEmpty(data) ? null : data;
                break;
        }

        return result;
    }


    /// <summary>
    /// 分析哈希表
    /// </summary>
    /// <param name="data"></param>
    /// <param name="type"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    private static List<Dictionary<string, object>>? AnalyzeTable(string data, string type, out string error)
    {
        error = null;
        try
        {
            string pattern = @"(?i)(?<=\[)(.*)(?=\])";
            Match match = Regex.Match(type, pattern);
            string[] kvTypes = match.Value.Split(',');
            string[] valStrLst = data.Split('|');
            List<Dictionary<string, object>> children = new List<Dictionary<string, object>>();
            foreach (string values in valStrLst)
            {
                error = null;
                if (string.IsNullOrEmpty(values))
                    continue;
                
                Dictionary<string, object> node = new Dictionary<string, object>();
                string[] valArr = values.Split(",");
                //检测有效值长度
                int count = valArr.Length > kvTypes.Length ? kvTypes.Length : valArr.Length;
                for (int i = 0; i < count; i++)
                {
                    string[] keyValuePair = kvTypes[i].Split(":");
                    string key = keyValuePair[0];
                    string valType = keyValuePair[1];
                    string value = valArr[i];

                    if (string.IsNullOrEmpty(error))
                    {
                        node.Add(key,ParseDefaultValue(valType,value,out error));
                    }
                    else
                    {
                        break;
                    }
                }

                if (string.IsNullOrEmpty(error))
                    children.Add(node);
                else
                {
                    error += string.Format("子表达式 : {0}\n", values);
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(error))
            {
                return children;
            }

            return null;

        }
        catch (Exception e)
        {
            error += e;
            return null;
        }
    }
    
    /// <summary>
    /// 分析数组
    /// </summary>
    /// <param name="data"></param>
    /// <param name="type"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    private static List<object>? AnalyzeArray(string data, string type, out string error)
    {
        error = null;
        try
        {
            var results = type.Split('[');
            if (results[results.Length-1].Equals("]"))
            {
                // var str = results[results.Length - 1];
                // type = str.Replace("]", "");
                type = results[0];
            }
            else
            {
                Match matchArray = Regex.Match(type, @"(?i)(?<=\[)(.*)(?=\])");
                type = matchArray.Value;
            }
            
            List<object> children = new List<object>();
            string[] valStrLst = data.Split('|');
            for (int i = 0; i < valStrLst.Length; ++i)
            {
                string value = valStrLst[i];
                
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                    continue;
                if (string.IsNullOrEmpty(error))
                    children.Add(ParseDefaultValue(type, value, out error));
                else
                    error += string.Format("子表达式 : {0}\n", value);
            }

            if (string.IsNullOrEmpty(error))
            {
                return children;
            }

            return null;
        }
        catch (Exception e)
        {
            error += e;
            return null;
        }
    }
    
    /// <summary>
    /// 分析类型转换
    /// </summary>
    /// <param name="type"></param>
    /// <param name="isAnnotation"></param>
    /// <returns></returns>
    public static string AnalyzeType(string type, bool isAnnotation = true)
    {
        if (isAnnotation)
        {
            AppConfig.varTypeInfos.Clear();
        }
        
        if (string.IsNullOrEmpty(type))
            return "any";
        
        string typeString = type.Trim().ToLower();

        if (IsNumber(typeString))
        {
            return "number";
        }

        if (IsString(typeString) || IsTable(typeString))
        {
            return typeString;
        }

        if (IsBoolean(typeString))
        {
            return "boolean";
        }

        if (IsNumberArray(typeString))
        {
            return "number[]";
        }

        if (IsStringArray(typeString))
        {
            return "string[]";
        }

        if (IsTableArray(typeString))
        {
            return "table[]";
        }

        if (IsBooleanArray(typeString))
        {
            return "boolean[]";
        }

        if (typeString.StartsWith("arraytable"))
        {
            StringBuilder arrayTableStr = new StringBuilder();
            arrayTableStr.Append("table<");
            if (isAnnotation)
                AppConfig.varTypeInfos.Append('{');

            var formatMatch = Regex.Matches(typeString, @"(?i)(?<=\[)(.*)(?=\])");
            string matchVal = formatMatch[0].Value;
            var objs = matchVal.Split(new[] { ':', ',' });
            for (int i = 0; i < objs.Length; i++)
            {
                int idx = i + 1;
                if (idx % 2 == 0)
                {
                    arrayTableStr.Append(AnalyzeType(objs[i], false));
                    arrayTableStr.Append(',');
                }
                else
                {
                    if (isAnnotation)
                    {
                        AppConfig.varTypeInfos.Append(objs[i]);
                        AppConfig.varTypeInfos.Append(',');
                    }
                }
            }
            arrayTableStr.Remove(arrayTableStr.Length - 1, 1);
            arrayTableStr.Append('>');
            if (isAnnotation)
            {
                AppConfig.varTypeInfos.Remove(AppConfig.varTypeInfos.Length - 1, 1);
                AppConfig.varTypeInfos.Append('}');
            }
            return arrayTableStr.ToString();
        }
        
        if (typeString.StartsWith("[") && typeString.EndsWith("]"))
        {
            Match matchArray = Regex.Match(typeString, @"(?i)(?<=\[)(.*)(?=\])");
            if (matchArray.Success)
            {
                var objs = matchArray.Value.Split(new[] { ':', ',' });
                if (objs.Length == 0)
                {
                    string val = AnalyzeType(matchArray.Value, false);
                    return $"{val}[]";
                }
            }
            string pattern = @"([a-z]+):([a-z]+)";
            MatchCollection matchCollection = Regex.Matches(typeString, pattern);
            StringBuilder tb = new StringBuilder();
            tb.Append("table<");
            if (isAnnotation)
            {
                AppConfig.varTypeInfos.Append('{');
            }
            foreach (Match match in matchCollection)
            {
                string key = match.Groups[1].Value;
                string valType = match.Groups[2].Value;
                tb.Append(AnalyzeType(valType, false));
                tb.Append(',');
                if (isAnnotation)
                {
                    AppConfig.varTypeInfos.Append(key);
                    AppConfig.varTypeInfos.Append(',');
                }
            }

            tb.Remove(tb.Length - 1,1);
            tb.Append('>');
            if (isAnnotation)
            {
                AppConfig.varTypeInfos.Remove(AppConfig.varTypeInfos.Length - 1,1);
                AppConfig.varTypeInfos.Append('}');
            }

            return tb.ToString();
        }

        return "any";
    }
    
    /// <summary>
    /// 转换lua数据
    /// </summary>
    /// <param name="name"></param>
    /// <param name="content"></param>
    /// <param name="key"></param>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <param name="isNull"></param>
    /// <param name="row"></param>
    /// <param name="column"></param>
    private static void GenLuaTableData(string name, StringBuilder content, string key, string type, object data,
        bool isNull, int row = default, int column = default)
    {
        ++AppConfig.IndentLevel;
        if (IsNumber(type))
        {
            if (!isNull)
            {
                var s = data.ToString();
                if (int.TryParse(s, out var result) || 
                    long.TryParse(s,out var result1) || 
                    float.TryParse(s,out var result2) || 
                    double.TryParse(s,out var result3))
                {
                    isNull = false;
                }
                else
                {
                    isNull = true;
                }

                if (!isNull)
                {
                    AppendLineContentIndent(content,$"{key} = {data},");
                }
            }
        }else if (IsString(type))
        {
            if (!isNull)
                AppendLineContentIndent(content,$"{key} = \"{data}\",");
        }else if (IsBoolean(type))
        {
            if (!isNull)
            {
                string boolData = data.ToString().ToLower();
                string error;
                bool boolean = DataToBool(boolData, out error);
                if (error != null)
                    Logger.LogError($"表名：{name} ,第{row}行{column}列，key = {key}，{error}");
                
                AppendLineContentIndent(content,$"{key} = {boolean.ToString().ToLower()},");
            }
        }else if (IsTable(type))
        {
            if (!isNull)
                AppendLineContentIndent(content,$"{key} = {data},");
        }else if (IsNumberArray(type)
                  || IsStringArray(type)
                  || IsTableArray(type)
                  || IsBooleanArray(type))
        {
            if (!isNull)
            {
                AppendLineContentIndent(content ,$"{key} = {{");
                ++AppConfig.IndentLevel;
                string error;
                var dataObjs = AnalyzeArray(data.ToString(), type, out error);

                if (error != null)
                    Logger.LogError($"表名：{name} ,第{row}行{column}列，{error}");

                if (dataObjs != null)
                {
                    for (int i = 0; i < dataObjs.Count; i++)
                    {
                        AppendLineContentIndent(content,$"{dataObjs[i]},");
                    }
                    content.Remove(content.Length - 1, 1);
                }
                --AppConfig.IndentLevel;
                AppendLineContentIndent(content, "},");
            }
        }else if (IsArrayTable(type))
        {
            if (!isNull)
            {
                AppendLineContentIndent(content, $"{key} ={{");
                ++AppConfig.IndentLevel;
                string error;
                var dataObjs = AnalyzeTable(data.ToString(), type, out error);
                
                if (error != null)
                    Logger.LogError($"表名：{name} ,第{row}行{column}列，{error}");

                if (dataObjs != null)
                {
                    for (int i = 0; i < dataObjs.Count; i++)
                    {
                        var nodes = dataObjs[i];
                        AppendLineContentIndent(content, "{");
                        ++AppConfig.IndentLevel;
                        foreach (var node in nodes)
                        {
                            AppendLineContentIndent(content, $"{node.Key} = {node.Value},");
                        }
                        --AppConfig.IndentLevel;
                        content.Remove(content.Length - 1, 1);
                        AppendLineContentIndent(content, "},");
                    }
                }
                
                --AppConfig.IndentLevel;
                AppendLineContentIndent(content, "},");
            }
        }
        else if (type.Equals("any"))
        {
            if (!isNull)
                AppendLineContentIndent(content, $"{key} = {data},");
        }
        --AppConfig.IndentLevel;
    }
    
    /// <summary>
    /// 添加注释
    /// </summary>
    /// <param name="name">表名</param>
    /// <param name="content">注释内容</param>
    /// <returns></returns>
    public static bool AddComment(string name, string content)
    {
        return Comment.TryAdd(name, content);
    }

    /// <summary>
    /// 注释转文本
    /// </summary>
    /// <returns></returns>
    public static StringBuilder CommentToString()
    {
        StringBuilder comment = new StringBuilder();
        foreach (KeyValuePair<string,string> keyValuePair in Comment)
        {
            comment.Append(keyValuePair.Value);
        }

        return comment;
    }

    /// <summary>
    /// 添加Lua内容
    /// </summary>
    /// <param name="name">表名</param>
    /// <param name="content">内容</param>
    /// <returns></returns>
    public static void AddLuaContent(string name, StringBuilder content)
    {
        if (Content.TryGetValue(name,out StringBuilder? stringBuilder))
        {
            stringBuilder.AppendLine(content.ToString());
        }
        else
        {
            content.AppendLine();
            Content.Add(name,content);
        }
    }

    public static StringBuilder GetXlsxToLuaContent(TableDataInfo dataInfo)
    {
        string tableName = dataInfo.TableName;
        int columns = dataInfo.Columns;
        int rows = dataInfo.Rows;
        StringBuilder content = new StringBuilder();
        bool isSummary = AppConfig.AppData.SummaryConfig;
        
        if (isSummary)
        {
            AppendLineContentIndent(content,$"---@type Cfg_{tableName}[]");
            AppendLineContentIndent(content,$"{tableName} = {{");
        }
        else
        {
            if (AppConfig.AppData.CommentFile)
                AppendLineContent(content, $"---@type Cfg_{tableName}[]");
            else
                AppConfig.GenAnnotation(tableName,content);
            
            AppendLineContent(content,$"local {tableName} = {{");
            --AppConfig.IndentLevel;
        }

        int starIdx = (int)AppConfig.AppData.StartingRow;
        for (int rowIdx = starIdx; rowIdx <= rows; ++rowIdx)
        {
            
            int rowIndex = rowIdx - 1;
            var valueId = dataInfo.Cells[rowIdx, 1].Value;
            ++AppConfig.IndentLevel;
            if (AppConfig.AppData.OutputArray)
            {
                AppendLineContentIndent(content,'{');
            }
            else
            {
                if (valueId == null)
                {
                    continue;
                }
                
                AppendLineContentIndent(content,$"[{valueId}] = {{");
            }
            
            for (int columnIdx = 1; columnIdx < columns + 1; ++columnIdx)
            {
                int columnIndex = columnIdx - 1;
                if (AppConfig.VarTypeIsNull(columnIndex))
                {
                    continue;
                }

                
                string varKey = AppConfig.varNames[columnIndex];
                string varType = AppConfig.varTypes[columnIndex];
                
                var rowData = dataInfo.Cells[rowIdx, columnIdx].Value;
                bool isNull = rowData == null;
                var isTranslation = dataInfo.Cells[4, columnIdx].Value;
                bool isTranslationVal = false;
                if (isTranslation != null)
                {
                    isTranslationVal = isTranslation.ToString().ToLower().Equals("true");
                }
                
                if (isTranslationVal)
                {
                    string keyId = $"{tableName}_{varKey}_{valueId}";
                    if (rowData != null)
                    {
                        if (!MultilingualList.ContainsKey(keyId))
                        {
                            // 新增翻译多语言
                            MultilingualList.Add(keyId,new List<object>(){rowData});
                        }
                        else
                        {
                            // 比较是否一致
                            var multilingual = MultilingualList[keyId];
                            if (!multilingual[0].Equals(rowData))
                            {
                               // 不一样则加入标志，填入翻译表的附表中
                               // 第一列填入key,第二列填入新值，第二列填入旧值
                               MultilingualListTmp.Add(keyId,new List<object>(){multilingual[0],rowData});
                            }
                        }
                    }

                    rowData = keyId;
                }
                GenLuaTableData(tableName, content, varKey, varType, rowData, isNull, rowIdx, columnIdx);
            }
            AppendContentIndent(content,"},");
            AppendLineContentIndent(content,"");
            --AppConfig.IndentLevel;
        }

        if (isSummary)
            AppendLineContentIndent(content, "},");
        else
        {
            ++AppConfig.IndentLevel;
            AppendLineContent(content,'}');
            AppendLineContent(content,"");
            AppendLineContent(content,$"return {tableName}");
        }
        
        return content;
    }

    /// <summary>
    /// 获取总表
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string,StringBuilder> GetSummaryConfigTable()
    {
        Dictionary<string, List<string>> folderTables = new Dictionary<string, List<string>>();
        string curFolderName = null;
        foreach (var item in AppConfig.DataTables)
        {
            if (curFolderName != item.Value.FolderName)
            {
                curFolderName = item.Value.FolderName;
                folderTables.Add(curFolderName, new List<string>());
            }
            folderTables[curFolderName].Add(item.Value.TableName);
        }
        
        StringBuilder comment = new StringBuilder();
        string[] folders = folderTables.Keys.ToArray();
        for (int i = 0; i < folders.Length; i++)
        {
            comment.Clear();

            comment.AppendLine($"---@class Cfg_{folders[i]}");
            foreach (string tableName in folderTables[folders[i]])
            {
                comment.AppendLine($"---@field {tableName} Cfg_{tableName}[]");
            }
            comment.AppendLine();
            AddComment(folders[i], comment.ToString());
        }

        Dictionary<string,StringBuilder> builders = new Dictionary<string,StringBuilder>();
        for (int i = 0; i < folders.Length; i++)
        {
            string name = folders[i];
            StringBuilder merge = new StringBuilder();
            merge.AppendLine($"---@type Cfg_{name}");

            merge.AppendLine($"local cfg_{name} = {{");

            var content = Content[name];
            merge.Append($"{content}");
            merge.AppendLine("}");
            merge.AppendLine("");

            merge.Append($"return cfg_{name}");
            builders.Add(name,merge);
        }

        return builders;
    }

    /// <summary>
    /// 获取i18n
    /// </summary>
    /// <returns></returns>
    public static StringBuilder GetI18N()
    {
        AppConfig.ResetTableItem();

        StringBuilder content = new StringBuilder();
        content.AppendLine($"local multilingual = {{");
        foreach (var item in MultilingualDataDic)
        {
            // 添加Key
            AppendLineContentIndent(content, $"[\"{item.Key}\"] = {{");
            ++AppConfig.IndentLevel;
            // 加入数据
            foreach (var itemValues in item.Value.Values)
            {
                AppendLineContentIndent(content, $"[\"{itemValues.Key}\"] = \"{itemValues.Value}\",");
            }
            --AppConfig.IndentLevel;
            content.Remove(content.Length - 1, 1);
            AppendLineContentIndent(content, "},");
        }
        content.Remove(content.Length - 1, 1);
        content.AppendLine("}");


        content.AppendLine("---@class Cfg_I18n");
        content.AppendLine("---@field language string");
        content.AppendLine("local i18n = {");
        foreach (var item in MultilingualList)
        {
            AppendLineContentIndent(content, $"{item.Key} = multilingual[\"{item.Value[0]}\"],");
        }
        content.Remove(content.Length - 1, 1);
        content.AppendLine("}");
        content.AppendLine();
        
        content.AppendLine("---@type Cfg_I18n");
        content.AppendLine("local t = { language = \"en\" }");
        //添加原表操作
        content.AppendLine("setmetatable(t, {");
        content.AppendLine("\t__index = function (t, key)");
        content.AppendLine("\t\tif i18n[key] then return i18n[key][t.language] end");
        content.AppendLine("\t\tassert(i18n[key],(\"index == nil 多语言中不存在键：%s\\n%s\"):format(tostring(key), debug.traceback()))");
        content.AppendLine("\tend");
        content.AppendLine("})");
        content.AppendLine();
        
        content.AppendLine("return t");
        return content;
    }

    /// <summary>
    /// 加载多语言翻译表
    /// </summary>
    /// <param name="filePath"></param>
    public static void LoadMultiLanguageData(string filePath)
    {
        MultilingualList.Clear();
        MultilingualKeys.Clear();
        MultilingualNames.Clear();
        MultilingualDataDic.Clear();

        if (File.Exists(filePath))
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ExcelPackage package = new ExcelPackage(fileStream);
                // 获取第一个工作簿数据
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                // 打印工作簿名字
                Logger.LogInfo($"成功读取[{AppConfig.MultilingualName}]表的【{worksheet.Name}】工作簿");
                //获取有效行数
                int rows = worksheet.Dimension.Rows;
                //获取有效列数
                int columns = worksheet.Dimension.Columns;
                
                //获取注释数据
                
                for (int column = 2; column <= columns; ++column)
                {
                    var data = worksheet.Cells[1, column];
                    var data1 = worksheet.Cells[2, column];
                    if (data1.Value != null)
                    {
                        MultilingualNames.Add(data.Value);
                        MultilingualKeys.Add(data1.Value);
                    }
                }
                
                // 开始遍历行数
                for (int row = 4; row < rows + 1; ++row)
                {
                    var mainKey = worksheet.Cells[row, 1].Value;
                    List<object> values = new List<object>();
                    for (int column = 2; column <= columns; ++column)
                    {
                        var data = worksheet.Cells[row, column];
                        var rowData = data.Value;
                        //Logger.LogInfo($"行数[{row}],列数[{column}],名字:{data},数据:{rowData}");
                        values.Add(rowData);
                    }
                    MultilingualList.Add(mainKey,values);
                }
            }
        }
        else
        {
            for (var i = 0; i < AppConfig.AppData.Translations.Length; i++)
            {
                var data = AppConfig.AppData.TranslationNames[i];
                var data1 = AppConfig.AppData.Translations[i];
                MultilingualKeys.Add(data1);
                MultilingualNames.Add(data);
            }
        }
        
        //整合已经存在的翻译
        RefreshMultiLanguageData();
    }

    /// <summary>
    /// 刷新整合的翻译数据
    /// </summary>
    public static void RefreshMultiLanguageData()
    {
        foreach (var keyValuePair in MultilingualList)
        {
            MultilingualData data = new MultilingualData();
            data.Key = keyValuePair.Value[0];
            data.Values = new Dictionary<object, object>();
            for (int i = 0; i < MultilingualKeys.Count; i++)
            {
                var key = MultilingualKeys[i];
                object val = "";
                if (keyValuePair.Value.Count > i)
                {
                    val = keyValuePair.Value[i];
                }
                data.Values.Add(key,val);
            }
            MultilingualDataDic.TryAdd(data.Key,data);
        }
    }

    /// <summary>
    /// 保存翻译多语言表
    /// </summary>
    /// <param name="filePath"></param>
    public static void SaveMultiLanguageData(string filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;//指明非商业应用
        using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
        {
            if (package.Workbook.Worksheets.Count == 0)
            {
                package.Workbook.Worksheets.Add("翻译表");
            }
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            
            // 添加表头
            worksheet.Cells[1, 1].Value = "多语言Key";
            worksheet.Cells[2, 1].Value = "key";
            Logger.LogInfo($"准备写出工作【{worksheet.Name}】");
            for (int i = 0; i < MultilingualKeys.Count; i++)
            {
                var key = MultilingualKeys[i];
                var name = MultilingualNames[i];
                worksheet.Cells[1, i + 2].Value = name;
                worksheet.Cells[2, i + 2].Value = key;
            }
            // 添加内容
            var keys = MultilingualList.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var lst = MultilingualList[key];
                //从第四行第一列开始
                worksheet.Cells[i + 4, 1].Value = key;
                for (int j = 0; j < lst.Count; j++)
                {
                    var val = lst[j];
                    //第二列开始
                    worksheet.Cells[i + 4, j+ 2].Value = val;
                }
            }
            // 修改表头背景颜色
            for (int i = 0; i < 3; i++)
            {
                worksheet.Rows[i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Rows[i + 1].Style.Fill.BackgroundColor.SetColor(255,112,173,71);
            }
            
            // 保存
            package.Save();
        }
    }
}