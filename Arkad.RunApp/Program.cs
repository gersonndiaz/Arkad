using System.Diagnostics;
using System.Runtime.InteropServices;

bool isOpen = true;

try
{
    var proc = new Process();
    proc.StartInfo.FileName = "Arkad.Server.exe";
    isOpen = proc.Start();
}
catch (Exception e)
{
    Console.WriteLine($"Error Iniciar Proceso: {e.Message}");
}

if (isOpen)
{
    OpenBrowser("http://localhost:5000");
}

static void OpenBrowser(string url)
{
    try
    {
        Process.Start(url);
    }
    catch (Exception e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            // throw 
        }

        Console.WriteLine($"Error Abrir Navegador: {e.Message}");
    }
}
