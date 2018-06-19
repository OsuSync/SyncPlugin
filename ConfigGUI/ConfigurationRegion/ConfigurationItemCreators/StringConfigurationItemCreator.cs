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
    public class StringConfigurationItemCreator : BaseConfigurationItemCreator
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr,prop, configuration_instance);

            var evalue = GetConfigValue(prop, configuration_instance);

            var text = new TextBox() {
                Text = evalue,
                Width = 240,
                Height = 22,
                VerticalContentAlignment = VerticalAlignment.Center,
                AcceptsReturn = false
            };
            panel.Children.Add(text);

            text.TextChanged += (s, e) =>
            {
                SetConfigValue(prop,configuration_instance, text.Text);
            };

            return panel;
        }
    }
}
