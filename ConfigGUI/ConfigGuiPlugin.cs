using ConfigGUI.ConfigurationRegion;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ConfigGUI
{
    [SyncPluginID("4eaf2dca-1d49-4f0c-b9b7-c220db5feab0", PLGUIN_VERSION)]
    public class ConfigGuiPlugin : Plugin
    {
        public const string PLUGIN_NAME="ConfigGUI";
        public const string PLUGIN_AUTHOR = "KedamaOvO";
        public const string PLGUIN_VERSION = "0.1.4";

        public ConfigurationItemFactory ItemFactory { get; } = new ConfigurationItemFactory();

        public ConfigGuiPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
        }

        public override void OnEnable()
        {
            I18n.Instance.ApplyLanguage(new DefaultLanguage());
            ConfigWindow window=null;

            base.EventBus.BindEvent<PluginEvents.InitCommandEvent>((t) =>
            {
                t.Commands.Dispatch.bind("config", args =>
                {
                    if (Application.Current == null)
                    {
                        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
                        var thread = new Thread(() =>
                        {
                            var app = new Application();
                            app.Startup += (s,e)=>completionSource.SetResult(true);
                            app.Run();
                        });
                        thread.Name = "STA WPF Application Thread";
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        completionSource.Task.Wait();
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        window = (window ?? new ConfigWindow(ItemFactory));
                        if (window.Visibility == Visibility.Visible)
                            window.Activate();
                        else
                            window.Show();
                    });

                    return true;
                 }, "show config window");
            });
        }
    }
}
