using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sync.MessageFilter;
using System.Threading.Tasks;
using Sync.Plugins;
using Sync;
using Sync.Command;
using Sync.Tools;

namespace BeatmapSuggest
{
    [SyncPluginID("916fe745-7a33-4c20-b985-58c4810e261e", "2.15.0")]
    public class BeatmapSuggestPlugin : Plugin
    {
        private Danmaku.BeatmapSuggestFilter filter;

        private PluginConfigurationManager config_manager;

        public BeatmapSuggestPlugin() : base("Beatmap Suggest Command", "Dark Projector")
        {
            filter = new Danmaku.BeatmapSuggestFilter();

            base.EventBus.BindEvent<PluginEvents.InitFilterEvent>(manager => manager.Filters.AddFilter(this.filter));
            base.EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(host => this.filter.SetFilterManager(host.Host.Messages));
            Sync.Tools.IO.CurrentIO.WriteColor(Name + " By " + Author, ConsoleColor.DarkCyan);

            config_manager = new PluginConfigurationManager(this);
            config_manager.AddItem(filter);
        }

        public override void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(filter.OsuApiKey))
            {
                IO.CurrentIO.WriteColor("[BeatmapSuggestPlugin]没有ApiKey,请用户自己提供ApiKey以便使用谱面推荐功能.ApiKey申请地址:https://osu.ppy.sh/p/api", ConsoleColor.Red);
            }
        }
    }
}
