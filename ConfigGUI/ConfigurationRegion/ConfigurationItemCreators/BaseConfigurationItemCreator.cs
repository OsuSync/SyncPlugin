using ConfigGUI.ConfigurationI18n;
using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    public abstract class BaseConfigurationItemCreator
    {
        protected static string GetConfigValue(PropertyInfo prop, object config_instance)
        {
            return prop.GetValue(config_instance)?.ToString() ?? "";
        }

        protected static void SetConfigValue(PropertyInfo prop, object config_instance,string value)
        {
            prop.SetValue(config_instance, new ConfigurationElement(value));
        }

        public virtual Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop,object configuration_instance)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Margin = new Thickness(0, 5, 0, 5);

            if (configuration_instance==null ||
                !I18nManager.Instance.TryGetLanguageValue(configuration_instance.GetType().Namespace, prop.Name, out string label_content))
                label_content = prop.Name;

            if (configuration_instance == null ||
                !I18nManager.Instance.TryGetLanguageDescription(configuration_instance.GetType().Namespace, prop.Name, out string description_content))
                description_content = "";

            Control label = new Label() { Content = $"{label_content}{(attr.RequireRestart?"(*)":"")}:", Margin = new Thickness(0, -3, 0, 0) };
            panel.Children.Add(label);
            if(!string.IsNullOrWhiteSpace(description_content))
                label.ToolTip = description_content;
            return panel;
        }
    }
}
