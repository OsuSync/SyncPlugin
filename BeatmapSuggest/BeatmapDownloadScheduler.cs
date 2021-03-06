﻿using Sync;
using Sync.MessageFilter;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BeatmapSuggest
{
    public struct BeatmapDownloadTask
    {
        public int id;
        public bool isSetId;
        public string name;
    }

    public class BeatmapDownloadScheduler
    {
        private LinkedList<BeatmapDownloadTask> suggest_history_queue;
        
        string save_path;

        public string api_key { get; set; }

        private string _osu_cookies;
        
        WebClient wc = new WebClient();

        public string osu_cookies { get => _osu_cookies; set{
                _osu_cookies = value;
                wc.Headers[HttpRequestHeader.Cookie] = osu_cookies;
            } }

        int capacity;

        public BeatmapDownloadScheduler(int history_capacity,string api_key,string osu_cookies)
        {
            suggest_history_queue = new LinkedList<BeatmapDownloadTask>();
            capacity = history_capacity;
            this.api_key = api_key;
            this.osu_cookies = osu_cookies;
        }

        public bool CheckDownloadable()
        {
            var error = string.Empty;

            if (suggest_history_queue.Count==0)
                error = $"[BeatmapDownloadScheduler]Queue is empty.";
            else if (string.IsNullOrWhiteSpace(osu_cookies))
                error = $"[BeatmapDownloadScheduler]OsuCookies is empty. you have to set OsuCookies as your cookies of osu!websites in config.ini , also type 'suggest login <OsuAccountName> <OsuAccountPassword>' in Sync for getting cookies automatically,and then restart Sync";
            else if (string.IsNullOrWhiteSpace(api_key))
                error = $"[BeatmapDownloadScheduler]"+DefaultLanguage.LANG_NO_API_KEY_NOFITY;

            if (!string.IsNullOrWhiteSpace(error))
            {
                IO.CurrentIO.WriteColor(error, ConsoleColor.Red);
                SyncHost.Instance.ClientWrapper?.Client?.SendMessage(new IRCMessage(SyncHost.Instance.ClientWrapper?.Client.NickName,error));

                return false;
            }

            return true;
        }

        public void Push(int id,bool isSetId,string name)
        {
            if (capacity<suggest_history_queue.Count)
            {
                suggest_history_queue.RemoveFirst();
            }

            BeatmapDownloadTask task = new BeatmapDownloadTask()
            {
                id=id,
                isSetId=isSetId,
                name=name
            };

            suggest_history_queue.AddLast(task);

            IO.CurrentIO.Write($"[BeatmapSuggest]Push {name}");
        }

        public async void DownloadLastSuggest()
        {
            if (suggest_history_queue.Count==0)
                return;

            BeatmapDownloadTask map = suggest_history_queue.Last();
            suggest_history_queue.RemoveLast();

            SendIRCMessage(string.Format(DefaultLanguage.LANG_START_DOWNLOAD,map.name));

            if (!await DownloadBeatmap(map))
                SendIRCMessage($"Download [{map.id}]{map.name} failed!");
        }

        public async void DownloadAll()
        {
            if (suggest_history_queue.Count == 0)
                return;

            var copy_list = new LinkedList<BeatmapDownloadTask>(suggest_history_queue);
            suggest_history_queue.Clear();

            SendIRCMessage(string.Format(DefaultLanguage.LANG_DOWNLOAD_TASK_COUNT,copy_list.Count));

            int success_count = 0;
            
            foreach (var map in copy_list)
                success_count += await DownloadBeatmap(map)?1:0;

            SendIRCMessage($"Downloaded {success_count}/{copy_list.Count} beatmaps.");
        }

        /// <summary>
        /// 试图获取当前Songs文件夹路径
        /// </summary>
        /// <returns></returns>
        private bool TryGetOsuSongFolder()
        {
            if (string.IsNullOrWhiteSpace(save_path))
            {
                var processes = Process.GetProcessesByName(@"osu!");
                if (processes.Length != 0)
                {
                    string osu_path = processes[0].MainModule.FileName.Replace(@"osu!.exe", string.Empty);

                    string osu_config_file = Path.Combine(osu_path, $"osu!.{Environment.UserName}.cfg");
                    var lines = File.ReadLines(osu_config_file);
                    string song_path;
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("BeatmapDirectory"))
                        {
                            song_path = line.Split('=')[1].Trim();
                            if (Path.IsPathRooted(song_path))
                                save_path = song_path;
                            else
                                save_path = Path.Combine(osu_path, song_path);
                            return true;
                        }
                    }
                }

                return false;
            }

            return true;
        }

        private async Task<bool> DownloadBeatmap(BeatmapDownloadTask map)
        {
            try
            {
                if (!TryGetOsuSongFolder())
                    return false;

                int beatmap_setid = map.id;

                //通过id获取对应的setid
                if (!map.isSetId)
                {
                    beatmap_setid =await GetBeatmapSetID(map.id);

                    if (beatmap_setid < 0)
                        return false;
                }

                IO.CurrentIO.WriteColor(string.Format(DefaultLanguage.LANG_START_DOWNLOAD, map.name), ConsoleColor.Green);

                string download_url = $"https://osu.ppy.sh/d/{beatmap_setid}";

                var file_save_path = Path.Combine(save_path, GetSafePath($"{beatmap_setid} {map.name}.osz"));

                wc.DownloadFile(new Uri(download_url), file_save_path);

                if (IsDownnloadSuccessfully(file_save_path))
                {
                    IO.CurrentIO.WriteColor(string.Format(DefaultLanguage.LANG_FINISH_DOWNLOAD, map.name), ConsoleColor.Green);
                    return true;
                }
                else
                    IO.CurrentIO.WriteColor(string.Format("Cant download beatmap {0}", map.name), ConsoleColor.Green);
            }
            catch (Exception e)
            {
                IO.CurrentIO.WriteColor(string.Format(DefaultLanguage.LANG_FAILED_DOWNLOAD, map.name, e.Message), ConsoleColor.Red);
            }

            return false;
        }

        private string GetSafePath(string raw_path)
        {
            string result = string.Empty;
            var chars = /*Path.GetInvalidPathChars().Concat*/(Path.GetInvalidFileNameChars());

            foreach (var ch in raw_path)
                result += chars.Contains(ch) ? ' ' : ch;

            return result;
        }

        private bool IsDownnloadSuccessfully(string save_path)
        {
            FileInfo info = new FileInfo(save_path);
            return info.Exists&&info.Length>100*1024; // >100kb success
        }

        /// <summary>
        /// 借助osu api,获取beatmapID对应的BeatmapSetID
        /// </summary>
        /// <param name="id">beatmapID</param>
        /// <returns></returns>
        private async Task<int> GetBeatmapSetID(int id)
        {
            string uri = @"https://osu.ppy.sh/api/get_beatmaps?" +
                $@"k={api_key}&b={id}&limit=1";

            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
            request.Method = "GET";
            var response = (HttpWebResponse)await request.GetResponseAsync();
            var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string data = stream.ReadToEnd();

            stream.Close();

            var result = Regex.Match(data, $"\"beatmapset_id\":\"(.+?)\"");

            if (!result.Success)
                return -1;

            return int.Parse(result.Groups[1].Value);
        }

        private void SendIRCMessage(string message)
        {
            Sync.SyncHost.Instance.Messages.RaiseMessage<ISourceClient>(new IRCMessage(string.Empty, message));
        }
    }
}
