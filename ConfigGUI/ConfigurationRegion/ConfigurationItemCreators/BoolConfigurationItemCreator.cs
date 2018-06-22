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
    public class BoolConfigurationItemCreator:BaseConfigurationItemCreator
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr,prop, configuration_instance);
            panel.Children.Clear();

            if (configuration_instance == null || 
                !I18nManager.Instance.TryGetLanguageValue(configuration_instance.GetType().Namespace, prop.Name, out string checkbox_content))
                checkbox_content = prop.Name;

            if (configuration_instance == null ||
                !I18nManager.Instance.TryGetLanguageDescription(configuration_instance.GetType().Namespace, prop.Name, out string description_content))
                description_content = "";

            var checkbox = new CheckBox() { Content = $"{checkbox_content}{(attr.RequireRestart ? "(*)" : "")}", Margin = new Thickness(5, -2, 0, 0) };

            //set default value
            var evalue = GetConfigValue(prop, configuration_instance);
            if (bool.TryParse(evalue, out bool bvalue))
                checkbox.IsChecked = bvalue;

            checkbox.Click += (s, e) =>
            {
                SetConfigValue(prop,configuration_instance, checkbox.IsChecked.ToString());
            };

            if (!string.IsNullOrWhiteSpace(description_content))
                checkbox.ToolTip = description_content;

            panel.Children.Add(checkbox);
            return panel;
        }
    }
}
