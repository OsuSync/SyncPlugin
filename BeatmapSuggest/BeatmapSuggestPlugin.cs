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
using Sync.Tools.ConfigurationAttribute;

namespace BeatmapSuggest
{
    [SyncPluginID("916fe745-7a33-4c20-b985-58c4810e261e", "2.15.0")]
    public class BeatmapSuggestPlugin : Plugin,IConfigurable
    {
        [Bool]
        public ConfigurationElement EnableInsoMirrorLink { get; set; } = "False";

        public ConfigurationElement OsuApiKey { get; set; } = string.Empty;

        [Integer(MinValue = 1,MaxValue = 100)]
        public ConfigurationElement SuggestHistoryCapacity { get; set; } = "10";

        private Danmaku.BeatmapSuggestFilter danmaku_filter;
        private Osu.BeatmapSuggestDownloadFilter osu_filter;

        private PluginConfigurationManager config_manager;

        public BeatmapSuggestPlugin() : base("Beatmap Suggest Command", "Dark Projector")
        {
            I18n.Instance.ApplyLanguage(new DefaultLanguage());

            danmaku_filter = new Danmaku.BeatmapSuggestFilter();
            osu_filter = new Osu.BeatmapSuggestDownloadFilter();

            base.EventBus.BindEvent<PluginEvents.InitFilterEvent>(manager => {
                manager.Filters.AddFilter(this.danmaku_filter);
                manager.Filters.AddFilter(this.osu_filter);
            });

            base.EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(host => this.danmaku_filter.SetFilterManager(host.Host.Messages));
            Sync.Tools.IO.CurrentIO.WriteColor(Name + " By " + Author, ConsoleColor.DarkCyan);

            config_manager = new PluginConfigurationManager(this);
            config_manager.AddItem(this);
        }

        public override void OnEnable()
        {
            InitPlugin();
        }

        private void InitPlugin()
        {
            if (string.IsNullOrWhiteSpace(OsuApiKey))
            {
                IO.CurrentIO.WriteColor("[BeatmapSuggestPlugin]"+DefaultLanguage.LANG_NO_API_KEY_NOFITY, ConsoleColor.Red);
                return;
            }

            BeatmapDownloadScheduler scheduler = new BeatmapDownloadScheduler(int.Parse(SuggestHistoryCapacity), OsuApiKey);

            danmaku_filter.Scheduler = scheduler;
            danmaku_filter.OsuApiKey = OsuApiKey;
            danmaku_filter.EnableInsoMirrorLink = bool.Parse(EnableInsoMirrorLink);

            osu_filter.Scheduler = scheduler;
        }

        public void onConfigurationLoad()
        {

        }

        public void onConfigurationSave()
        {

        }

        public void onConfigurationReload()
        {
            InitPlugin();
        }
    }
}
