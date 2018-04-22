using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ConfigGUI
{
    public class ConfigGuiPlugin : Plugin
    {
        public const string PLUGIN_NAME="ConfigGUI";
        public const string PLUGIN_AUTHOR = "KedamaOvO";
        public const string PLGUIN_VERSION = "0.0.1";

        public ConfigGuiPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
            var thread = new Thread(() =>
            {
                if (Application.Current == null)
                    new Application().Run();
            });
            thread.Name = "STA WPF Application Thread";
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public override void OnEnable()
        {
            I18n.Instance.ApplyLanguage(new DefaultLanguage());

            base.EventBus.BindEvent<PluginEvents.InitCommandEvent>((t) =>
            {
                t.Commands.Dispatch.bind("config", args =>
                 {
                     Application.Current.Dispatcher.Invoke(()=>new ConfigWindow().Show());
                     return true;
                 }, "show config window");
            });
        }
    }
}
