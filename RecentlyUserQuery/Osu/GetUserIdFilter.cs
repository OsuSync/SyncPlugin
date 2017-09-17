using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.MessageFilter;
using Sync.Plugins;
using Sync.Source;

namespace RecentlyUserQuery.Osu
{
    class GetUserIdFilter : IFilter,ISourceClient
    {
        MessageDispatcher messageSender = null;

        public GetUserIdFilter(MessageDispatcher messageSender)
        {
            this.messageSender = messageSender;
        }

        const string queryUserIdCommand= "?userid",queryUserNameCommand="?username";

        public void onMsg(ref IMessageBase msg)
        {
            string message = msg.Message.RawText, param = string.Empty;

            if (message.StartsWith(queryUserIdCommand))
            {
                param = message.Substring(queryUserIdCommand.Length).Trim();
                
                string result= String.Format("userid \"{0}\" is {1} ", param, (UserIdGenerator.GetId(param)));
                
                Sync.SyncHost.Instance.Messages.RaiseMessage<ISourceClient>(new IRCMessage("RecentQuery", result));
                msg.Cancel = true;
                return;
            }

            if (message.StartsWith(queryUserNameCommand))
            {
                msg.Cancel = true;
                param = message.Substring(queryUserNameCommand.Length).Trim();

                if (Int32.TryParse(param, out int id))
                    return;

                string result= String.Format("userName \"{0}\" is {1} ", UserIdGenerator.GetUserName(id), param);

                Sync.SyncHost.Instance.Messages.RaiseMessage<ISourceClient>(new IRCMessage("RecentQuery", result));
            }
        }

        public void Dispose()
        {
            //nothing to do
        }
    }
}
