using Sync.Tools.ConfigurationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    public class FloatConfigurationItemCreator : SliderConfigurationItemCreator
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr, prop, configuration_instance);

            FloatAttribute iattr = attr as FloatAttribute;

            var slider = panel.Children[1] as Slider;
            var num_view = panel.Children[2] as TextBox;

            slider.Maximum = iattr.MaxValue;
            slider.Minimum = iattr.MinValue;
            slider.TickFrequency = iattr.Step;
            num_view.SetBinding(TextBox.TextProperty, new Binding("Value")
            {
                Source = slider,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat="F2"
            });

            return panel;
        }
    }
}
