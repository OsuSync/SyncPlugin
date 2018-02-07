using Sync.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sync;
using Sync.Command;
using Sync.MessageFilter;
using Sync.Tools;
using static Sync.Plugins.PluginEvents;
using System.Threading.Tasks;

namespace PPQuery
{
    [SyncPluginDependency("c620c63a-32b6-4281-87a9-b7da62be0215", Version = "^2.15.0", Require = true)]
    [SyncPluginID("5ab459a8-0f62-4500-8417-f71fab646493", "2.16.0")]
    public class PPQuery : Plugin, IFilter, ISourceClient
    {
        public PPQuery() : base("PP Query", "Deliay")
        {

        }

        public override void OnEnable()
        {
            Instance.BindEvent<InitFilterEvent>((evt) => evt.Filters.AddFilter(this));
            IO.CurrentIO.WriteColor("PP Query Plugin By Deliay >w<", ConsoleColor.DarkCyan);
        }

        public void onMsg(ref IMessageBase msg)
        {
            if (msg.User.RawText == SyncHost.Instance.ClientWrapper.Client.NickName)
            {
                if (msg.Message.RawText.StartsWith(DefaultPlugin.Clients.DirectOSUIRCBot.CONST_ACTION_FLAG) && msg.Message.RawText.Contains("osu.ppy.sh/b/"))
                {
                    msg.Cancel = true;
                    SyncHost.Instance.ClientWrapper.Client.SendMessage(new IRCMessage("tillerino", msg.Message.RawText));
                }
                else if (msg.Message.RawText.Contains("?with "))
                {
                    SyncHost.Instance.ClientWrapper.Client.SendMessage(new IRCMessage("tillerino", "!"+msg.Message.RawText.Substring(1)));
                }
            }

            if (msg.User.Result.ToLower().Equals("tillerino"))
            {
                msg.Cancel = true;
                SyncHost.Instance.ClientWrapper.Client.SendMessage(new IRCMessage(SyncHost.Instance.ClientWrapper.Client.NickName, msg.Message.RawText));
            }
        }
    }
}
