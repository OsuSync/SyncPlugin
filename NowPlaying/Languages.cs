using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NowPlaying
{
    public class Languages : I18nProvider
    {
        public static LanguageElement OSU_PATH_NOT_SET = "未设置osu文件夹路径，将自行搜寻当前运行中的osu程序来自动设置";
        public static LanguageElement FIND_OSU_PATH = "找到osu路径! {0:S}";
        public static LanguageElement OSU_PATH_FAIL = "未设置osu文件夹路径，也没运行中的osu程序，无法使用此插件其他高级功能，请设置好路径并重新启动osuSync才能继续使用";
        public static LanguageElement ERROR_WHILE_FIND_PATH = "启用详细信息功能失败！ {0:S}";
        public static LanguageElement UNKNOWN_COMMAND = "无效的命令！{0:S}";
        public static LanguageElement STATUS_PLAYING = "玩";
        public static LanguageElement STATUS_EDITING = "做";
        public static LanguageElement STATUS_OTHER = "听";
        public static LanguageElement STATUS_TIP_INFO = "我在{0:S}{1:S}";
        public static LanguageElement STATUS_TIP_INFO_WRAP = "我在{0:S}{1:S}";
        public static LanguageElement ERROR_WHILE_SEARCH_MAP = "尝试检索{0:S}-{1:S} [{2:S}]时失败！{3:S}";
        public static LanguageElement CONSOLE_OUTPUT_RESULT = "文件{0:S} (时间: {1}) HP/CS/AR/OD: {2}{3}{4}{5}";
        public static LanguageElement OUTPUT_RESULT = "当前谱面{0}:{1}";
        public static LanguageElement CURRENT_IDLE = "当前没有在打图！";
    }
}
