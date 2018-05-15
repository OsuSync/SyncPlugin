using ConfigGUI.ConfigurationI18n;
using ConfigGUI.ConfigurationRegion;
using ConfigGUI.MultiSelect;
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
        private static I18nManager s_i18n_manager = new I18nManager();
        private static ConfigurationItemFactory s_item_factory = new ConfigurationItemFactory();

        private Panel m_sync_config_panel = null;

        public ConfigWindow()
        {
            InitializeComponent();

            m_sync_config_panel = CreateSyncConfigPanel();

            InitializeSyncConfigPanel();
            InitializeConfigPanel();

            Title = DefaultLanguage.WINDOW_TITLE;
        }

        #region Sync Config
        private void InitializeSyncConfigPanel()
        {
            var tree_item = new TreeViewItem() { Header = "Sync" };
            configsTreeView.Items.Add(tree_item);
            tree_item.Selected += (s, e) =>
              {
                  configRegion.Content = m_sync_config_panel;
              };
        }

        private Panel CreateSyncConfigPanel()
        {
            var sync_config_type = typeof(Configuration);
            StackPanel panel = new StackPanel();

            foreach (var prop in sync_config_type.GetProperties())
            {
                BaseConfigurationAttribute attr =null;

                if (prop.PropertyType == typeof(bool))
                    attr = new BoolAttribute();
                else if (prop.PropertyType == typeof(string))
                    attr= new StringAttribute();

                var item_panel=ConfigurationItemFactory.Instance.CreateItemPanel(attr, prop, null);
                panel.Children.Insert(0, item_panel);
            }

            return panel;
        }
        #endregion

        #region Plugins Config
        private void InitializeConfigPanel()
        {
            Type config_manager_type = typeof(PluginConfigurationManager);
            var config_manager_list = config_manager_type.GetField("ConfigurationSet", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null) as IEnumerable<PluginConfigurationManager>;

            List<TreeViewItem> tree_view_list= new List<TreeViewItem>();
            //each configuration manager
            foreach (var manager in config_manager_list)
            {
                //get plguin name
                var plguin = config_manager_type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager) as Plugin;
                var tree_item = new TreeViewItem() { Header = plguin.Name };

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

                    var sub_tree_item = new TreeViewItem() { Header = config_type.Name };

                    sub_tree_item.Selected += (s, e) =>
                    {
                        var panle = GetConfigPanel(config_type, config_instance);
                        configRegion.Content = panle;
                    };

                    tree_item.Items.Add(sub_tree_item);
                }
                tree_view_list.Add(tree_item);
            }

            tree_view_list.Sort((a, b) => ((string)a.Header).CompareTo((string)b.Header));

            foreach (var view in tree_view_list)
                configsTreeView.Items.Add(view);
        }

        private Dictionary<object, ConfigurationPanel> m_configuration_region_dict = new Dictionary<object, ConfigurationPanel>();

        private Panel GetConfigPanel(Type config_type,object config_instance)
        {
            if (m_configuration_region_dict.TryGetValue(config_instance, out var region))
                return region.Panel;

            region = new ConfigurationPanel(config_type, config_instance);

            m_configuration_region_dict.Add(config_instance, region);
            return region.Panel;
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
