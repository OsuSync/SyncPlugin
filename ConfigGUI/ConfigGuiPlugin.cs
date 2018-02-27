using Sync.Plugins;
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
        ConfigApplication application;

        public ConfigGuiPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
        }

        public override void OnEnable()
        {
            base.EventBus.BindEvent<PluginEvents.InitCommandEvent>((t) =>
            {
                t.Commands.Dispatch.bind("config", args =>
                 {
                     application.ShowWindow();
                     return true;
                 }, "show config window");
            });

            base.EventBus.BindEvent<PluginEvents.ProgramReadyEvent>((e) =>
            {
                var thread = new Thread(InitWindow);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            });
        }

        [STAThread]
        private void InitWindow()
        {
            application = new ConfigApplication();
            application.Run();
        }
    }

    class ConfigApplication:Application
    {
        private ConfigWindow configWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            configWindow = new ConfigWindow();
        }

        public void ShowWindow()
        {
            configWindow.Dispatcher.Invoke(() =>
            {
                configWindow.Show();
            });
        }
    }
}
