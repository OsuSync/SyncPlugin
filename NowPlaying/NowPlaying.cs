using Sync.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sync;
using Sync.Command;
using Sync.MessageFilter;
using System.Threading.Tasks;
using Sync.Tools;
using System.IO;
using System.Diagnostics;
using System.Threading;
using static Sync.Plugins.PluginEvents;

namespace NowPlaying
{
    [SyncPluginID("b88d23bd-50bc-4b3d-9a81-9ba559c05aab", "2.16.0")]
    public class NowPlaying : Plugin
    {
        private MessageDispatcher MainMessager = null;
        private MSNHandler handler = null;
        private NpFilter fltNp = null;
        PluginConfigurationManager config;

        public NowPlaying() : base("Now Playing", "Deliay, DarkProjector")
        {
        }

        public override void OnEnable()
        {
            I18n.Instance.ApplyLanguage(new Languages());

            EventBus.BindEvent<LoadCompleteEvent>(evt => MainMessager = evt.Host.Messages);

            handler = new MSNHandler();
            handler.Load();
            handler.StartHandler();
            fltNp = new NpFilter();
            EventBus.BindEvent<InitFilterEvent>((filter) => filter.Filters.AddFilter(fltNp));

            config = new PluginConfigurationManager(this);
            config.AddItem(fltNp);
            IO.CurrentIO.WriteColor(Name + " By " + Author, ConsoleColor.DarkCyan);
        }
    }
}
