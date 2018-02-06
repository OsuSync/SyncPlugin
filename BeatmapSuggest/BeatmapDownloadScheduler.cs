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

        string api_key;

        int capacity;

        public BeatmapDownloadScheduler(int history_capacity,string api_key)
        {
            suggest_history_queue = new LinkedList<BeatmapDownloadTask>();
            capacity = history_capacity;
            this.api_key = api_key;
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

        public void DownloadLastSuggest()
        {
            if (suggest_history_queue.Count==0)
                return;

            BeatmapDownloadTask map = suggest_history_queue.Last();
            suggest_history_queue.RemoveLast();

            SendIRCMessage(string.Format(DefaultLanguage.LANG_START_DOWNLOAD,map.name));

            DownloadBeatmap(map);
        }

        public void DownloadAll()
        {
            if (suggest_history_queue.Count == 0)
                return;

            var copy_list = new LinkedList<BeatmapDownloadTask>(suggest_history_queue);
            suggest_history_queue.Clear();

            SendIRCMessage(string.Format(DefaultLanguage.LANG_DOWNLOAD_TASK_COUNT,copy_list.Count));

            foreach (var map in copy_list)
            {
                DownloadBeatmap(map);
            }
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

        private async void DownloadBeatmap(BeatmapDownloadTask map)
        {
            await Task.Run(() => 
            {
                try
                {
                    if (!TryGetOsuSongFolder())
                    {
                        return;
                    }

                    int beatmap_setid=map.id;

                    //通过id获取对应的setid
                    if (!map.isSetId)
                    {
                        beatmap_setid = GetBeatmapSetID(map.id);
                        if (beatmap_setid<0)
                        {
                            return;
                        }
                    }

                    IO.CurrentIO.WriteColor(string.Format(DefaultLanguage.LANG_START_DOWNLOAD, map.name), ConsoleColor.Green);
                    
                    string download_url = $"http://osu.uu.gl/s/{beatmap_setid}";

                    WebClient wc = new WebClient();
                    wc.DownloadFile(new Uri(download_url), save_path + "\\" + $"{beatmap_setid} {map.name}.osz");

                    IO.CurrentIO.WriteColor(string.Format(DefaultLanguage.LANG_FINISH_DOWNLOAD, map.name), ConsoleColor.Green);
                }
                catch (Exception e)
                {
                    IO.CurrentIO.WriteColor(string.Format(DefaultLanguage.LANG_FAILED_DOWNLOAD, map.name,e.Message), ConsoleColor.Red);
                }
            });
        }

        /// <summary>
        /// 借助osu api,获取beatmapID对应的BeatmapSetID
        /// </summary>
        /// <param name="id">beatmapID</param>
        /// <returns></returns>
        private int GetBeatmapSetID(int id)
        {
            string uri = @"https://osu.ppy.sh/api/get_beatmaps?" +
                $@"k={api_key}&b={id}&limit=1";

            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
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
