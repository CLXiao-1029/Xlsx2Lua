using System.Text;

namespace Xlsx2Lua;

internal class Logger
{
    enum LogLevel
    {
        All = 0,
        Warning = 1,
        Error = 2
    }
    
    private static readonly StringBuilder LogContent = new StringBuilder();

    public static void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Log($"LogInfo:{message}",LogLevel.All);
    }

    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Log($"LogError:{message}",LogLevel.Error);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Log($"LogWarning:{message}",LogLevel.Warning);
        Console.ForegroundColor = ConsoleColor.White;
    }
    
    public static void LogException(Exception message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Log($"LogException:{message}");
        Log($"LogException:程序被迫退出，请修正错误后重试");
        Console.ForegroundColor = ConsoleColor.White;
        Environment.Exit(0);
    }

    /// <summary>
    /// 输出错误信息并在用户按任意键后退出
    /// </summary>
    public static void LogErrorAndExit(string errorString)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Log($"LogError:{errorString}");
        Log($"LogError:程序被迫退出，请修正错误后重试");
        Console.ForegroundColor = ConsoleColor.White;
        Console.ReadKey();
        Environment.Exit(0);
    }
    
    public static string LogToString()
    {
        return LogContent.ToString();
    }

    private static void Log(object message,LogLevel level = LogLevel.All)
    {
        if ((LogLevel)AppConfig.AppData.ShowLogLevel <= level)
            Console.WriteLine($"{message}");

        if (AppConfig.AppData.OutputLogFile)
            LogContent.AppendLine($"{message}");
    }
}