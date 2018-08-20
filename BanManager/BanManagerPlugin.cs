using Sync.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sync;
using Sync.Command;
using BanManagerPlugin.Ban;
using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;
using System.IO;

namespace BanManagerPlugin
{
    [SyncPluginID("b08211ce-2899-4153-aba8-74dca477eba5", "2.16.0")]
    [SyncPluginDependency("c620c63a-32b6-4281-87a9-b7da62be0215", Version = "^2.15.0", Require = true)]
    public class BanManagerPlugin : Plugin,IConfigurable
    {
        public const string PLUGIN_NAME = "Ban Manager", PLUGIN_AUTHOR= "Dark Projector";

        BanManager banManager = null;

        PluginConfigurationManager config_manager;

        [Path(IsDirectory = false, RequireExist = false)]
        public ConfigurationElement SavePath { get; set; } = @"../ban_save.json";
        
        [Bool]
        public ConfigurationElement DebugMode { get; set; } = @"False";

        public BanManagerPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
            IO.CurrentIO.WriteColor(Name + " By " + Author, ConsoleColor.DarkCyan);

            I18n.Instance.ApplyLanguage(new DefaultLanguage());

            config_manager = new PluginConfigurationManager(this);
            config_manager.AddItem(this);

            base.EventBus.BindEvent<PluginEvents.InitFilterEvent>(manager => {
                banManager = new BanManager(SavePath,manager.Filters);
                manager.Filters.AddFilters(banManager.ClientFilter, banManager.ServerFliter);
            });

            base.EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(host => banManager.MessageSender=host.Host.Messages);
        }

        private void LoadSaveFile()
        {
            if (banManager!=null)
                banManager.Info = BanManager.LoadBanInfo(SavePath);
        }

        public void onConfigurationLoad()
        {
            LoadSaveFile();
            Log.IsDebug = DebugMode.ToBool();
        }

        public void onConfigurationReload()
        {
            LoadSaveFile();
            Log.IsDebug = DebugMode.ToBool();
        }

        public void onConfigurationSave()
        {
            try
            {
                var content = banManager.Info.SaveAsFormattedString();
                File.WriteAllText(SavePath, content);
            }
            catch (Exception e)
            {
                Log.Error("Save baninfo file failed!:" + e.Message);
            }
        }
    }
}
