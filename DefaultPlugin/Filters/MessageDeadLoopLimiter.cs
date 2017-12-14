using DefaultPlugin.Clients;
using Sync.MessageFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefaultPlugin.Filters
{
    class MessageDeadLoopLimiter : IFilter, ISourceClient
    {
        public void onMsg(ref IMessageBase msg)
        {
            if(DefaultPlugin.MainClient.Client is DirectOSUIRCBot client &&
                DirectOSUIRCBot.IRCBotName == DirectOSUIRCBot.IRCNick &&
                msg.User.RawText == DirectOSUIRCBot.IRCBotName)
            {
                msg.Cancel = true;
            }
        }
    }
}
