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
    public partial class StartScreen : Form
    {
        public StartScreen()
        {
            Process[] processos = Process.GetProcessesByName("dnSpy");
            if (processos.Length != 0)
            {
                foreach (var item in processos)
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
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
            InitializeComponent();
          
           
        }
        string currentHwid;
        private void label2_Click(object sender, EventArgs e)
        {

        }
        string hwid()
        {
            string drive = @"c";
            ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            dsk.Get();
            string volumeSerial = dsk["VolumeSerialNumber"].ToString();

            return volumeSerial.ToString();
        }
        private void StartScreen_Load(object sender, EventArgs e)
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
            if (Properties.Settings.Default.hwid.ToString().Length <= 0)
            {
                textBox1.Text = hwid();
                Properties.Settings.Default.hwid = hwid();
                Properties.Settings.Default.Save();
                
            }
            textBox1.Text = hwid();
            currentHwid = Properties.Settings.Default.hwid;

             WebClient wC = new WebClient();
            try
            {
                string resposta = wC.DownloadString("http://brokeh-checker.tk/check.php?hwid=" + currentHwid);

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
            Clipboard.SetText( currentHwid );
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(checkerStatus.Text != "Online")
            {
                MessageBox.Show("Checker offline!");
            }
            else
            {
                string error2 = "Internal error!";
                WebClient wC = new WebClient();
                try
                {
                    string resposta = wC.DownloadString("http://brokeh-checker.tk/check.php?hwid=" + currentHwid);

                    if (resposta.Contains("OK|"))
                    {
                        Form1 formd = new Form1(currentHwid);

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
