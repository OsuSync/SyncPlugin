using ConfigGUI.ConfigurationI18n;
using Sync.Tools;
using Sync.Tools.ConfigGUI;
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

            if (configuration_instance == null
                || !I18nManager.Instance.TryGetLanguageValue(configuration_instance.GetType().Namespace, prop.Name, out string checkbox_content))
                checkbox_content = prop.Name;

            var checkbox = new CheckBox() { Content = $"{checkbox_content}{(attr.RequireRestart ? "(*)" : "")}", Margin = new Thickness(5, -2, 0, 0) };

            //set default value
            var evalue = Tools.GetConigValue(prop, configuration_instance);
            if (bool.TryParse(evalue, out bool bvalue))
                checkbox.IsChecked = bvalue;

            checkbox.Click += (s, e) =>
            {
                if(prop.PropertyType==typeof(ConfigurationElement))
                    prop.SetValue(configuration_instance, new ConfigurationElement(checkbox.IsChecked.ToString()));
                else if(prop.PropertyType == typeof(bool))
                    prop.SetValue(configuration_instance, checkbox.IsChecked);
            };

            panel.Children.Add(checkbox);
            return panel;
        }
    }
}
