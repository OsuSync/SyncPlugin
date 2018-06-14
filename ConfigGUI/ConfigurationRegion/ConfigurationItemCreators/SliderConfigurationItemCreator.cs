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
using System.Windows.Data;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    public abstract class SliderConfigurationItemCreator:BaseConfigurationItemCreator
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr,prop, configuration_instance);

            var slider = new Slider()
            {
                Width = 200,
                IsSnapToTickEnabled = true,
            };

            //set value
            var evalue = Tools.GetConigValue(prop, configuration_instance);
            if (int.TryParse(evalue, out int ivalue))
                slider.Value = ivalue;

            var num_view = new TextBox()
            {
                Text = $"{(int)slider.Value}",
                Width = 50,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
            };

            num_view.SetBinding(TextBox.TextProperty, new Binding("Value") { Source = slider });

            slider.ValueChanged += (s, e) =>
            {
                prop.SetValue(configuration_instance, new ConfigurationElement($"{(int)slider.Value}"));
            };

            panel.Children.Add(slider);
            panel.Children.Add(num_view);

            return panel;
        }
    }
}
