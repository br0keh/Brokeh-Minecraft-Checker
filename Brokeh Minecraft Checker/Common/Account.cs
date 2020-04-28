using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Brokeh_Minecraft_Checker.Common
{
    public class Account
    {
        /// <summary>
        /// The proxy which was used to resolve the account
        /// </summary>
        public string Proxy { get; set; }

        /// <summary>
        /// The account type, such as
        /// - None: None
        /// - SFA: Semi full access
        /// - FA: Full access
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The username of the account
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// The password of the account
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// The extras of the account, will be empty if none have been found
        /// </summary>
        public List<AccountExtra> Extras { get; }

        public Account(string username, string password)
        {
            Username = username;
            Password = password;
            Extras = new List<AccountExtra>();
        }

        public override string ToString()
        {
            return $"{nameof(Username)}: {Username}, {nameof(Password)}: {Password}, {nameof(Proxy)}: {Proxy}";
        }

        public string ToCsv()
        {
           
            
            return $"{Username}:{Password}," +
                   $"{string.Join("|", Extras.Select(extra => extra.Name))}," +
                   $"{Proxy}";
        }
    }
}