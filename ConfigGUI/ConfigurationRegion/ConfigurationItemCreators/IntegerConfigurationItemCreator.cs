using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Sync.Tools.ConfigGUI;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    class IntegerConfigurationItemCreator: SliderConfigurationItemCreator
    {
        public override Panel CreateControl(ConfigAttributeBase attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr, prop, configuration_instance);

            ConfigIntegerAttribute iattr = attr as ConfigIntegerAttribute;

            var slider = panel.Children[1] as Slider;

            slider.Maximum = iattr.MaxValue;
            slider.Minimum = iattr.MinValue;

            return panel;
        }
    }
}
