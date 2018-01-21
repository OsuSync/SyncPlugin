using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync;
using Sync.Command;
using Sync.Plugins;
using Sync.Tools;
using static RecentlyUserQuery.DefaultLanguage;

namespace RecentlyUserQuery
{
    [SyncPluginID("cc7b2265-e628-4ae6-9af6-6579f818ff0d", "2.16.0")]
    [SyncPluginDependency("c620c63a-32b6-4281-87a9-b7da62be0215", Version = "^2.15.0", Require = true)]
    public class RecentlyMessageQueryPlugin : Plugin
    {
        MessageRecorder recorder = new MessageRecorder();

        public const string PLUGIN_NAME = "Recently Message Query Plugin";
        public const string PLUGIN_AUTHOR = "Dark Projector";
        
        public RecentlyMessageQueryPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
        }

        public override void OnEnable()
        {
            Sync.Tools.IO.CurrentIO.WriteColor(PLUGIN_NAME + " By " + PLUGIN_AUTHOR, ConsoleColor.DarkCyan);
            ///todo
            I18n.Instance.ApplyLanguage(new DefaultLanguage());

            base.EventBus.BindEvent<PluginEvents.InitCommandEvent>(manager => manager.Commands.Dispatch.bind("recently", onProcessCommand, "recently --<command> [arg...] 操作消息记录器相关功能,--help获取相关指令"));
            base.EventBus.BindEvent<PluginEvents.InitFilterEvent>(manager => manager.Filters.AddFilters(new Danmaku.MessageRecorderFilter(recorder)));
            base.EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(host => host.Host.Filters.AddFilters(new Osu.MessageRecorderControlFilter(host.Host.Messages, recorder), new Osu.GetUserIdFilter(host.Host.Messages)));
        }

        private bool onProcessCommand(Arguments args)
        {
            if (args.Count != 0 )
                Sync.Tools.IO.CurrentIO.Write(recorder.ProcessCommonCommand(args).Replace(" || ","\n"));
            else
                Sync.Tools.IO.CurrentIO.WriteColor(LANG_HELP,ConsoleColor.Yellow); 
            return true;
        }

        private void SendResponseMessage(string message)
        {
            Sync.Tools.IO.CurrentIO.Write(message);
        }

        public void onConfigurationLoad()
        {
            throw new NotImplementedException();
        }

        public void onConfigurationSave()
        {
            throw new NotImplementedException();
        }
		
    }

    public class UserIdGenerator
    {
        private UserIdGenerator() { }

        private static Dictionary<string, int> idrecorder = new Dictionary<string, int>();
        private static Dictionary<int, string> userNamerecorder = new Dictionary<int, string>();
        private static int current_id;

        public static int GetId(string userName)
        {
            if (idrecorder.ContainsKey(userName))
                return idrecorder[userName];
            int id = current_id++;
            idrecorder[userName] = id;
            userNamerecorder[id] = userName;
            return id;
        }

        //没有id对应的用户就返回String.Empty
        public static string GetUserName(int id)
        {
            return userNamerecorder.ContainsKey(id) ? userNamerecorder[id] : string.Empty;
        }

        public static void Clear()
        {
            idrecorder.Clear();
            userNamerecorder.Clear();
            current_id = 0;
        }
    }
}
