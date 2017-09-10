﻿using Sync.Plugins;
using Sync;
using DefaultPlugin.Sources.BiliBili;
using DefaultPlugin.Sources.Twitch;
using DefaultPlugin.Filters;
using DefaultPlugin.Commands;
using System;
using Sync.Tools;
using System.Threading.Tasks;
using Sync.Source;
using Sync.Client;
using DefaultPlugin.Clients;
using static Sync.Plugins.PluginEvents;
using Sync.MessageFilter;

namespace DefaultPlugin
{

    public class DefaultPlugin : Plugin
    {
        public static MessageDispatcher MainMessager = null;
        public static FilterManager MainFilters = null;
        public static SourceManager MainSources = null;
        public static SourceWorkWrapper MainSource = null;
        public static ClientWorkWrapper MainClient = null;
        public static SendableSource MainSendable = null;
        public static ClientManager MainClients = null;
        private BiliBili srcBili;
        private Twitch srcTwitch;
        private DirectOSUIRCBot clientOSU;
        private DefaultFormat fltFormat;
        private GiftReceivePeeker fltGift;
        private OnlineChangePeeker fltOnline;
        
        public static PluginConfigurationManager Config { get; set; }

        public DefaultPlugin() : base("Default Plug-ins", "Deliay") { }

        public override void OnEnable()
        {
            I18n.Instance.ApplyLanguage(new Language());

            srcBili = new BiliBili();
            srcTwitch = new Twitch();

            base.EventBus.BindEvent<InitCommandEvent>(evt => new BaseCommand(evt.Commands));
            base.EventBus.BindEvent<InitSourceEvent>(evt => {
                evt.Sources.AddSource(srcBili);
                evt.Sources.AddSource(srcTwitch);
            });

            fltFormat = new DefaultFormat();
            fltGift = new GiftReceivePeeker();
            fltOnline = new OnlineChangePeeker();

            base.EventBus.BindEvent<InitFilterEvent>(evt => evt.Filters.AddFilters(fltFormat, fltGift, fltOnline));

            base.EventBus.BindEvent<LoadCompleteEvent>(DefaultPlugin_onLoadComplete);

            clientOSU = new DirectOSUIRCBot();

            base.EventBus.BindEvent<InitClientEvent>(evt => {
                evt.Clients.AddAllClient(clientOSU);
#if (DEBUG)
                evt.Clients.AddClient(new ConsoleReciveSendOnlyClient());
#endif
            });

            IO.CurrentIO.WriteColor("Default Plugin by Deliay", ConsoleColor.DarkCyan);
        }

        private void DefaultPlugin_onLoadComplete(LoadCompleteEvent @event)
        {
            SyncHost host = @event.Host;
            MainFilters = host.Filters;
            MainSources = host.Sources;
            MainSource = host.SourceWrapper;
            MainClient = host.ClientWrapper;
            MainMessager = host.Messages;
            MainClients = host.Clients;

            //config load
            Config = new PluginConfigurationManager(this);
            Config.AddItem(srcBili);
            Config.AddItem(srcTwitch);
            Config.AddItem(clientOSU);
        }
    }
}