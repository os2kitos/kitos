using System;
using System.Diagnostics;
using System.Threading;

namespace CiscoAnyConnectTool
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var task = args[0];
                switch (task)
                {
                    case "disconnect":
                        return
                            ToProcessResult(
                                Disconnect()
                            );
                    case "connect":
                        return
                            ToProcessResult(
                                Connect(args)
                            );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return 0;
        }

        private static int ToProcessResult(bool result)
        {
            return result ? 0 : -1;
        }

        private static bool Disconnect()
        {
            var vpncli = CreateVpnProcess("disconnect");
            var start = vpncli.Start();
            vpncli.WaitForExit();
            return start;
        }

        private static Process CreateVpnProcess(string startupArguments)
        {
            var vpncli = new Process();
            vpncli.StartInfo.UseShellExecute = false;
            vpncli.StartInfo.FileName =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).TrimEnd('\\')}\\Cisco\\Cisco AnyConnect Secure Mobility Client\\vpncli.exe";
            vpncli.StartInfo.Arguments = startupArguments;
            vpncli.StartInfo.RedirectStandardInput = true;
            vpncli.StartInfo.RedirectStandardOutput = false;
            vpncli.StartInfo.CreateNoWindow = true;
            return vpncli;
        }

        private static bool Connect(string[] args)
        {
            var host = args[1];
            var username = args[2];
            var password = args[3];

            var vpncli = CreateVpnProcess("-s"); //-s enables input from std in
            var start = vpncli.Start();
            vpncli.StandardInput.WriteLine($"connect {host}{Environment.NewLine}{username}{Environment.NewLine}{password}");
            //Don't wait for exit - just allow it to boot
            Thread.Sleep(TimeSpan.FromSeconds(5));
            return start;
        }
    }
}
