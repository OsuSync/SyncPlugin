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

            m_items_mapping.Add(typeof(StringAttribute),new StringConfigurationItemCreator());
            m_items_mapping.Add(typeof(BoolAttribute), new BoolConfigurationItemCreator());
            m_items_mapping.Add(typeof(FloatAttribute), new FloatConfigurationItemCreator());
            m_items_mapping.Add(typeof(IntegerAttribute), new IntegerConfigurationItemCreator());
            m_items_mapping.Add(typeof(FontAttribute), new FontConfigurationItemCreator());
            m_items_mapping.Add(typeof(ColorAttribute), new ColorConfigurationItemCreator());
            m_items_mapping.Add(typeof(ListAttribute), new ListConfigurationItemCreator());
            m_items_mapping.Add(typeof(ReflectListAttribute), new ListConfigurationItemCreator());
            m_items_mapping.Add(typeof(PathAttribute), new PathConfigurationItemCreator());
        }

        public Panel CreateItemPanel(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            return m_items_mapping[attr.GetType()].CreateControl(attr,prop, configuration_instance);
        }
    }
}
