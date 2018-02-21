using Sync.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.MessageFilter;
using System.Net.Sockets;
using System.IO;
using Sync.Tools;
using System.Threading;
using Sync.Source;
using System.Text.RegularExpressions;

namespace DefaultPlugin.Clients
{
    public class DirectOSUIRCBot : DefaultClient, IConfigurable
    {
        public const string CONST_ACTION_FLAG = "\x0001ACTION ";

        TcpClient tcpClient;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;

        Thread recive, send;

        bool workStatus = false;

        Queue<string> messageQueue;

        public static ConfigurationElement IRCBotName { get; set; } = "";
        public static ConfigurationElement IRCBotPasswd { get; set; } = "";
        public static ConfigurationElement IRCNick { get; set; } = "";

        static readonly Regex msgRegex = new Regex(":(.+?)!.+?:(.*?)$");

        public DirectOSUIRCBot() : base("Deliay", "DirectOsuIRCBot")
        {
            messageQueue = new Queue<string>();
        }

        public override void Restart()
        {
            StopWork();
            StartWork();
        }

        public override void SendMessage(IMessageBase message)
        {
            SendRawMessage($"PRIVMSG {message.User} :{message.Message}");
        }

        private void SendRawMessage(string msg)
        {
            messageQueue.Enqueue(msg);
        }

        private void ReciveRawMessage(string msg)
        {

            if (!msg.Contains(@"PRIVMSG "))
            {
                //处理非对话消息
                if (msg.StartsWith("PING "))
                    SendRawMessage(msg.Replace(@"PING", @"PONG"));
            }
            else
            {
                Match match = msgRegex.Match(msg);
                string nick = match.Groups[1].Value;
                string rawmsg = match.Groups[2].Value;
                DefaultPlugin.MainMessager.RaiseMessage<ISourceClient>(new IRCMessage(nick, rawmsg));
            }
        }

        private void ReciveLoop()
        {
            try
            {
                while (workStatus)
                {
                    Thread.Sleep(1);

                    if ((tcpClient.Client.Poll(20, SelectMode.SelectRead)) && (tcpClient.Client.Available == 0))
                    {
                        IO.CurrentIO.WriteColor("[Osu!IRC]"+Language.LANG_OSUIRC_NETWORK_INTERRUPTED, ConsoleColor.Red);
                        ConnectAndLogin();
                        continue;
                    }

                    if (ns.DataAvailable)
                    {
                        string message = sr.ReadLine();

                        ReciveRawMessage(message);
                    }
                }
            }
            catch(Exception e)
            {
                IO.CurrentIO.WriteColor("[Osu!IRC]Reciver occured error:"+e.Message,ConsoleColor.Red);
                CurrentStatus = SourceStatus.REMOTE_DISCONNECTED;
            }

            IO.CurrentIO.WriteColor("[Osu!IRC]Reciver thread finish", ConsoleColor.Yellow);
        }

        private void SendLoop()
        {
            try
            {
                while (workStatus)
                {
                    Thread.Sleep(1);
                    if (messageQueue.Count > 0)
                    {
                        if (!tcpClient.Connected)
                        {
                            //等Reciver线程处理即可
                            continue;
                        }

                        string message = string.Empty;
                        lock (messageQueue)
                        {
                            message = messageQueue.Dequeue();
                        }
                        if (message == string.Empty) continue;
                        if (!tcpClient.Connected)
                        {
                            workStatus = false;
                            CurrentStatus = SourceStatus.REMOTE_DISCONNECTED;
                            continue;
                        }
                        sw.WriteLine(message);
                        sw.Flush();
                        message = string.Empty;
                    }
                }
            }
            catch(Exception e)
            {
                IO.CurrentIO.WriteColor("[Osu!IRC]Sender occured error:" + e.Message, ConsoleColor.Red);
                CurrentStatus = SourceStatus.REMOTE_DISCONNECTED;
            }

            IO.CurrentIO.WriteColor("[Osu!IRC]Sender thread finish", ConsoleColor.Yellow);
        }


        public override void StartWork()
        {
            if (workStatus) return;
            EventBus.RaiseEvent(new ClientStartWorkEvent());

            ConnectAndLogin();

            workStatus = true;
            recive = new Thread(ReciveLoop);
            send = new Thread(SendLoop);
            recive.Start();
            send.Start();

            SendMessage(new IRCMessage(IRCNick.ToString(), "[DirectOSUIRCBot]Welcome!"));
        }

        private void ConnectAndLogin()
        {
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect("irc.ppy.sh", 6667);
                if (!tcpClient.Connected)
                {
                    throw new Exception("Network error!");
                }
                CurrentStatus = SourceStatus.CONNECTED_WAITING;
                ns = tcpClient.GetStream();
                sr = new StreamReader(ns);
                sw = new StreamWriter(ns);

                IRCLogin();

                IO.CurrentIO.WriteColor("[Osu!IRC]"+Language.LANG_OSUIRC_LOGIN_SUCCESS, ConsoleColor.Green);
            }
            catch(Exception e)
            {
                IO.CurrentIO.WriteColor("[Osu!IRC]"+string.Format(Language.LANG_OSUIRC_LOGIN_FAILED,e.Message), ConsoleColor.Yellow);
            }

        }

        private void IRCLogin()
        {
            sw.WriteLine($"PASS {IRCBotPasswd}");
            sw.WriteLine($"USER {IRCBotName} 1 * : {IRCBotName}");
            sw.WriteLine($"NICK {IRCBotName}");
            sw.Flush();

            this.NickName = IRCNick;

            CurrentStatus = SourceStatus.CONNECTED_WORKING;
        }

        public override void StopWork()
        {
            if(tcpClient != null && tcpClient.Connected)
            {
                sw.Write("QUIT");
                sw.Flush();
            }
            workStatus = false;
            EventBus.RaiseEvent(new ClientStopWorkEvent());
        }

        public override void SwitchOtherClient()
        {
            StopWork();
        }

        public override void SwitchThisClient()
        {
            CurrentStatus = SourceStatus.IDLE;
        }

        public void onConfigurationLoad()
        {
            this.NickName = IRCNick;
        }

        public void onConfigurationSave()
        {
            
        }

        public void onConfigurationReload()
        {
            this.NickName = IRCNick;
        }
    }
}
