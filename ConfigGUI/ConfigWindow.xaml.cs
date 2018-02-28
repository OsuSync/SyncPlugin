using MultiSelect;
using Sync.Plugins;
using Sync.Tools;
using Sync.Tools.ConfigGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConfigGUI
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            InitializeI18n();
            InitializeConfigPanel();
        }

        private Dictionary<string, Dictionary<string, string>> m_i18n_dict = new Dictionary<string, Dictionary<string, string>>();

        private void InitializeI18n()
        {
            var i18n_list = typeof(I18n).GetField("ApplyedProvider",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null) as List<I18nProvider>;
            foreach(var i18n in i18n_list)
            {
                var type = i18n.GetType();
                var i18n_dict = new Dictionary<string,string>();
                m_i18n_dict.Add(type.Namespace, i18n_dict);
                
                foreach (var field in type.GetFields())
                {
                    if(field.GetCustomAttribute<ConfigI18nAttribute>()!=null)
                    {
                        string name=field.Name;
                        string value = (LanguageElement)field.GetValue(i18n);
                        i18n_dict.Add(name, value);
                    }
                }
            }
        }

        private void InitializeConfigPanel()
        {
            Type config_manager_type = typeof(PluginConfigurationManager);
            var config_manager_list = config_manager_type.GetField("ConfigurationSet", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null) as LinkedList<PluginConfigurationManager>;

            foreach (var manager in config_manager_list)
            {
                var plguin = config_manager_type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager) as Plugin;
                var config_items_field = config_manager_type.GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
                var config_items_list = config_items_field.GetValue(manager);
                var enumerator = config_items_field.FieldType.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance)
                    .Invoke(config_items_list, null) as IEnumerator;

                var tree_item = new TreeViewItem() { Header = plguin.Name };


                while (enumerator.MoveNext())
                {
                    var config_item = enumerator.Current;
                    var config_instance = config_item.GetType().GetField("config", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(config_item);
                    var config_type = config_instance.GetType();

                    var sub_tree_item = new TreeViewItem() { Header = config_type.Name };

                    sub_tree_item.Selected += (s, e) =>
                    {
                        var panle = GetConfigPanel(config_type, config_instance);
                        configRegion.Content = panle;
                    };

                    tree_item.Items.Add(sub_tree_item);
                }
                configsTreeView.Items.Add(tree_item);
            }
        }

        private Dictionary<object, StackPanel> stackPanelDictionary = new Dictionary<object, StackPanel>();

        private StackPanel GetConfigPanel(Type config_type,object config_instance)
        {
            if (stackPanelDictionary.TryGetValue(config_instance, out var stack_panel))
                return stack_panel;

            stack_panel=new StackPanel();

            foreach(var prop in config_type.GetProperties())
            {
                if (prop.PropertyType != typeof(ConfigurationElement)) continue;

                var attr=prop.GetCustomAttribute<ConfigAttributeBase>();
                if (attr == null) attr = new ConfigStringAttribute();

                string label_content = prop.Name;
                if (m_i18n_dict.TryGetValue(config_type.Namespace, out var dict))
                    if(dict.TryGetValue(prop.Name,out var name))
                        label_content = name;


                stack_panel.Children.Add(CreateControlFromAttribute(prop,label_content,config_instance,attr));
            }

            stackPanelDictionary.Add(config_instance, stack_panel);
            return stack_panel;
        }

        private UIElement CreateControlFromAttribute(PropertyInfo prop,string name,object config_instance, ConfigAttributeBase attr)
        {
            StackPanel uIElement = new StackPanel();
            uIElement.Orientation = Orientation.Horizontal;
            uIElement.Margin = new Thickness(0,5,0,5);

            Control desc_label = desc_label = new Label() { Content = $"{name}:",Margin=new Thickness(0,-5,0,0) };

            var evalue = GetConigValue(prop, config_instance);

            switch (attr)
            {
                case ConfigBoolAttribute battr:
                    var checkbox = new CheckBox() { Content = $"{name}",Margin =new Thickness(0,-2,0,0)};
                    if (bool.TryParse(evalue, out bool bvalue))
                        checkbox.IsChecked=bvalue;

                    checkbox.Click += (s, e) =>
                    {
                        prop.SetValue(config_instance, new ConfigurationElement(checkbox.IsChecked.ToString()));
                    };
                    desc_label = checkbox;
                    break;
                case ConfigIntegerAttribute iattr:
                    {
                        var slider = new Slider()
                        {
                            Maximum = iattr.MaxValue,
                            Minimum = iattr.MinValue,
                            Width = 200,
                            IsSnapToTickEnabled = true,
                        };

                        if (int.TryParse(evalue, out int ivalue))
                            slider.Value = ivalue;
                        uIElement.Children.Add(slider);

                        var num_view = new TextBox() { Text = $"{(int)slider.Value}", Width = 50, VerticalContentAlignment = VerticalAlignment.Center };
                        uIElement.Children.Add(num_view);

                        num_view.SetBinding(TextBox.TextProperty, new Binding("Value") { Source = slider });

                        slider.ValueChanged += (s, e) =>
                          {
                              prop.SetValue(config_instance, new ConfigurationElement($"{(int)slider.Value}"));
                          };
                    }
                    break;
                case ConfigFloatAttribute dattr:
                    {
                        var slider = new Slider()
                        {
                            Maximum = dattr.MaxValue,
                            Minimum = dattr.MinValue,
                            Width = 200,
                            IsSnapToTickEnabled = true,
                        };

                        if (float.TryParse(evalue, out float fvalue))
                            slider.Value = fvalue;
                        uIElement.Children.Add(slider);

                        var num_view = new TextBox() { Text = $"{slider.Value}" ,Width = 50, VerticalContentAlignment = VerticalAlignment.Center };
                        uIElement.Children.Add(num_view);

                        num_view.SetBinding(TextBox.TextProperty, new Binding("Value") { Source = slider });

                        slider.ValueChanged += (s, e) =>
                        {
                            prop.SetValue(config_instance, new ConfigurationElement($"{slider.Value:F4}"));
                        };
                    }
                    break;
                case ConfigPathAttribute pattr:
                    {
                        var path_box = new TextBox() { Text = evalue, Width = 160,VerticalContentAlignment=VerticalAlignment.Center };
                        var button = new Button() {Width=75,Margin=new Thickness(5,0,5,0)};

                        if(pattr.IsFilePath)
                            button.Content = "Open";
                        else
                            button.Content = "Browse";

                        uIElement.Children.Add(path_box);
                        uIElement.Children.Add(button);

                        button.Click += (s, e) =>
                          {
                              if(pattr.IsFilePath)
                              {
                                  var fileDialog = new System.Windows.Forms.OpenFileDialog();
                                  fileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                                  fileDialog.RestoreDirectory = true;
                                  if(fileDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
                                      path_box.Text = fileDialog.FileName;
                              }
                              else
                              {
                                  var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                                  if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                      path_box.Text = folderDialog.SelectedPath;
                              }
                              prop.SetValue(config_instance, new ConfigurationElement($"{path_box.Text}"));
                          };
                    }
                    break;
                case ConfigColorAttribute cattr:
                    {
                        var color_box = new TextBox() { Text = evalue, Width = 160, VerticalContentAlignment = VerticalAlignment.Center };
                        var button = new Button() {Content="Select", Width = 75, Margin = new Thickness(5, 0, 5, 0) };

                        uIElement.Children.Add(color_box);
                        uIElement.Children.Add(button);
                        button.Click += (s, e) =>
                        {
                            var colorDialog=new System.Windows.Forms.ColorDialog();
                            var color_str = GetConigValue(prop, config_instance);

                            colorDialog.Color = StringToColor(color_str);
                            colorDialog.FullOpen = true;
                            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                color_box.Text = RgbaToString(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B, colorDialog.Color.A);
                            prop.SetValue(config_instance, new ConfigurationElement($"{color_box.Text}"));
                        };
                    }
                    break;
                case ConfigFontAttribute fattr:
                    {
                        var font_box = new TextBox() { Text = evalue, Width = 160, VerticalContentAlignment = VerticalAlignment.Center };
                        var button = new Button() { Content = "Font", Width = 75, Margin = new Thickness(5, 0, 5, 0) };

                        uIElement.Children.Add(font_box);
                        uIElement.Children.Add(button);

                        button.Click += (s, e) =>
                          {
                              var fontDialog = new System.Windows.Forms.FontDialog();
                              var font_str = GetConigValue(prop, config_instance);

                              fontDialog.Font=new System.Drawing.Font(font_str,20);
                              if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                  font_box.Text = fontDialog.Font.Name;
                              prop.SetValue(config_instance, new ConfigurationElement($"{font_box.Text}"));
                          };
                    }
                    break;
                case ConfigListAttribute lattr:
                    {
                        if(lattr.AllowMultiSelect)
                        {
                            string[] values = lattr.ValueList;
                            IEnumerable<string> default_values = evalue.ToString().Split(lattr.SplitSeparator).Select(s => s.Trim());

                            var multi_list = new MultiSelectComboBox() { Width=250};
                            var dict= new Dictionary<string, object>();
                            var default_dict = new Dictionary<string, object>();

                            if (values != null)
                            {
                                foreach (var val in values)
                                    dict.Add(val, val);
                                foreach (var val in default_values)
                                    default_dict.Add(val, val);
                            }
                            multi_list.ItemsSource = dict;
                            multi_list.SelectedItems = default_dict;
                            multi_list.Click += (s, e) =>
                              {
                                  prop.SetValue(config_instance, new ConfigurationElement(string.Join(lattr.SplitSeparator.ToString(), multi_list.SelectedItems.Keys)));
                              };

                            uIElement.Children.Add(multi_list);
                        }
                        else
                        {
                            var combo_list = new ComboBox() { Width=150};
                            combo_list.ItemsSource = lattr.ValueList;
                            combo_list.SelectedIndex = Array.IndexOf(lattr.ValueList, evalue.ToString());
                            uIElement.Children.Add(combo_list);

                            combo_list.SelectionChanged += (s, e) =>
                              {
                                  prop.SetValue(config_instance, new ConfigurationElement($"{combo_list.SelectedValue}"));
                              };
                        }
                    }
                    break;
                case ConfigStringAttribute sattr:
                    var text=new TextBox() { Text = evalue,Width = 160, VerticalContentAlignment = VerticalAlignment.Center };
                    uIElement.Children.Add(text);

                    text.TextChanged += (s, e) =>
                      {
                          prop.SetValue(config_instance, new ConfigurationElement($"{text.Text}"));
                      };
                    break;
            }

            uIElement.Children.Insert(0, desc_label);
            return uIElement;
        }

        private ConfigurationElement GetConigValue(PropertyInfo prop, object config_instance)
        {
            return prop.GetValue(config_instance) as ConfigurationElement;
        }

        private System.Drawing.Color StringToColor(string rgba)
        {
            var color = System.Drawing.Color.FromArgb(
                Convert.ToByte(rgba.Substring(7, 2), 16),
                Convert.ToByte(rgba.Substring(1, 2), 16),
                Convert.ToByte(rgba.Substring(3, 2), 16),
                Convert.ToByte(rgba.Substring(5, 2), 16));
            return color;
        }

        private string RgbaToString(byte r,byte g,byte b,byte a)
        {
            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
