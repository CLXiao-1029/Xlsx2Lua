using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Xlsx2Lua;

internal class TortoiseGitHelper
{
    public static string GetLatestCommitRecord(string workPath)
    {
        string log;
        GitCommand("log -1 --pretty=%h",workPath,out log);
        return log.Trim();
    }

    private static void GitCommand(string command, string workingDirectory, out string line)
    {
        string fileName = "git";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName = "git.exe";
        }
        Process p = new Process();
        p.StartInfo.FileName = fileName;
        p.StartInfo.Arguments = command;
        p.StartInfo.WorkingDirectory = workingDirectory;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        p.Start();
        line = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        p.Close();
        p.Dispose();
    }
}