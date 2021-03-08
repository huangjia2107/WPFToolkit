/*
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;

using Prism.Regions;
using System;

namespace UIResources.Regions
{
    public class StackPanelRegionAdapter : RegionAdapterBase<StackPanel>
    {
        public StackPanelRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {

        }

        protected override void Adapt(IRegion region, StackPanel regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (FrameworkElement element in e.NewItems)
                    {
                        if(regionTarget.Children.Count==0)
                            regionTarget.Children.Add(element);
                        else
                        {
                            for(int i = 0; i < regionTarget.Children.Count;i++)
                            {
                                var child = regionTarget.Children[i];

                                var childAttr = Attribute.GetCustomAttribute(child.GetType(), typeof(ViewSortHintAttribute)) as ViewSortHintAttribute;
                                if (childAttr == null)
                                {
                                    regionTarget.Children.Add(element);
                                    break;
                                }

                                var elementAttr = Attribute.GetCustomAttribute(element.GetType(), typeof(ViewSortHintAttribute)) as ViewSortHintAttribute;
                                if (elementAttr == null)
                                {
                                    regionTarget.Children.Add(element);
                                    break;
                                }

                                if (string.Compare( elementAttr.Hint, childAttr.Hint)<0)
                                {
                                    regionTarget.Children.Insert(i, element);
                                    break;
                                }

                                if (i == regionTarget.Children.Count - 1)
                                    regionTarget.Children.Add(element);
                            }
                        }
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (FrameworkElement view in e.OldItems)
                    {
                        regionTarget.Children.Remove(view);
                    }
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
*/