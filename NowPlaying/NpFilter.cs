using System;
using System.Linq;
using System.Text;
using Sync;
using Sync.MessageFilter;
using Sync.Tools;
using System.IO;
using System.Diagnostics;

namespace NowPlaying
{
    public class NpFilter : IConfigurable, IFilter, ISourceDanmaku
    {
        private OSUStatus osuStat = new OSUStatus();

        object locker = new object();

        public static ConfigurationElement OsuFolderPath { get; set; } = "";
        public static ConfigurationElement EnableAdvanceFeature { get; set; } = "0";
        
        Stopwatch sw = new Stopwatch();

        BeatmapEntry CurrentPlayingBeatmap;

        public NpFilter()
        {
            NowPlayingEvents.Instance.BindEvent<StatusChangeEvent>(OnOSUStatusChange);
        }

        private void InitAdvance()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(OsuFolderPath))
                {
                    IO.CurrentIO.WriteColor($"[NowPlaying]未设置osu文件夹路径，将自行搜寻当前运行中的osu程序来自动设置", ConsoleColor.Yellow);

                    var processes = Process.GetProcessesByName(@"osu!");
                    if (processes.Length != 0)
                    {
                        OsuFolderPath = processes[0].MainModule.FileName.Replace(@"osu!.exe", string.Empty);
                        IO.CurrentIO.WriteColor($"[NowPlaying]Found osu!.exe ,Get osu folder:{OsuFolderPath}", ConsoleColor.Green);
                    }
                    else
                    {
                        IO.CurrentIO.WriteColor($"[NowPlaying]未设置osu文件夹路径，也没运行中的osu程序，无法使用此插件其他高级功能，请设置好路径并重新启动osuSync才能继续使用", ConsoleColor.Red);
                    }
                }
                sw.Stop();
            }
            catch (Exception e)
            {
                IO.CurrentIO.WriteColor($"[NowPlaying]trying support advance features failed!,{e.Message}", ConsoleColor.Yellow);
                OsuFolderPath = string.Empty;
            }
        }

        private void OnOSUStatusChange(StatusChangeEvent @event)
        {
            osuStat = @event.CurrentStatus;
#if (DEBUG)
            Sync.Tools.IO.CurrentIO.WriteColor(osuStat.status + " " + osuStat.artist + " - " + osuStat.title, ConsoleColor.DarkCyan);
#endif
        }

        public void onMsg(ref IMessageBase msg)
        {
            if (!msg.Message.RawText.StartsWith("?np"))
                return;

            msg.Cancel = true;
            string param = msg.Message.RawText.Replace("?np", string.Empty).Trim();
            switch (param)
            {
                case "":
                    SendCurrentStatus();
                    break;

                case "-setid":
                case "-sid":
                    SendCurrentBeatmapSetID();
                    break;

                case "-hp":
                    SendCurrentBeatmapHP();
                    break;

                case "-od":
                    SendCurrentBeatmapOD();
                    break;

                case "-cs":
                    SendCurrentBeatmapCS();
                    break;

                case "-ar":
                    SendCurrentBeatmapAR();
                    break;

                case "-id":
                    SendCurrentBeatmapID();
                    break;

                default:
                    SendRawMessage( $"无效的命令\"{param}\"");
                    break;
            }
        }

        private void SendCurrentStatus()
        {
            string strMsg = string.Empty;
            if (osuStat.Status == "Playing")
            {
                strMsg = "玩";
            }
            else if (osuStat.Status == "Editing")
            {
                strMsg = "做";
            }
            else //include  Listening
            {
                strMsg = "听";
            }
            if (osuStat.Title.Length > 17)
            {
                SendRawMessage( "我在" + strMsg + osuStat.Title.Substring(0, 14) + "...");
            }
            else
            {
                SendRawMessage( "我在" + strMsg + osuStat.Title);
            }
        }

        private void OnOsuStatusAdvanceChange(StatusChangeEvent stat)
        {
            var currentOsuStat = stat.CurrentStatus;

            sw.Reset();
            sw.Start();

            string osu_file_path = string.Empty;
            if (!(/*osuStat.status != "Playing"*/string.IsNullOrWhiteSpace(currentOsuStat.Diff) || string.IsNullOrWhiteSpace(OsuFolderPath)))
            {
                try
                {
                    string folder_query_path = ConvertVaildPath($"*{currentOsuStat.Artist} - {currentOsuStat.Title}*");

                    string file_query_path = ConvertVaildPath($"*[{currentOsuStat.Diff}]")+".osu";

                    var path_query_list = Directory.EnumerateDirectories(OsuFolderPath + "Songs\\", folder_query_path);

                    if (path_query_list.Count() != 0)
                    {
                        foreach (string path in path_query_list)
                        {
                            var files_query_list = Directory.EnumerateFiles(path, file_query_path);
                            if (files_query_list.Count() != 0)
                            {
                                osu_file_path = files_query_list.First();
                                break;
                            }
                        }
                    }


                }
                catch (Exception e)
                {
                    IO.CurrentIO.WriteColor($"[NowPlaying]try to get beatmap \"{currentOsuStat.Artist} - {currentOsuStat.Title} [{currentOsuStat.Diff}]\" failed,Message:{e.Message}",ConsoleColor.Red);
                    osu_file_path = string.Empty;
                }
            }

            BeatmapEntry temp_beatmap = null;

            if (!string.IsNullOrWhiteSpace(osu_file_path))
            {
                try
                {
                    temp_beatmap = OsuFileParser.ParseText(File.ReadAllText(osu_file_path));

                    //Set path as extra data 
                    temp_beatmap.OsuFilePath = osu_file_path;
                }
                catch
                {
                    temp_beatmap = null;
                }
            }
            else
            {
                temp_beatmap = null;
            }

            lock (locker)
            {
                if (temp_beatmap?.OsuFilePath != CurrentPlayingBeatmap?.OsuFilePath)
                {
                    NowPlayingEvents.Instance.RaiseEvent(new CurrentPlayingBeatmapChangedEvent(temp_beatmap));

                    if (temp_beatmap != null)
                        IO.CurrentIO.WriteColor($"[NowPlaying]query files:{osu_file_path},time:{sw.ElapsedMilliseconds}ms,AR/CS/OD/HP:({temp_beatmap.DiffAR}/{temp_beatmap.DiffCS}/{temp_beatmap.DiffOD}/{temp_beatmap.DiffHP})", ConsoleColor.Green);

                    CurrentPlayingBeatmap = temp_beatmap;
                }
            }

            sw.Stop();

            return;
        }

        private static string ConvertVaildPath(string raw_path)
        {
            StringBuilder sb = new StringBuilder(raw_path);
            
            sb.Replace(".", string.Empty);

            foreach (var invaild_char in Path.GetInvalidFileNameChars())
            {
                sb.Replace(invaild_char, '*');
            }

            return sb.ToString();
        }

        public void onConfigurationLoad()
        {
            if (int.TryParse(EnableAdvanceFeature, out int value) && value == 1)
            {
                InitAdvance();
                if (!string.IsNullOrWhiteSpace(OsuFolderPath))
                    NowPlayingEvents.Instance.BindEvent<StatusChangeEvent>(OnOsuStatusAdvanceChange);
            }
        }

        public void onConfigurationSave()
        {

        }

        public void SendCurrentBeatmapSetID() => SendRawMessage (CurrentPlayingBeatmap!= null ? $"当前铺面SetID:{CurrentPlayingBeatmap.BeatmapSetId}" : $"咕咕咕,当前并没打任何图");

        public void SendCurrentBeatmapID() => SendRawMessage(CurrentPlayingBeatmap != null ? $"当前铺面ID:{CurrentPlayingBeatmap.BeatmapId}" : $"咕咕咕,当前并没打任何图");

        public void SendCurrentBeatmapAR() => SendRawMessage(CurrentPlayingBeatmap != null ? $"当前铺面AR:{CurrentPlayingBeatmap.DiffAR}" : $"咕咕咕,当前并没打任何图");

        public void SendCurrentBeatmapHP() => SendRawMessage(CurrentPlayingBeatmap != null ? $"当前铺面HP:{CurrentPlayingBeatmap.DiffHP}" : $"咕咕咕,当前并没打任何图");

        public void SendCurrentBeatmapCS() => SendRawMessage(CurrentPlayingBeatmap != null ? $"当前铺面CS:{CurrentPlayingBeatmap.DiffCS}" : $"咕咕咕,当前并没打任何图");

        public void SendCurrentBeatmapOD() => SendRawMessage(CurrentPlayingBeatmap != null ? $"当前铺面OD:{CurrentPlayingBeatmap.DiffOD}" : $"咕咕咕,当前并没打任何图");

        private void raiseCurrentStatus()
        {
            string strMsg = string.Empty;
            if (osuStat.Status == "Playing")
            {
                strMsg = "玩";
            }
            else if (osuStat.Status == "Editing")
            {
                strMsg = "做";
            }
            else //include  Listening
            {
                strMsg = "听";
            }
            if (osuStat.Title.Length > 17)
            {
                SendRawMessage("我在" + strMsg + osuStat.Title.Substring(0, 14) + "...");
            }
            else
            {
                SendRawMessage("我在" + strMsg + osuStat.Title);
            }
        }
        
        public static void SendRawMessage(string message)
        {
            SyncHost.Instance.SourceWrapper.SendableSource.Send(new IRCMessage(string.Empty, message));
        }

    }
}
