using System.ComponentModel;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Xlsx2Lua;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
#if NET5_0_OR_GREATER
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; //指明非商业应用
        try
        {
            // args = new[] { "--" };
            // 初始化参数
            InitData(args);

            DateTime dateTimeAll = DateTime.Now;
            AppConfig.RefreshCommitRecord();
            if (AppConfig.AppData.CommitRecordType != ECommitRecordType.None)
            {
                Logger.LogWarning($"当前Config的最新提交记录：{AppConfig.CommitRecord}");
            }
            LoadMultiLanguageData();
            LoadExcelData();
            StartExcelToLua();

            if (AppConfig.AppData.ShowTimelapse)
                Logger.LogInfo($"导表总耗时：{(DateTime.Now - dateTimeAll).TotalMilliseconds}");

            Logger.LogInfo("导表完成");

            AppConfig.SaveLogFile();
        }
        catch (Exception ex)
        {
            Logger.LogException(ex);
            AppConfig.SaveLogFile();
        }
    }

    static void InitData(string[] args)
    {
        if (args.Length > 0)
        {
            string val = args[0];
            // 检测是否使用配置文件
            if (val.StartsWith("--"))
            {
                Logger.LogInfo("读取Xlsx2Lua配置");
                // 初始化配置数据
                AppConfig.ReadConfigData();
            }
            else
            {
                AppConfig.ReadConfigData();
                
                if (args.Length < 1)
                {
                    Logger.LogErrorAndExit("未输入Excel表格所在目录");
                    return;
                }

                if (args.Length < 2)
                {
                    Logger.LogErrorAndExit("未输入导出位置路径");
                    return;
                }

                //设置 Excel 目录
                AppConfig.AppData.ConfigPath = args[0];

                //设置 导出文件 目录
                AppConfig.AppData.OutputPath = args[1];

                //设置 导出文件 扩展名
                if (args.Length > 2)
                    AppConfig.AppData.OutputExt = args[2];

                //设置读 Excel 有效起始行数
                if (args.Length > 3)
                    AppConfig.AppData.StartingRow = int.Parse(args[3]);

                //设置 导出文件 是否有序
                if (args.Length > 4)
                    AppConfig.AppData.OutputArray = bool.Parse(args[4]);

                //是否生成总表
                if (args.Length > 5)
                    AppConfig.AppData.SummaryConfig = bool.Parse(args[5]);

                //是否拆分导出文件的注解
                if (args.Length > 6)
                    AppConfig.AppData.CommentFile = bool.Parse(args[6]);

                //是否显示流逝时间
                if (args.Length > 7)
                    AppConfig.AppData.ShowTimelapse = bool.Parse(args[7]);

                //是否开启翻译统计
                if (args.Length > 8)
                    AppConfig.AppData.Translation = bool.Parse(args[8]);

                //是否开启实时翻译
                if (args.Length > 9)
                    AppConfig.AppData.RealtimeTrans = bool.Parse(args[9]);

                //显示日志的等级
                if (args.Length > 10)
                    AppConfig.AppData.ShowLogLevel = int.Parse(args[10]);

                //是否输出日志文件
                if (args.Length > 11)
                    AppConfig.AppData.OutputLogFile = bool.Parse(args[11]);

                //获取提交记录类型，默认0
                if (args.Length > 12)
                    AppConfig.AppData.CommitRecordType = (ECommitRecordType)int.Parse(args[12]);

                //保存配置数据
                AppConfig.SaveAppData();
            }
        }
        else
        {
            Logger.LogErrorAndExit("参数错误");
            return;
        }
    }
    
    /// <summary>
    /// 加载多语言表
    /// </summary>
    static void LoadMultiLanguageData()
    {
        // 检测多语言表是否存在
        string filePath = Path.Combine(AppConfig.AppData.ConfigPath, AppConfig.MultilingualName);
        ExportLuaHelper.LoadMultiLanguageData(filePath);
    }

    static void LoadExcelData()
    {
        if (AppConfig.AppData.SummaryConfig)
        {
            Dictionary<string, List<FileInfo>> fileDic = new Dictionary<string, List<FileInfo>>();
            //获取当前文件夹中的子文件夹
            FileUtils.GetTopDirectoryFiles(AppConfig.AppData.ConfigPath, ref fileDic);
            //遍历文件
            foreach (var (folder, fileInfos) in fileDic)
            {
                foreach (FileInfo fileInfo in fileInfos)
                {
                    if (fileInfo.Name.StartsWith("~$"))
                        continue;

                    string ext = fileInfo.Extension.ToLower();
                    if (!ext.Equals(".xlsx"))
                        continue;

                    ReadXlsxData(fileInfo, folder);
                }
            }
        }
        else
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            FileUtils.GetDirectoryFiles(AppConfig.AppData.ConfigPath, ref fileInfos);

            foreach (FileInfo fileInfo in fileInfos)
            {
                if (fileInfo.Name.StartsWith("~$"))
                    continue;

                string ext = fileInfo.Extension.ToLower();
                if (!ext.Equals(".xlsx"))
                    continue;

                ReadXlsxData(fileInfo);
            }
        }
    }

    static void StartExcelToLua()
    {
        // 开始转表
        string[] keys = AppConfig.DataTables.Keys.ToArray();
        for (int i = 0; i < keys.Length; i++)
        {
            DateTime newTableDateTime = DateTime.Now;
            AppConfig.ResetTableItem();

            string key = keys[i];

            Logger.LogInfo($"============准备导表 {key}============");
            TableDataInfo tableDataInfo = AppConfig.DataTables[key];
            Logger.LogInfo($"{key} 开始解析字段类型");

            AppConfig.AddAnnotation(tableDataInfo.Cells, tableDataInfo.Columns);

            Logger.LogInfo($"{key} 开始解析lua表");
            if (AppConfig.AppData.SummaryConfig)
            {
                ExportLuaHelper.AddComment(tableDataInfo.TableName, AppConfig.GetAnnotation(tableDataInfo.TableName));
                ExportLuaHelper.AddLuaContent(tableDataInfo.FolderName,
                    ExportLuaHelper.GetXlsxToLuaContent(tableDataInfo));
            }
            else
            {
                if (AppConfig.AppData.CommentFile)
                    ExportLuaHelper.AddComment(tableDataInfo.TableName,
                        AppConfig.GetAnnotation(tableDataInfo.TableName));
                var content = ExportLuaHelper.GetXlsxToLuaContent(tableDataInfo);
                string ext = AppConfig.AppData.OutputExt;
                string fileName = ext.StartsWith('.')
                    ? $"{tableDataInfo.TableName}{ext}"
                    : $"{tableDataInfo.TableName}.{ext}";
                FileUtils.Save(Path.Combine(AppConfig.AppData.OutputPath, fileName), content);
            }

            if (AppConfig.AppData.ShowTimelapse)
                Logger.LogInfo(
                    $"{tableDataInfo.FileName} - {tableDataInfo.TableName} 导表完成 耗时：{(DateTime.Now - newTableDateTime).TotalMilliseconds}");
            else
                Logger.LogInfo($"{tableDataInfo.FileName} - {tableDataInfo.TableName} 导表完成");
        }

        // 更新翻译表
        Logger.LogInfo("更新多语言表信息");
        ExportLuaHelper.RefreshMultiLanguageData();

        if (AppConfig.AppData.SummaryConfig)
        {
            Logger.LogInfo("开始生成lua总表...");
            string ext = AppConfig.AppData.OutputExt;
            var all = ExportLuaHelper.GetSummaryConfigTable();
            var allKeys = all.Keys.ToArray();
            for (int i = 0; i < allKeys.Length; i++)
            {
                var name = allKeys[i];
                var content = all[name];
                string fileName = ext.StartsWith('.') ? $"{name}{ext}" : $"{name}.{ext}";
                FileUtils.Save(Path.Combine(AppConfig.AppData.OutputPath, fileName), content);
            }
        }

        Logger.LogInfo("生成多语言表 . . .");
        FileUtils.Save(Path.Combine(AppConfig.AppData.OutputPath, "i18n.lua"), ExportLuaHelper.GetI18N());
        if (AppConfig.AppData.CommentFile)
        {
            Logger.LogInfo("生成lua注释 . . .");
            FileUtils.Save(Path.Combine(AppConfig.AppData.OutputPath, "ConfigComment.lua"),
                ExportLuaHelper.CommentToString());
        }

        if (AppConfig.AppData.Translation)
        {
            Logger.LogInfo($"导出翻译文件{AppConfig.MultilingualName} . . .");
            OutMainExcel();
        }
    }

    static void ReadXlsxData(FileInfo fileInfo, string folder = "")
    {
        //检查文件名
        if (fileInfo.Name.StartsWith("~$") || fileInfo.Name.StartsWith("Translate"))
        {
            return;
        }

        //检查扩展名
        string ext = fileInfo.Extension.ToLower();
        if (!ext.Equals(".xlsx"))
        {
            return;
        }

        FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        ExcelPackage package = new ExcelPackage(fileStream);
        for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[i];
            var sheetNames = worksheet.Name.Split('|');
            if (sheetNames.Length < 2)
            {
                continue;
            }

            //获取表名
            string tableName = sheetNames.Last();
            //获取有效行数
            int rows = worksheet.Dimension.Rows;
            //获取有效列数
            int columns = worksheet.Dimension.Columns;
            //Logger.LogInfo($"FileName:{fileInfo.Name},TableName:{tableName},maxRow:{rows},maxColumn:{columns}");

            //检测有效行数
            if (rows < AppConfig.AppData.StartingRow)
            {
                continue;
            }

            // 构建数据
            TableDataInfo tableDataInfo = new TableDataInfo();
            tableDataInfo.Rows = rows;
            tableDataInfo.Columns = columns;
            tableDataInfo.FileName = fileInfo.Name;
            tableDataInfo.TableName = tableName;
            tableDataInfo.FolderName = folder;
            tableDataInfo.Cells = worksheet.Cells;
            Logger.LogInfo($"加载文件{fileInfo.Name}中的{tableName}工作簿.");

            // 准备加入数据
            string key = tableName;
            if (AppConfig.AppData.SummaryConfig)
                key = $"{folder}|{tableName}";
            if (AppConfig.DataTables.TryGetValue(key, out TableDataInfo existInfo))
            {
                string errFormat = "导出配置表文件【{0}】的工作簿”{1}“时，发现已经被配置表文件【{2}】占用，请修改当前配置表工作簿名”{3}“";
                string error = string.Format(errFormat, tableDataInfo.FileName, tableDataInfo.TableName,
                    existInfo.FileName, tableDataInfo.TableName);
                Logger.LogErrorAndExit(error);
                continue;
            }

            AppConfig.DataTables.Add(key, tableDataInfo);
        }
        fileStream.Close();
        fileStream.Dispose();
        
    }

    static void OutMainExcel()
    {
        string filePath = Path.Combine(AppConfig.AppData.ConfigPath, AppConfig.MultilingualName);
        FileInfo file = new FileInfo(filePath);
        if (file.Exists)
            file.Delete();
        ExportLuaHelper.SaveMultiLanguageData(filePath);
    }
}