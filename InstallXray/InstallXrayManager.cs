using System.Diagnostics;
using System.Text;

namespace XrayServerAPI.InstallXray;

public class InstallXrayManager
{
    private readonly string DOMAIN;
    public InstallXrayManager(string domain)
    {
        DOMAIN = domain;
    }

    public void Install()
    {
        try
        {
            Console.WriteLine("Starting Xray installation...");
            ExecBash(GetInstallCertProc(DOMAIN));
            ExecBash(GetInstallXrayProc(DOMAIN));
            Console.WriteLine("Xray installation completed successfully.");
        }
        catch(Exception ex)
        {
            Console.WriteLine("Error installing Xray: " + ex.Message);
        }
    }

    private Process GetInstallCertProc(string domain)
    {
        var certScriptPath = Path.Combine(AppContext.BaseDirectory, "InstallXray", "InstallCert.bash");
        Process instCert = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"{certScriptPath} {domain}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        return instCert;
    }

    private Process GetInstallXrayProc(string domain)
    {
        var scriptPath = Path.Combine(
            AppContext.BaseDirectory,
            "InstallXray",
            "InstallXray.bash"
        );

        return new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"{scriptPath} {domain}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
    }

    private void ExecBash(Process process)
    {
        var errorBuilder = new StringBuilder();
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine("ERR: " + e.Data);
            errorBuilder.AppendLine(e.Data);
        };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception(
                $"Bash script failed with exit code {process.ExitCode}\n{errorBuilder}"
            );
        }
    }
}
