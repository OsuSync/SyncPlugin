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

namespace ConfigGUI.ConfigurationRegion
{
    class ConfigurationPanel
    {
        public Panel Panel { private set; get; } = new StackPanel();

        public ConfigurationPanel(Type configuration_type, object configuration_instance)
        {
            //each PluginConfiuration
            foreach (var prop in configuration_type.GetProperties())
            {
                if (prop.PropertyType != typeof(ConfigurationElement)) continue;

                var config_attr = prop.GetCustomAttribute<BaseConfigurationAttribute>() ?? new StringAttribute();
                var holder_attr = prop.GetCustomAttribute<ConfigurationHolderAttribute>() ?? new ConfigurationHolderAttribute();

                if (holder_attr.Hide == true) continue;

                string name = prop.Name;

                var item_panel = ConfigurationItemFactory.Instance.CreateItemPanel(config_attr, prop, configuration_instance);
                if (item_panel == null)
                    throw new NullReferenceException($"Creator return null! Config attribute type: {config_attr}");
                Panel.Children.Add(item_panel);
            }
        }
    }
}
