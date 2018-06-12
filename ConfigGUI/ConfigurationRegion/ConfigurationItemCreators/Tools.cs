using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConfigGUI.ConfigurationRegion.ConfigurationItemCreators
{
    public static class Tools
    {
        public static string GetConigValue(PropertyInfo prop, object config_instance)
        {
            return prop.GetValue(config_instance)?.ToString() ?? "";
        }
    }
}
