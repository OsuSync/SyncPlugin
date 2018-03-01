using Sync.Tools.ConfigGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    class FloatConfigurationItemCreator : SliderConfigurationItemCreator
    {
        public override Panel CreateControl(ConfigAttributeBase attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr, prop, configuration_instance);

            ConfigFloatAttribute iattr = attr as ConfigFloatAttribute;

            var slider = panel.Children[1] as Slider;

            slider.Maximum = iattr.MaxValue;
            slider.Minimum = iattr.MinValue;
            slider.TickFrequency = iattr.Step;

            return panel;
        }
    }
}
