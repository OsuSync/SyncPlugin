using Sync.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sync;
using Sync.Command;
using BanManagerPlugin.Ban;

namespace BanManagerPlugin
{
    [SyncPluginID("b08211ce-2899-4153-aba8-74dca477eba5", "2.16.0")]
    [SyncPluginDependency("c620c63a-32b6-4281-87a9-b7da62be0215", Version = "^2.15.0", Require = true)]
    public class BanManagerPlugin : Plugin
    {
        BanManager banManager = null;

        public BanManagerPlugin() : base("Ban Manager", "Dark Projector")
        {
        }

        public override void OnEnable()
        {
            Sync.Tools.IO.CurrentIO.WriteColor(Name + " By " + Author, ConsoleColor.DarkCyan);

            Sync.Tools.I18n.Instance.ApplyLanguage(new DefaultLanguage());

            base.EventBus.BindEvent<PluginEvents.InitFilterEvent>(manager => {
                banManager = new BanManager(manager.Filters, null);
                manager.Filters.AddFilters(banManager.GetClientFliter(), banManager.GetServerFliter());
            });

            base.EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(host => banManager.SetMessageDispatcher(host.Host.Messages));

        }
    }
}
