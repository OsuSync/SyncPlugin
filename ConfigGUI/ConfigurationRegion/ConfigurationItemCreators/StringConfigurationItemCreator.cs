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
    class StringConfigurationItemCreator : ConfigurationItemCreatorBase
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr,prop, configuration_instance);

            var evalue = Tools.GetConigValue(prop, configuration_instance);

            var text = new TextBox() { Text = evalue, Width = 240, VerticalContentAlignment = VerticalAlignment.Center };
            panel.Children.Add(text);

            text.TextChanged += (s, e) =>
            {
                if(prop.PropertyType == typeof(ConfigurationElement))
                    prop.SetValue(configuration_instance, new ConfigurationElement($"{text.Text}"));
                else if(prop.PropertyType == typeof(string))
                    prop.SetValue(configuration_instance, text.Text);
            };

            return panel;
        }
    }
}
