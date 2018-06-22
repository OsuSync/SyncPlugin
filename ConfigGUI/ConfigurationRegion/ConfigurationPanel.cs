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

namespace ConfigGUI.ConfigurationRegion
{
    class ConfigurationPanel
    {
        public Panel Panel { private set; get; } = new StackPanel();

        public ConfigurationPanel(Type configuration_type, object configuration_instance, ConfigurationHolderAttribute class_holder)
        {
            //each PluginConfiuration
            foreach (var prop in configuration_type.GetProperties())
            {
                if (prop.PropertyType != typeof(ConfigurationElement)) continue;

                var config_attr = prop.GetCustomAttribute<BaseConfigurationAttribute>();
                bool hide = config_attr?.Hide ?? class_holder?.Hide ?? false;

                if (hide == true) continue;
                if (config_attr == null)
                    config_attr = new StringAttribute();

                string name = prop.Name;

                var item_panel = ConfigurationItemFactory.Instance.CreateItemPanel(config_attr, prop, configuration_instance);
                if (item_panel == null)
                    throw new NullReferenceException($"Creator return null! Config attribute type: {config_attr}");
                Panel.Children.Add(item_panel);
            }
        }
    }
}
