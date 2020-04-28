using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brokeh_Minecraft_Checker
{
    internal static class Program
    {
        /// <summary>
        /// The main method
        /// </summary>
        [STAThread]
        private static void Main()
        {
            KillDnSpyProcessByName();

            for (int i = 0; i < 11; i++)
            {
                KillDnSpyProcessByProcessList();
            }

            Application.EnableVisualStyles();
            KillDnSpyProcessByName();

            Application.SetCompatibleTextRenderingDefault(false);
            KillDnSpyProcessByName();

            Application.Run(new StartScreen());
            KillDnSpyProcessByName();
            KillDnSpyProcessByName();
        }

        public static void KillDnSpyProcessByProcessList()
        {
            Process[] processesByName = Process.GetProcessesByName("dnSpy");
            if (processesByName.Length == 0)
                return;

            foreach (var item in processesByName)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        public static void KillDnSpyProcessByName()
        {
            List<Process> processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .ToList();

            foreach (var process in processes)
            {
                int id = process.Id;
                string winTitle = process.MainWindowTitle;
                if (winTitle.Contains("dnSpy"))
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
        }
    }
}