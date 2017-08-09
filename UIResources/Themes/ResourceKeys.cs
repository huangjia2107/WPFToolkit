using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UIResources.Themes
{
    public class ResourceKeys
    {

        /// <summary>
        /// Style="{DynamicResource {x:Static themes:ResourceKeys.NoBgButtonStyleKey}}"
        /// </summary>
        internal const string NoBgButtonStyle = "NoBgButtonStyle";
        public static ComponentResourceKey NoBgButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NoBgButtonStyle); }
        }
    }
}
