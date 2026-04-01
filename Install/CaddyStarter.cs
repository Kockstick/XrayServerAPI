using System.Diagnostics;
using System.Text;

namespace XrayServerAPI.Install;

public class CaddyStarter
{
    private readonly string DOMAIN;
    public CaddyStarter(string domain)
    {
        DOMAIN = domain;
    }

    public void Start()
    {
        try
        {
            Console.WriteLine("Starting Caddy...");
            ExecBash(GetInstallCaddyProc(DOMAIN));
            Console.WriteLine("Caddy starting completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error starting Caddy: " + ex.Message);
            throw new Exception("Error starting Caddy: ", ex);
        }
    }

    private Process GetInstallCaddyProc(string domain)
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Install", "InstallCaddy.bash");

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
