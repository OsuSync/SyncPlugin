using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.Plugins;

namespace BanManagerPlugin.Ban
{
    public class BanManager
    {
        public BanClientFilter ClientFilter { get; private set; } = null;
        public BanServerFilter ServerFliter { get; private set; } = null;
        public FilterManager FilterManager { get; private set; } = null;

        public MessageDispatcher MessageSender { get; set; } = null;
        public BanInfo Info { get; set; } = null;

        public BanManager(string save_path,FilterManager manager)
        {
            Info = LoadBanInfo(save_path);
            FilterManager = manager;
            ClientFilter = new BanClientFilter(this);
            ServerFliter = new BanServerFilter(this);
        }

        public static BanInfo LoadBanInfo(string save_path)
        {
            try
            {
                var content = File.ReadAllText(save_path);
                return BanInfo.LoadFromJSON(content);
            }
            catch (Exception e)
            {
                Log.Error("Can't load baninfo save file,program will backup old and create default new.Please check your file if is vaild.:" + e.Message);

                if (File.Exists(save_path))
                    File.Move(save_path, save_path + ".back");

                var info = new BanInfo();
                var content = info.SaveAsFormattedString();
                File.WriteAllText(save_path, content);

                return info;
            }
        }
    }
}
