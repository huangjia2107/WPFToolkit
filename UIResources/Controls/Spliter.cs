using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using UIResources.Helps;
using Utils.Common;

namespace UIResources.Controls
{
    [TemplatePart(Name = ItemsControlTemplateName, Type = typeof(ItemsControl))]
    public class Spliter : Control
    {
        private const string ItemsControlTemplateName = "PART_ItemsControl";
        private ItemsControl _itemsControl = null;

        private double _maxLength = 0;
        private ObservableCollection<double> _originalItems = null;
        private ObservableCollection<SpliterSpan> _spanList = null;

        private IEnumerable<double> _oldItems = null;

        static Spliter()
        {
            EventManager.RegisterClassHandler(typeof(Spliter), Thumb.DragStartedEvent, new DragStartedEventHandler(OnThumbDragStarted));
            EventManager.RegisterClassHandler(typeof(Spliter), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
            EventManager.RegisterClassHandler(typeof(Spliter), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnThumbDragCompleted));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(Spliter), new FrameworkPropertyMetadata(typeof(Spliter)));
        }

        public Spliter()
        {
            _spanList = new ObservableCollection<SpliterSpan>();
        }

        public static readonly RoutedEvent ItemsChangedEvent = EventManager.RegisterRoutedEvent("ItemsChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<IEnumerable<double>>), typeof(Spliter));
        public event RoutedPropertyChangedEventHandler<IEnumerable<double>> ItemsChanged
        {
            add { AddHandler(ItemsChangedEvent, value); }
            remove { RemoveHandler(ItemsChangedEvent, value); }
        }

        #region Dependency Property

        private static readonly DependencyPropertyKey ItemsPropertyKey =
            DependencyProperty.RegisterReadOnly("Items", typeof(IEnumerable<double>), typeof(Spliter), new PropertyMetadata(null));
        public static readonly DependencyProperty ItemsProperty = ItemsPropertyKey.DependencyProperty;
        public IEnumerable<double> Items
        {
            get { return (IEnumerable<double>)GetValue(ItemsProperty); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(Spliter), new FrameworkPropertyMetadata(Orientation.Vertical, OnOrientationChanged));
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spliter = (Spliter)d;

            spliter.UpdateMaxLength();
            spliter.UpdateSpans();
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<double>), typeof(Spliter), new FrameworkPropertyMetadata(null, OnItemsSourceChanged));
        public IEnumerable<double> ItemsSource
        {
            get { return (IEnumerable<double>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var spliter = (Spliter)d;
            if (e.NewValue == null)
            {
                spliter.RemoveCollectionChanged();
                spliter._originalItems = null;

                spliter.CoerceItems(null);
            }
            else
            {
                var observableCollection = e.NewValue as ObservableCollection<double>;
                if (observableCollection != null)
                {
                    spliter.RemoveCollectionChanged();
                    spliter._originalItems = observableCollection;
                    spliter.AddCollectionChanged();

                    spliter.CoerceItems(observableCollection);
                }
                else
                {
                    spliter.CoerceItems((IEnumerable<double>)e.NewValue);
                }

                spliter.UpdateMaxLength();
            }

            spliter.UpdateSpans();
        }

        #endregion

        #region Override

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);

            UpdateMaxLength();
            UpdateSpans();

            return size;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _itemsControl = GetTemplateChild(ItemsControlTemplateName) as ItemsControl;

            UpdateMaxLength();
        }

        #endregion

        #region Event

        private void RemoveCollectionChanged()
        {
            if (_originalItems != null)
                _originalItems.CollectionChanged -= OnOriginalItemsChanged;
        }

        private void AddCollectionChanged()
        {
            if (_originalItems != null)
                _originalItems.CollectionChanged += OnOriginalItemsChanged;
        }

