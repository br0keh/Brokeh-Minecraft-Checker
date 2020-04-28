using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Net;
using System.Diagnostics;

namespace Brokeh_Minecraft_Checker
{
    /// <summary>
    /// The Start Screen is shown when the application boots up
    /// There everything can be configured
    /// </summary>
    public partial class StartScreen : Form
    {
        private string _currentHardwareId;

        public StartScreen()
        {
            Program.KillDnSpyProcessByProcessList();
            Program.KillDnSpyProcessByName();
            InitializeComponent();
        }

        /// <summary>
        /// Retrieves the Hardware Id
        /// </summary>
        /// <returns>The fetched hardware id</returns>
        private static string GetHardwareId()
        {
            const string drive = @"c";

            var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            dsk.Get();

            string volumeSerial = dsk["VolumeSerialNumber"].ToString();
            return volumeSerial;
        }

        private void StartScreen_Load(object sender, EventArgs e)
        {
            Program.KillDnSpyProcessByName();

            if (Properties.Settings.Default.hwid.Length <= 0)
            {
                textBox1.Text = GetHardwareId();
                Properties.Settings.Default.hwid = GetHardwareId();
                Properties.Settings.Default.Save();
            }

            textBox1.Text = GetHardwareId();
            _currentHardwareId = Properties.Settings.Default.hwid;

            var webClient = new WebClient();
            try
            {
                string response =
                    webClient.DownloadString("http://brokeh-checker.tk/check.php?hwid=" + _currentHardwareId);

                checkerStatus.Text = "Online";
            }
            catch
            {
                checkerStatus.Text = "Offline temporarily";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(_currentHardwareId);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkerStatus.Text != "Online")
            {
                MessageBox.Show("Checker offline!");
            }
            else
            {
                string error2 = "Internal error!";
                WebClient wC = new WebClient();
                try
                {
                    string resposta =
                        wC.DownloadString("http://brokeh-checker.tk/check.php?hwid=" + _currentHardwareId);

                    if (resposta.Contains("OK|"))
                    {
                        Form1 formd = new Form1(_currentHardwareId);

                        this.Hide();
                        formd.Show();
                    }
                    else if (resposta.Contains("Not authorized!"))
                    {
                        MessageBox.Show(resposta);
                        Application.Exit();
                    }
                    else
                    {
                        MessageBox.Show(error2);

                        Application.Exit();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Checker offline!");
                }
            }
        }
    }
}