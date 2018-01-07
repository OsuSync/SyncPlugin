using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sync.MessageFilter;
using System.Threading.Tasks;
using Sync.Plugins;
using Sync;
using Sync.Command;

namespace BeatmapSuggest
{
    [SyncPluginID("916fe745-7a33-4c20-b985-58c4810e261e")]
    public class BeatmapSuggestPlugin : Plugin
    {
        private Danmaku.BeatmapSuggestFilter filter = new Danmaku.BeatmapSuggestFilter();

        public BeatmapSuggestPlugin() : base("Beatmap Suggest Command", "Dark Projector")
        {
        }

        public override void OnEnable()
        {
            Sync.Tools.IO.CurrentIO.WriteColor(Name + " By " + Author, ConsoleColor.DarkCyan);
            base.EventBus.BindEvent<PluginEvents.InitFilterEvent>(manager => manager.Filters.AddFilter(this.filter));
            base.EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(host => this.filter.SetFilterManager(host.Host.Messages));
        }
    }
}
