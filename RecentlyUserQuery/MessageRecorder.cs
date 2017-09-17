using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.Tools;
using static RecentlyUserQuery.DefaultLanguage;
using Sync.Command;

namespace RecentlyUserQuery
{
    public struct Record
    {
        public Record(int id,string userName,string message)
        {
            this.id = id;
            this.userName = userName;
            this.message = message;
        }

        public int id;
        public string userName, message;
    }

    public class MessageRecorder
    {
        private List<Record> historyList = new List<Record>();
   
        private int capacity = 15;

        public int Capacity
        {
            get { return capacity; }
            set
            {
                capacity = value<0?0:value;
                ChangeUpdate();
            }
        }

        private bool isRecording = true;

        public bool IsRecording
        {
            get { return isRecording; }
            set { isRecording = value; }
        }
        
        public List<Record> GetHistoryList()
        {
            return historyList;
        }

        private void ChangeUpdate()
        {
            while (Capacity < historyList.Count)
                historyList.RemoveAt(0);
        }

        public void Update(string userName,string message)
        {
            historyList.Add(new Record(UserIdGenerator.GetId(userName), userName, message));
            ChangeUpdate();
        }

        public void Clear()
        {
            historyList.Clear();
        }

        public string ProcessCommonCommand(Arguments args)
        {
            int value = 0;

            switch (args[0])
            {
                case "--status":
                    return string.Format(LANG_MSG_STATUS, (string)(IsRecording ? LANG_RUNNING : LANG_STOP), GetHistoryList().Count, Capacity);

                case "--disable":
                    IsRecording = false;
                    Clear();
                    UserIdGenerator.Clear();
                    return LANG_MSG_DISABLE;

                case "--start":
                    IsRecording = true;
                    return LANG_MSG_START;

                case "--realloc":
                    if (args.Count < 2)
                        return LANG_MSG_REALLOC_ERR;
                    else
                    {
                        int.TryParse(args[1], out value);
                        Capacity = value;
                        return string.Format(LANG_MSG_REALLOC,Capacity);
                    }

                case "--i": //鸽一会
                    return LANG_MSG_NOTIMPLENT;

                case "--u": //鸽一会
                    return LANG_MSG_NOTIMPLENT;

                case "--recently":
                    return (EnumRecentUser().Result);

                default:
                    return LANG_MSG_UNKNOWNCOMMAND;
            }
        }

        private async Task<string> EnumRecentUser()
        {
            var task = new Task<string>(() =>
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                foreach (var record in GetHistoryList())
                    result[record.userName] = (record.id);
                StringBuilder sb = new StringBuilder();
                foreach (var pair in result)
                    sb.AppendFormat("{0}->{1} || ", pair.Value, pair.Key);

                return sb.ToString();
            });
            task.Start();
            return await task;
        }
    }
}
