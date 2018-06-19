using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Sync.Tools.ConfigurationAttribute;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    public class IntegerConfigurationItemCreator: SliderConfigurationItemCreator
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr, prop, configuration_instance);

            IntegerAttribute iattr = attr as IntegerAttribute;

            var slider = panel.Children[1] as Slider;

            slider.Maximum = iattr.MaxValue;
            slider.Minimum = iattr.MinValue;

            return panel;
        }
    }
}
