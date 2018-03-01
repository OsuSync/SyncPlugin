using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Sync.Tools;
using Sync.Tools.ConfigGUI;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    class PathConfigurationItemCreator:ConfigurationItemCreatorBase
    {
        public override Panel CreateControl(ConfigAttributeBase attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr, prop, configuration_instance);

            ConfigPathAttribute pattr = attr as ConfigPathAttribute;

            var evalue = Tools.GetConigValue(prop, configuration_instance);

            var path_box = new TextBox() { Text = evalue, Width = 160, VerticalContentAlignment = VerticalAlignment.Center };
            var button = new Button() { Width = 75, Margin = new Thickness(5, 0, 5, 0) };

            if (pattr.IsFilePath)
                button.Content = DefaultLanguage.BUTTON_OPEN;
            else
                button.Content = DefaultLanguage.BUTTON_BROWSE;

            panel.Children.Add(path_box);
            panel.Children.Add(button);

            button.Click += (s, e) =>
            {
                if (pattr.IsFilePath)
                {
                    var fileDialog = new System.Windows.Forms.OpenFileDialog();
                    fileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    fileDialog.RestoreDirectory = true;
                    if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        path_box.Text = fileDialog.FileName;
                }
                else
                {
                    var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        path_box.Text = folderDialog.SelectedPath;
                }
                prop.SetValue(configuration_instance, new ConfigurationElement($"{path_box.Text}"));
            };

            path_box.TextChanged += (s, e) =>
            {
                if (pattr.Check(path_box.Text))
                    prop.SetValue(configuration_instance, new ConfigurationElement($"{path_box.Text}"));
            };

            return panel;
        }
    }
}
