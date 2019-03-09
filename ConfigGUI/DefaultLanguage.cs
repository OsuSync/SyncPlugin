using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;

namespace ConfigGUI
{
    public class DefaultLanguage : I18nProvider
    {
        public static LanguageElement BUTTON_OPEN = "Open";
        public static LanguageElement BUTTON_BROWSE= "Browse";
        public static LanguageElement BUTTON_FONT = "Font";
        public static LanguageElement BUTTON_COLOR = "Color";
        public static LanguageElement WINDOW_TITLE = "Config";
        public static LanguageElement WINDOW_TITLE_REQUIRE_RESTART = "Some settings restart to take effect";

        public static LanguageElement BUTTON_SAVE = "Save";
        public static LanguageElement LABEL_SAVED = "Saved!";
        public static LanguageElement LABEL_SAVED_SAVING = "Saving...";

        public static LanguageElement TRAY_HIDE_SHOW = "Hide/Show";
        public static LanguageElement TRAY_CONFIG = "Config";
        public static LanguageElement TRAY_OPEN_SYNC_FOLDER = "Sync Folder";
        public static LanguageElement TRAY_EXIT= "Exit";

        public static LanguageElement COMMAND_LINE_HINT = "[ConfigGUI]Enter \"config\" to open the configuration panel.";
    }
}