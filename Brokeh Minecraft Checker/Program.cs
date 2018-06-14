using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brokeh_Minecraft_Checker
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var processes = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)).ToList();

            foreach (var process in processes)
            {
                var id = process.Id;
                var Wintitle = process.MainWindowTitle.ToString();
                if (Wintitle.Contains("dnSpy"))
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
            Process[] processos = Process.GetProcessesByName("dnSpy");
            if (processos.Length != 0)
            {
                foreach (var item in processos)
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
             processos = Process.GetProcessesByName("dnSpy");
            if (processos.Length != 0)
            {
                foreach (var item in processos)
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
             processos = Process.GetProcessesByName("dnSpy");
            if (processos.Length != 0)
            {
                foreach (var item in processos)
                {
                    Process.GetCurrentProcess().Kill();
                }
                 processos = Process.GetProcessesByName("dnSpy");
                if (processos.Length != 0)
                {
                    foreach (var item in processos)
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
                 processos = Process.GetProcessesByName("dnSpy");
                if (processos.Length != 0)
                {
                    foreach (var item in processos)
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
                 processos = Process.GetProcessesByName("dnSpy");
                if (processos.Length != 0)
                {
                    foreach (var item in processos)
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
                 processos = Process.GetProcessesByName("dnSpy");
                if (processos.Length != 0)
                {
                    foreach (var item in processos)
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                     processos = Process.GetProcessesByName("dnSpy");
                    if (processos.Length != 0)
                    {
                        foreach (var item in processos)
                        {
                            Process.GetCurrentProcess().Kill();
                        }
                         processos = Process.GetProcessesByName("dnSpy");
                        if (processos.Length != 0)
                        {
                            foreach (var item in processos)
                            {
                                Process.GetCurrentProcess().Kill();
                            }
                        }
                         processos = Process.GetProcessesByName("dnSpy");
                        if (processos.Length != 0)
                        {
                            foreach (var item in processos)
                            {
                                Process.GetCurrentProcess().Kill();
                            }
                        }
                         processos = Process.GetProcessesByName("dnSpy");
                        if (processos.Length != 0)
                        {
                            foreach (var item in processos)
                            {
                                Process.GetCurrentProcess().Kill();
                            }
                        }
                        processos = Process.GetProcessesByName("dnSpy");
                        if (processos.Length != 0)
                        {
                            foreach (var item in processos)
                            {
                                Process.GetCurrentProcess().Kill();
                            }
                        }
                    }
                }
            }
            Application.EnableVisualStyles();
             processes = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)).ToList();

            foreach (var process in processes)
            {
                var id = process.Id;
                var Wintitle = process.MainWindowTitle.ToString();
                if (Wintitle.Contains("dnSpy"))
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
            Application.SetCompatibleTextRenderingDefault(false);
             processes = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)).ToList();

            foreach (var process in processes)
            {
                var id = process.Id;
                var Wintitle = process.MainWindowTitle.ToString();
                if (Wintitle.Contains("dnSpy"))
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
            Application.Run(new StartScreen());
             processes = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)).ToList();

            foreach (var process in processes)
            {
                var id = process.Id;
                var Wintitle = process.MainWindowTitle.ToString();
                if (Wintitle.Contains("dnSpy"))
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
             processes = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)).ToList();

            foreach (var process in processes)
            {
                var id = process.Id;
                var Wintitle = process.MainWindowTitle.ToString();
                if (Wintitle.Contains("dnSpy"))
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
        }
    }
}
