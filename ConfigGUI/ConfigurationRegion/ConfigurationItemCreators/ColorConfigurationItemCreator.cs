using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    public class ColorConfigurationItemCreator:BaseConfigurationItemCreator
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr, prop, configuration_instance);

            ColorAttribute cattr = attr as ColorAttribute;

            var color_str = GetConfigValue(prop, configuration_instance);

            var color_box = new TextBox() { Text = color_str, Width = 160, Height = 22, VerticalContentAlignment = VerticalAlignment.Center };
            var bound = new Border()
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = 15,
                Height = 15,
                Margin = new Thickness(5, 0, 0, 0)
            };
            var color_rect = new Rectangle() { };
            var button = new Button() { Content = DefaultLanguage.BUTTON_COLOR, Width = 75, Margin = new Thickness(5, 0, 5, 0) };

            var color = StringToColor(color_str);
            color_rect.Fill = new SolidColorBrush() { Color = Color.FromArgb(color.A, color.R, color.G, color.B) };

            bound.Child = color_rect;
            panel.Children.Add(color_box);
            panel.Children.Add(bound);
            panel.Children.Add(button);

            button.Click += (s, e) =>
            {
                var colorDialog = new System.Windows.Forms.ColorDialog();
                color_str = GetConfigValue(prop, configuration_instance);

                color = StringToColor(color_str);
                colorDialog.Color = color;
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    color = colorDialog.Color;
                    color_box.Text = RgbaToString(color.R, color.G, color.B, color.A);
                }
            };

            color_box.TextChanged += (s, e) =>
            {
                color = StringToColor(color_box.Text);
                color_rect.Fill = new SolidColorBrush() { Color = Color.FromArgb(color.A, color.R, color.G, color.B) };

                if (cattr.Check(color_box.Text))
                    SetConfigValue(prop,configuration_instance, color_box.Text);
            };

            return panel;
        }

        protected System.Drawing.Color StringToColor(string rgba)
        {
            if (rgba.Length != 9) return System.Drawing.Color.Black;

            var color = System.Drawing.Color.FromArgb(
                Convert.ToByte(rgba.Substring(7, 2), 16),
                Convert.ToByte(rgba.Substring(1, 2), 16),
                Convert.ToByte(rgba.Substring(3, 2), 16),
                Convert.ToByte(rgba.Substring(5, 2), 16));
            return color;
        }

        protected string RgbaToString(byte r, byte g, byte b, byte a)
        {
            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        }
    }
}
