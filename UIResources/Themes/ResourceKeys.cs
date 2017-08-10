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
        /// Style="{DynamicResource/StaticResource {x:Static themes:ResourceKeys.NoBgButtonStyleKey}}"
        /// </summary>
        public const string NoBgButtonStyle = "NoBgButtonStyle";
        public static ComponentResourceKey NoBgButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), NoBgButtonStyle); }
        }

        public const string TitlebarButtonStyle = "TitlebarButtonStyle";
        public static ComponentResourceKey TitlebarButtonStyleKey
        {
            get { return new ComponentResourceKey(typeof(ResourceKeys), TitlebarButtonStyle); }
        }
    }
}
