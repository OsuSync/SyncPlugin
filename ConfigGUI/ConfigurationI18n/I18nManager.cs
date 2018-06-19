using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConfigGUI.ConfigurationI18n
{
    sealed class I18nManager
    {
        private Dictionary<string, Dictionary<string, string>> m_i18n_dict = new Dictionary<string, Dictionary<string, string>>();
        public static I18nManager Instance;

        public I18nManager()
        {
            var i18n_list = typeof(Sync.Tools.I18n).GetField("ApplyedProvider", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as List<I18nProvider>;
            foreach (var i18n in i18n_list)
            {
                var type = i18n.GetType();
                var i18n_dict = new Dictionary<string, string>();
                m_i18n_dict.Add(type.Namespace, i18n_dict);

                foreach (var field in type.GetFields())
                {
                    if (field.FieldType==typeof(GuiLanguageElement))
                    {
                        string name = field.Name;
                        string value = (GuiLanguageElement)field.GetValue(i18n);
                        i18n_dict.Add(name, value);
                    }
                }
            }

            Instance = this;
        }

        public bool TryGetLanguageValue(string @namespace, string config_name, out string val)
        {
            val = null;
            if (m_i18n_dict.TryGetValue(@namespace, out var dict))
                if (dict.TryGetValue(config_name, out val))
                    return true;
            return false;
        }

        public bool TryGetLanguageDescription(string @namespace, string config_name, out string val)
        {
            val = null;
            if (m_i18n_dict.TryGetValue(@namespace, out var dict))
                if (dict.TryGetValue($"{config_name}Description", out val))
                    return true;
            return false;
        }
    }
}