        private void OnOriginalItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    {
                        CoerceItems(_originalItems);
                        UpdateSpans();
                    }
                    break;
            }
        }

        private static void OnThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            var spliter = sender as Spliter;
            spliter.OnThumbDragStarted(e);
        }

        private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var spliter = sender as Spliter;
            spliter.OnThumbDragDelta(e);
        }

        private static void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var spliter = sender as Spliter;
            spliter.OnThumbDragCompleted(e);
        }

        private void OnThumbDragStarted(DragStartedEventArgs e)
        {
            _oldItems = Items == null ? null : Items.ToArray();
        }

        private void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            var thumb = e.OriginalSource as Thumb;
            var span = thumb.DataContext as SpliterSpan;

            var delta = Orientation == Orientation.Vertical ? e.VerticalChange : e.HorizontalChange;
            var pos = Orientation == Orientation.Vertical ? Grid.GetRow(thumb) : Grid.GetColumn(thumb);

            switch (pos)
            {
                case 0:

                    if (DoubleUtil.LessThan(delta, 0))
                    {
                        span.Start -= UpDistance(span, -delta);
                    }
                    else
                    {
                        var dis = Math.Min(delta, span.End - span.Start - 10);
                        span.Start += dis;
                    }

                    break;
                case 1:

                    if (DoubleUtil.LessThan(delta, 0))
                    {
                        var dis = UpDistance(span, -delta);

                        span.Start -= dis;
                        span.End -= dis;
                    }
                    else
                    {
                        var dis = DownDistance(span, delta);

                        span.Start += dis;
                        span.End += dis;
                    }

                    break;
                case 2:

                    if (DoubleUtil.LessThan(delta, 0))
                    {
                        var dis = Math.Min(-delta, span.End - span.Start - 10);
                        span.End -= dis;
                    }
                    else
                    {
                        span.End += DownDistance(span, delta); ;
                    }

                    break;
            }

            UpdateSpanToItems();
        }

        private void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            if (_oldItems != Items)
                RaiseEvent(new RoutedPropertyChangedEventArgs<IEnumerable<double>>(_oldItems, Items, ItemsChangedEvent));
        }

        #endregion

        #region Func

        private void CoerceItems(IEnumerable<double> items)
        {
            _oldItems = Items == null ? null : Items.ToArray();
            if (items != null)
            {
                //1.less than 0, reset to 0;
                //2.greater than 1, reset to 1;
                //3.(1)merge multi duplicate values to one;
                //  (2)merge multi duplicate values to two;
                //  (3)remove all duplicate values;
                //4.Sort;

                //(1)
                //var filteresItems = items.Select(v => Math.Min(1, Math.Max(0, v))).Distinct(new DoubleEqualityComparer()).OrderBy(v => v, new DoubleComparer());

                //(2)
                //var filteresItems = items.Select(v => Math.Min(1, Math.Max(0, v))).GroupBy(v => v, new DoubleEqualityComparer()).SelectMany(g => g.Take(2)).OrderBy(v => v, new DoubleComparer());

                //(3)
                var filteresItems = items.Select(v => Math.Min(1, Math.Max(0, v))).GroupBy(v => v, new DoubleEqualityComparer()).Where(g => g.Count() == 1).Select(g => g.Key).OrderBy(v => v, new DoubleComparer());

                var count = filteresItems.Count();

                SetValue(ItemsPropertyKey, count % 2 == 0 ? filteresItems : filteresItems.Take(count - 1));
                RaiseEvent(new RoutedPropertyChangedEventArgs<IEnumerable<double>>(_oldItems, Items, ItemsChangedEvent));

                return;
            }

            SetValue(ItemsPropertyKey, null);

            if (_oldItems != null)
                RaiseEvent(new RoutedPropertyChangedEventArgs<IEnumerable<double>>(_oldItems, null, ItemsChangedEvent));
        }

        private void UpdateMaxLength()
        {
            if (_itemsControl == null)
                return;

            if (Orientation == Orientation.Horizontal)
                _maxLength = _itemsControl.ActualWidth;
            else
                _maxLength = _itemsControl.ActualHeight;
        }

        private void UpdateSpans()
        {
            _spanList.Clear();

            if (Items == null)
            {
                if (_itemsControl != null)
                    _itemsControl.ItemsSource = null;

                return;
            }

            var items = Items.ToList();
            for (int i = 0; i < items.Count; i++)
            {
                if (i % 2 == 0)
                    continue;

                _spanList.Add(new SpliterSpan { Start = _maxLength * items[i - 1], End = _maxLength * items[i] });
            }

            if (_itemsControl != null && _itemsControl.ItemsSource == null)
                _itemsControl.ItemsSource = _spanList;
        }

        private void UpdateSpanToItems()
        {
            var items = new List<double>();
            foreach (var span in _spanList)
            {
                items.Add(span.Start / _maxLength);
                items.Add(span.End / _maxLength);
            }

            SetValue(ItemsPropertyKey, items);
        }

        private double UpDistance(SpliterSpan dragedSpan, double deltaDis)
        {
            var leftSpan = _spanList.LastOrDefault(s => DoubleUtil.LessThanOrClose(s.End, dragedSpan.Start));
            return Math.Min(deltaDis, leftSpan == null ? dragedSpan.Start : (Math.Max(0, dragedSpan.Start - leftSpan.End)));
        }

        private double DownDistance(SpliterSpan dragedSpan, double deltaDis)
        {
            var rightSpan = _spanList.FirstOrDefault(s => DoubleUtil.GreaterThanOrClose(s.Start, dragedSpan.End));
            return Math.Min(deltaDis, (rightSpan == null ? (_maxLength - dragedSpan.End) : Math.Max(0, rightSpan.Start - dragedSpan.End)));
        }

        #endregion

        #region Struct

        class SpliterSpan : INotifyPropertyChanged
        {
            private double _start;
            public double Start
            {
                get { return _start; }
                set
                {
                    _start = value;
                    RaisePropertyChanged();
                }
            }

            private double _end;
            public double End
            {
                get { return _end; }
                set
                {
                    _end = value;
                    RaisePropertyChanged();
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentException("This method cannot be called with an empty string", "propertyName");

                var propertyChanged = this.PropertyChanged;

                if (propertyChanged != null)
                    propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion
        }

        #endregion

    }
}
