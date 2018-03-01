using ConfigGUI.ConfigurationRegion.ConfigurationItemCreators;
using Sync.Tools.ConfigGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ConfigGUI.ConfigurationRegion
{
    class ConfigurationItemFactory
    {
        public static ConfigurationItemFactory Instance;

        public Dictionary<Type,ConfigurationItemCreatorBase> m_items_mapping = new Dictionary<Type, ConfigurationItemCreatorBase>();

        public ConfigurationItemFactory()
        {
            Instance = this;

            m_items_mapping.Add(typeof(ConfigStringAttribute),new StringConfigurationItemCreator());
            m_items_mapping.Add(typeof(ConfigBoolAttribute), new BoolConfigurationItemCreator());
            m_items_mapping.Add(typeof(ConfigFloatAttribute), new FloatConfigurationItemCreator());
            m_items_mapping.Add(typeof(ConfigIntegerAttribute), new IntegerConfigurationItemCreator());
            m_items_mapping.Add(typeof(ConfigFontAttribute), new FontConfigurationItemCreator());
            m_items_mapping.Add(typeof(ConfigColorAttribute), new ColorConfigurationItemCreator());
            m_items_mapping.Add(typeof(ConfigListAttribute), new ListConfigurationItemCreator());
            m_items_mapping.Add(typeof(ConfigReflectListAttribute), new ListConfigurationItemCreator());
            m_items_mapping.Add(typeof(ConfigPathAttribute), new PathConfigurationItemCreator());
        }

        public Panel CreateItemPanel(ConfigAttributeBase attr, PropertyInfo prop, object configuration_instance)
        {
            return m_items_mapping[attr.GetType()].CreateControl(attr,prop, configuration_instance);
        }
    }
}
