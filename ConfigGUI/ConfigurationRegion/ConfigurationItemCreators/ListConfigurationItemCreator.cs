using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ConfigGUI.MultiSelect;
using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    public class ListConfigurationItemCreator:BaseConfigurationItemCreator
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            ListAttribute lattr = attr as ListAttribute;
            return lattr.AllowMultiSelect ? CreateMultiSelectList(lattr, prop, configuration_instance) : CreateSingleSelectList(lattr, prop, configuration_instance);
        }

        public Panel CreateSingleSelectList(ListAttribute lattr, PropertyInfo prop, object configuration_instance)
        {
            var evalue = GetConfigValue(prop, configuration_instance);
            var panel = base.CreateControl(lattr, prop, configuration_instance);

            var combo_list = new ComboBox() { Width = 150 };
            combo_list.ItemsSource = lattr.ValueList;
            combo_list.SelectedIndex = Array.IndexOf(lattr.ValueList, evalue.ToString());

            panel.Children.Add(combo_list);

            combo_list.SelectionChanged += (s, e) =>
            {
                SetConfigValue(prop,configuration_instance, $"{combo_list.SelectedValue}");
            };

            return panel;
        }

        public Panel CreateMultiSelectList(ListAttribute lattr, PropertyInfo prop, object configuration_instance)
        {
            var evalue = GetConfigValue(prop, configuration_instance);
            var panel = base.CreateControl(lattr, prop, configuration_instance);

            string[] values = lattr.ValueList;
            IEnumerable<string> default_values = evalue.ToString().Split(lattr.SplitSeparator).Select(s => s.Trim());

            var multi_list = new MultiSelectComboBox() { Width = 250 };
            var dict = new Dictionary<string, object>();
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
                SetConfigValue(prop,configuration_instance, string.Join(lattr.SplitSeparator.ToString(), multi_list.SelectedItems.Keys));
            };

            panel.Children.Add(multi_list);

            return panel;
        }
    }
}
