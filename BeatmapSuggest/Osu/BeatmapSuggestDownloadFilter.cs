using Sync.MessageFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatmapSuggest.Osu
{
    class BeatmapSuggestDownloadFilter : IFilter, ISourceClient
    {
        private const string DownloadCommand = "?dl";

        internal BeatmapDownloadScheduler Scheduler { get; set; }

        public void onMsg(ref IMessageBase msg)
        {
            if ((!msg.Message.RawText.StartsWith(DownloadCommand))||Scheduler == null)
                return;

            msg.Cancel = true;

            string param = msg.Message.RawText.Replace(DownloadCommand, string.Empty).Trim();

            switch (param)
            {
                case "":
                case "last":
                    Scheduler.DownloadLastSuggest();
                    break;
                case "all":
                    Scheduler.DownloadAll();
                    break;
                default:
                    break;
            }
        }
    }
}
