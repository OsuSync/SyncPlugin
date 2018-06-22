using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    public class PathConfigurationItemCreator:BaseConfigurationItemCreator
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            var panel = base.CreateControl(attr, prop, configuration_instance);

            PathAttribute pattr = attr as PathAttribute;

            var evalue = GetConfigValue(prop, configuration_instance);

            var path_box = new TextBox() { Text = evalue, Width = 160,Height = 22, VerticalContentAlignment = VerticalAlignment.Center };
            var button = new Button() { Width = 75, Margin = new Thickness(5, 0, 5, 0) };

            if (!pattr.IsDirectory)
                button.Content = DefaultLanguage.BUTTON_OPEN;
            else
                button.Content = DefaultLanguage.BUTTON_BROWSE;

            panel.Children.Add(path_box);
            panel.Children.Add(button);

            button.Click += (s, e) =>
            {
                if (!pattr.IsDirectory)
                {
                    var fileDialog = new System.Windows.Forms.OpenFileDialog();
                    try
                    {
                        fileDialog.InitialDirectory = Path.GetFullPath(evalue);
                        fileDialog.FileName = Path.GetFileName(evalue);
                    }
                    catch(ArgumentException)
                    {
                        fileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    }
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
                SetConfigValue(prop,configuration_instance, path_box.Text);
            };

            path_box.TextChanged += (s, e) =>
            {
                if (pattr.Check(path_box.Text))
                    SetConfigValue(prop,configuration_instance, path_box.Text);
            };

            return panel;
        }
    }
}
