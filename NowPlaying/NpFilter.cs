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
                    IO.CurrentIO.WriteColor(Languages.OSU_PATH_NOT_SET, ConsoleColor.Yellow);

                    var processes = Process.GetProcessesByName(@"osu!");
                    if (processes.Length != 0)
                    {
                        OsuFolderPath = processes[0].MainModule.FileName.Replace(@"osu!.exe", string.Empty);
                        IO.CurrentIO.WriteColor(string.Format(Languages.FIND_OSU_PATH, OsuFolderPath), ConsoleColor.Green);
                    }
                    else
                    {
                        IO.CurrentIO.WriteColor(Languages.OSU_PATH_FAIL, ConsoleColor.Red);
                    }
                }
                sw.Stop();
            }
            catch (Exception e)
            {
                IO.CurrentIO.WriteColor(string.Format(Languages.ERROR_WHILE_FIND_PATH, e.Message), ConsoleColor.Yellow);
                OsuFolderPath = string.Empty;
            }
        }

        private void OnOSUStatusChange(StatusChangeEvent @event)
        {
            osuStat = @event.CurrentStatus;
#if (DEBUG)
            Sync.Tools.IO.CurrentIO.WriteColor(osuStat.Status + " " + osuStat.Artist + " - " + osuStat.Title, ConsoleColor.DarkCyan);
#endif
        }

        public void onMsg(ref IMessageBase msg)
        {
            if (!msg.Message.RawText.StartsWith("?np"))
                return;

            msg.Cancel = true;
            string param = msg.Message.RawText.Substring(3).TrimStart('-');
            object value = 0;

            switch (param)
            {
                case "":
                    SendCurrentStatus();
                    return;

                case "setid":
                case "sid":
                    value = CurrentPlayingBeatmap.BeatmapSetId;
                    break;

                case "hp":
                    value = CurrentPlayingBeatmap.DiffHP;
                    break;

                case "od":
                    value = CurrentPlayingBeatmap.DiffOD;
                    break;

                case "cs":
                    value = CurrentPlayingBeatmap.DiffCS;
                    break;

                case "ar":
                    value = CurrentPlayingBeatmap.DiffAR;
                    break;

                case "id":
                    value = CurrentPlayingBeatmap.BeatmapId;
                    break;

                default:
                    SendRawMessage(string.Format(Languages.UNKNOWN_COMMAND, param));
                    break;
            }

            SendStatusMessage(param, value);
        }

        private void SendCurrentStatus()
        {
            string strMsg = string.Empty;
            if (osuStat.Status == "Playing")
            {
                strMsg = Languages.STATUS_PLAYING;
            }
            else if (osuStat.Status == "Editing")
            {
                strMsg = Languages.STATUS_EDITING;
            }
            else //include  Listening
            {
                strMsg = Languages.STATUS_OTHER;
            }
            if (osuStat.Title.Length > 17)
            {
                SendRawMessage(string.Format(Languages.STATUS_TIP_INFO_WRAP, strMsg, osuStat.Title.Substring(0, 14) + "..."));
            }
            else
            {
                SendRawMessage(string.Format(Languages.STATUS_TIP_INFO, strMsg + osuStat.Title));
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
                    IO.CurrentIO.WriteColor(string.Format(Languages.ERROR_WHILE_SEARCH_MAP, currentOsuStat.Artist, currentOsuStat.Title, currentOsuStat.Diff, e.Message), ConsoleColor.Red);
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
                        IO.CurrentIO.WriteColor(string.Format(Languages.CONSOLE_OUTPUT_RESULT, osu_file_path, sw.ElapsedMilliseconds, temp_beatmap.DiffHP, temp_beatmap.DiffCS, temp_beatmap.DiffAR, temp_beatmap.DiffOD), ConsoleColor.Green);

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
        
        public static void SendStatusMessage(string key, object value)
        {
            SendRawMessage(string.Format(Languages.OUTPUT_RESULT, key, value));
        }

        public static void SendRawMessage(string message)
        {
            SyncHost.Instance.SourceWrapper.SendableSource.Send(new IRCMessage(string.Empty, message));
        }

    }
}
