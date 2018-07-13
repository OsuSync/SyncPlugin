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
    }
}