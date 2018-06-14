using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Sync.Tools.ConfigGUI;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    class HideConfigurationItemCreator:BaseConfigurationItemCreator
    {
        public override Panel CreateControl(BaseConfigurationAttribute attr, PropertyInfo prop, object configuration_instance)
        {
            return null;
        }
    }
}
