using System.Diagnostics;

public class UploadService {
    public async Task<bool> ScanWithWindowsDefender(string filePath) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "powershell",
                Arguments = $"Start-MpScan -ScanPath {filePath}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0; // 0 = Clean
    }

}
