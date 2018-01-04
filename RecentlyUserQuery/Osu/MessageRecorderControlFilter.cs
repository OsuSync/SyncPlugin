using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync;
using Sync.MessageFilter;
using Sync.Tools;
using Sync.Source;
using Sync.Plugins;
using Sync.Command;

namespace RecentlyUserQuery.Osu
{
    class MessageRecorderControlFilter : IFilter, ISourceClient
    {
        MessageRecorder recorder = null;
        MessageDispatcher manager = null;

        object locker=new object();

        public MessageRecorderControlFilter(MessageDispatcher manager,MessageRecorder recorder)
        {
            this.recorder = recorder;
            this.manager = manager;
        }

        const string recentlyCommand = "?recently";

        public void onMsg(ref IMessageBase msg)
        {
            string message = msg.Message.RawText;

            if (recorder.IsRecording == false|| !message.StartsWith(recentlyCommand))
                return;

            Arguments args = message.Split(' ');

            msg.Cancel = true;

            if (args.Count > 1)
            {
                args.RemoveAt(0);
                SendResponseMessage(recorder.ProcessCommonCommand(args));
            }
            else
                SendEnumRecentUser();
        }

        private void SendEnumRecentUser()
        {
            Task.Run(() =>
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                foreach (var record in recorder.GetHistoryList())
                    result[record.userName] = (record.id);
                StringBuilder sb = new StringBuilder();
                foreach (var pair in result)
                    sb.AppendFormat("{0}->{1} || ", pair.Value, pair.Key);
                SendResponseMessage(sb.ToString());
            });
        }

        private void SendResponseMessage(string message)
        {
            lock (locker)
            {
                Sync.SyncHost.Instance.Messages.RaiseMessage<ISourceClient>(new IRCMessage("RecentQuery", message));
            }
        }

        public void Dispose()
        {
            //nothing to do
        }
    }
}
