using ConfigGUI.ConfigurationRegion.ConfigurationItemCreators;
using Sync.Tools.ConfigurationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ConfigGUI.ConfigurationRegion
{
    public class ConfigurationItemFactory
    {
        public static ConfigurationItemFactory Instance;

        public Dictionary<Type,BaseConfigurationItemCreator> m_items_mapping = new Dictionary<Type, BaseConfigurationItemCreator>();

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
            m_items_mapping.Add(typeof(PathAttribute), new PathConfigurationItemCreator());
        }

        public void RegisterItemCreator<T>(BaseConfigurationItemCreator creator) where T: BaseConfigurationAttribute
        {
            m_items_mapping.Add(typeof(T),creator);
        }

        public Panel CreateItemPanel(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            BaseConfigurationItemCreator creator;
            Type type = attr.GetType();

            if (!m_items_mapping.TryGetValue(type, out creator))
            {
                IEnumerable<KeyValuePair<Type, BaseConfigurationItemCreator>> list = m_items_mapping;
                KeyValuePair<Type, BaseConfigurationItemCreator> pair;

                while (list.Count()>1)
                {
                    list = list.Where(p => type.IsSubclassOf(p.Key) || p.Key.IsAssignableFrom(type));

                    pair = list.FirstOrDefault();

                    if(pair.Key==null || pair.Value==null)
                    {
                        type = typeof(StringAttribute);
                        creator = m_items_mapping[type];
                        break;
                    }

                    type = pair.Key;
                    creator = pair.Value;

                    list = list.Take(1);
                }
            }
            return creator.CreateControl(attr,prop, configuration_instance);
        }
    }
}
