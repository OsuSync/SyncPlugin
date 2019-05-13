using Sync.MessageFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanManagerPlugin.Ban
{
    [FilterPriority(Priority = FilterPriority.Highest)]
    public class BanClientFilter : IFilter, ISourceDanmaku
    {
        BanManager manager = null;

        public BanClientFilter(BanManager refManager)
        {
            manager = refManager;
        }

        public void onMsg(ref IMessageBase msg)
        {

            if (manager.Info.IsBanned(msg.User.RawText))
            {
#if DEBUG
                Log.Debug(msg.User.RawText+" was banned.");
#endif
                msg.Cancel = true;
            }
        }
    }
}
