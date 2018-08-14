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
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace BeatmapSuggest
{
    [SyncPluginID("916fe745-7a33-4c20-b985-58c4810e261e", "2.15.0")]
    public class BeatmapSuggestPlugin : Plugin,IConfigurable
    {
        [Bool]
        public ConfigurationElement EnableInsoMirrorLink { get; set; } = "False";

        public ConfigurationElement OsuApiKey { get; set; } = string.Empty;

        public ConfigurationElement OsuCookies { get; set; } = string.Empty;

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

            EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(host => this.danmaku_filter.SetFilterManager(host.Host.Messages));
            Sync.Tools.IO.CurrentIO.WriteColor(Name + " By " + Author, ConsoleColor.DarkCyan);

            EventBus.BindEvent<PluginEvents.InitCommandEvent>(OnInitCommand);

            config_manager = new PluginConfigurationManager(this);
            config_manager.AddItem(this);
        }

        public override void OnEnable()
        {
            InitPlugin();
        }

        #region Commands
        
        private void OnInitCommand(PluginEvents.InitCommandEvent e)
        {
            e.Commands.Dispatch.bind("suggest",OnCommand, "BeatmapSuggestPlugin's commands,add arg '-help' for help.");
        }

        private void ShowHelp()
        {

        }

        private bool OnCommand(Arguments args)
        {
            if (args.Count==0)
            {
                ShowHelp();
                return true;
            }

            switch (args[0].Trim())
            {
                case "login":
                    var result=Regex.Matches(string.Join(" ",args), @"\s'(\w+)'");
                    if (result.Count != 2)
                    {
                        IO.CurrentIO.WriteColor($"[BeatmapSuggestPlugin]Wrong login command.Please type login command correctlly like \"suggest login 'your_osu_id' 'your_osu_password'\"", ConsoleColor.Red);
                        return false;
                    }
                    else
                    {
                        var account = result[0].Groups[1].Value;
                        var password = result[1].Groups[1].Value;

                        if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
                        {
                            IO.CurrentIO.WriteColor($"[BeatmapSuggestPlugin]Wrong login command.Please type login command correctlly like \"suggest login 'your_osu_id' 'your_osu_password'\"", ConsoleColor.Red);
                            return false;
                        }

                        GetCookiesFromLogin(account, password);
                    }

                    break;

                case "set":
                    if (args.Count<3)
                    {
                        IO.CurrentIO.WriteColor($"[BeatmapSuggestPlugin]nothing to set.", ConsoleColor.Red);
                        return false;
                    }

                    switch (args[1].Trim())
                    {
                        case "api":
                            OsuApiKey = args[2];
                            break;

                        case "cookies":
                            OsuCookies=string.Concat(args.Skip(2));
                            break;

                        default:
                            IO.CurrentIO.WriteColor($"[BeatmapSuggestPlugin]unknown name to set.", ConsoleColor.Red);
                            return false;
                    }

                    break;

                default:
                    IO.CurrentIO.WriteColor($"[BeatmapSuggestPlugin]Unknown arg \"{args[0]}\"", ConsoleColor.Yellow);
                    return false;
            }

            return true;
        }

        #endregion

        private async void GetCookiesFromLogin(string account,string password)
        {
            var cookie_container = new CookieContainer();

            //build request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://osu.ppy.sh/forum/ucp.php?mode=login");
            request.ContentType = "application/x-www-form-urlencoded";
            request.AllowAutoRedirect = true;
            request.CookieContainer = cookie_container;
            request.Method = "POST";
            string param = "&password=" + password;
            param += "&redirect= /";
            param += "&sid= ";
            param += "&login=login";
            param += "&username=" + account;
            byte[] param_byte = Encoding.UTF8.GetBytes(param);
            Stream stream = request.GetRequestStream();
            stream.Write(param_byte, 0, param_byte.Length);
            stream.Close();


            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            var cookie_collection = cookie_container.GetCookies(new Uri("http://osu.ppy.sh"));
            
            //build cookies string
            StringBuilder sb = new StringBuilder();
            foreach (Cookie cookie in cookie_collection)
                sb.Append($"{cookie.Name}={cookie.Value};");

            IO.CurrentIO.WriteColor($"[BeatmapSuggestPlugin]Got {cookie_collection.Count} cookies",ConsoleColor.Green);

            OsuCookies = sb.ToString();
            if (osu_filter.Scheduler!=null)
                osu_filter.Scheduler.osu_cookies = OsuCookies;
                
        }

        private void InitPlugin()
        {
            if (string.IsNullOrWhiteSpace(OsuApiKey))
            {
                IO.CurrentIO.WriteColor("[BeatmapSuggestPlugin]"+DefaultLanguage.LANG_NO_API_KEY_NOFITY, ConsoleColor.Red);
                return;
            }

            BeatmapDownloadScheduler scheduler = new BeatmapDownloadScheduler(int.Parse(SuggestHistoryCapacity), OsuApiKey, OsuCookies);

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
