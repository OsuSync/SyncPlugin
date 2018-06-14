using ConfigGUI.ConfigurationI18n;
using ConfigGUI.ConfigurationRegion;
using Sync.Tools;
using Sync.Tools.ConfigGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ConfigGUI
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private static I18nManager s_i18n_manager = new I18nManager();
        private ConfigurationItemFactory m_item_factory;

        private Panel m_sync_config_panel = null;

        public ConfigWindow(ConfigurationItemFactory itemFactory)
        {
            m_item_factory = itemFactory;

            InitializeComponent();
            InitializeConfigPanel();

            Title = DefaultLanguage.WINDOW_TITLE;
        }

        #region Plugins Config

        private void InitializeConfigPanel()
        {
            Type config_manager_type = typeof(PluginConfigurationManager);
            var config_manager_list = config_manager_type.GetField("ConfigurationSet", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null) as IEnumerable<PluginConfigurationManager>;

            List<TreeViewItem> tree_view_list = new List<TreeViewItem>();
            //each configuration manager
            foreach (var manager in config_manager_list)
            {
                //get plguin name
                string plugin_name = config_manager_type.GetField("name", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager) as string;

                var tree_item = new TreeViewItem() { Header = plugin_name };

                //get List<PluginConfiuration>
                var config_items_field = config_manager_type.GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
                var config_items_list = config_items_field.GetValue(manager);

                //List<PluginConfiuration>.GetEnumerator
                var enumerator = config_items_field.FieldType.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance)
                    .Invoke(config_items_list, null) as IEnumerator;

                //each List<PluginConfiuration>
                while (enumerator.MoveNext())
                {
                    var config_item = enumerator.Current;
                    var config_instance = config_item.GetType().GetField("config", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(config_item);
                    var config_type = config_instance.GetType();

                    if (config_type.GetCustomAttribute<HideAttribute>() != null) continue;

                    //Create config panle
                    var panle = GetConfigPanel(config_type, config_instance);
                    if (panle.Children.Count != 0)
                    {
                        var sub_tree_item = new TreeViewItem() { Header = config_type.Name };

                        sub_tree_item.Selected += (s, e) =>
                        {
                            //Get config panle
                            var content = GetConfigPanel(config_type, config_instance);
                            configRegion.Content = content;
                        };

                        tree_item.Items.Add(sub_tree_item);
                    }
                }

                if(tree_item.Items.Count != 0)
                    tree_view_list.Add(tree_item);
            }

            tree_view_list.Sort((a, b) => ((string)a.Header).CompareTo((string)b.Header));

            foreach (var view in tree_view_list)
                configsTreeView.Items.Add(view);
        }

        private Dictionary<object, ConfigurationPanel> m_configuration_region_dict = new Dictionary<object, ConfigurationPanel>();

        private Panel GetConfigPanel(Type config_type, object config_instance)
        {
            if (m_configuration_region_dict.TryGetValue(config_instance, out var region))
                return region.Panel;

            region = new ConfigurationPanel(config_type, config_instance);
            if (region.Panel.Children.Count != 0)
                m_configuration_region_dict.Add(config_instance, region);
            return region.Panel;
        }

        #endregion Plugins Config

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}