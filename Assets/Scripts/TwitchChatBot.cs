using System;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using System.Threading;
using Assets.Data;
using Assets.Services;

namespace Assets
{
    public class TwitchChatBot
    {
        const string oauth = "oauth:ehxeo6hhb4ip503m1f2fvdc7fq2ghb";

        const string oauthFormat = "PASS {0}";
        const string messageFormat = "PRIVMSG #{0} :{1}";

        // server to connect to (edit at will)
        private readonly string _server;
        // server port (6667 by default)
        private readonly int _port;
        // user information defined in RFC 2812 (IRC: Client Protocol) is sent to the IRC server 
        private readonly string _user;

        // the bot's nickname
        private readonly string _nick;
        // channel to join
        private readonly string _channel;

        private readonly int _maxRetries;

        private int countofThing = 0;

        private bool stop = false;


        public TwitchChatBot(string server, int port,  string nick, string channel, int maxRetries = 3)
        {
            _server = server;
            _port = port;
            _nick = nick;
            _channel = channel;
            _maxRetries = maxRetries;
        }

        public void Start()
        {
            IrcClient irc = new IrcClient("irc.twitch.tv", 6667,
                _nick, oauth, _channel);
            PingSender ping = new PingSender(irc);
            ping.Start();
            Debug.LogWarning("START CHAT BOT");
            // Listen to the chat until program exits
            while (true)
            {
                // Read any message from the chat room
                string message = irc.ReadMessage();
                Console.WriteLine(message); // Print raw irc messages

                if (message.Contains("PRIVMSG"))
                {
                    // Messages from the users will look something like this (without quotes):
                    // Format: ":[user]![user]@[user].tmi.twitch.tv PRIVMSG #[channel] :[message]"

                    // Modify message to only retrieve user and message
                    int intIndexParseSign = message.IndexOf('!');
                    string userName = message.Substring(1, intIndexParseSign - 1); // parse username from specific section (without quotes)
                                                                                   // Format: ":[user]!"
                                                                                   // Get user's message
                    intIndexParseSign = message.IndexOf(" :");
                    message = message.Substring(intIndexParseSign + 2);

                    //Console.WriteLine(message); // Print parsed irc message (debugging only)

                    // Broadcaster commands
                    if (userName.Equals(_channel))
                    {
                        if (message.Equals("!exitbot"))
                        {
                            irc.SendPublicChatMessage("Bye! Have a beautiful time!");
                            Environment.Exit(0); // Stop the program
                        }
                    }

                    // General commands anyone can use
                    if (message.Equals("!hello"))
                    {
                        irc.SendPublicChatMessage("Hello World!");
                    }
                    if (message.Equals("!taxes"))
                    {
                        irc.SendPublicChatMessage("The Mitrochrondria is the powerhouse of the cell.");
                    }
                    if (message.Equals("!balance"))
                    {
                        var bal = DataService.Instance.GetBalance(userName); 
                        irc.SendPublicChatMessage(string.Format("{0}'s Balance is {1}. ", userName, bal));
                    }
                    if (message.StartsWith("!bet"))
                    {
                        var msg = message.Split(' ');
                        if (!int.TryParse(msg[1], out int team))
                        {
                            irc.SendPublicChatMessage(string.Format("Could not parse  team of {0}", message));
                            continue;
                        }
                        if (!int.TryParse(msg[2], out int amount))
                        {
                            irc.SendPublicChatMessage(string.Format("Could not parse  amount of {0}", message));
                            continue;
                        }
                        var bal = DataService.Instance.GetBalance(userName);
                        if (bal < amount)
                        {
                            irc.SendPublicChatMessage(string.Format("Sorry {0} you don't have enough money, your balance is {1}. ", userName, bal));
                        }
                        BettingService.Instance.AddBetToTeam(userName, amount, team);
                    }
                }
            }

        }

        public void OnEnd()
        {
            stop = true;
        }

        public void Connect()
        {
// irc myClient = new irc(_server, _port, _nick, oauth, _channel);


        }

        public void Disconnect()
        {

        }
    }
}