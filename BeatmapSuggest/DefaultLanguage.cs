using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.Tools;

namespace BeatmapSuggest
{
    public class DefaultLanguage:I18nProvider
    {
        public static LanguageElement LANG_GET_BEATMAP_FAILED = "获取谱面{0}信息失败,原因:{1}";
        public static LanguageElement LANG_SUGGEST_MEG = "{0} want you to play the beatmap [{1} {2}] || [{3} dl] || [{4} mirror] or type\"?dl\"/\"?dl all\"";
        public static LanguageElement LANG_NOT_FOUND_ERR = "找不到匹配的内容或者id并不是有效的beatmapSetId";
        public static LanguageElement LANG_UNKNOWN_TITLE = "<unk title>";
        public static LanguageElement LANG_GET_BEATMAP_TIME_OUT = "获取谱面{0}信息超时,TaskStatus{1}";

        public static LanguageElement LANG_INVAILD_ID = "无效的id值 {0}";
        public static LanguageElement LANG_UNKOWN_PARAM = "未知参数 {0}";

        public static LanguageElement LANG_ERROR_INFO_IMCOMPLETE = "信息不完整";
        public static LanguageElement LANG_NO_API_KEY_NOFITY = "没有ApiKey,请用户自己提供ApiKey以便使用谱面推荐功能.ApiKey申请地址:https://osu.ppy.sh/p/api";
        public static LanguageElement LANG_START_DOWNLOAD = "开始下载谱面{0}";
        public static LanguageElement LANG_FINISH_DOWNLOAD = "下载谱面{0}完成";
        public static LanguageElement LANG_FAILED_DOWNLOAD= "下载谱面{0}出错,原因{1}";
        public static LanguageElement LANG_DOWNLOAD_TASK_COUNT = "开始下载共{0}张谱面.";
    }
}
