using System.Windows;
using System.Windows.Controls;

namespace UIResources.Controls
{
    public class SpitButtonMenuItem : MenuItem
    {
        static SpitButtonMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpitButtonMenuItem), new FrameworkPropertyMetadata(typeof(SpitButtonMenuItem)));

            ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)));
            template.Seal();
            ItemsPanelProperty.OverrideMetadata(typeof(SpitButtonMenuItem), new FrameworkPropertyMetadata(template));
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MenuItem();
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns></returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FrameworkElement;
        }
    }
}
