using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace WpfApp4.Controls
{
    public class DateTimePicker : Control    
    {
        static DateTimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimePicker), new FrameworkPropertyMetadata(typeof(DateTimePicker)));
        }

        public DateTimePicker()
        {
            //Init();
            Time = DateTime.Now;
        }

        private void Init()
        {
            Year = Time.Year;
            Month = Time.Month;
            Day = Time.Day;
            Hour = Time.Hour;
            Minute = Time.Minute;
            Second = Time.Second;
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(DateTime), typeof(DateTimePicker), new PropertyMetadata(DateTime.Now, OnTimePropertyChanged));
        public DateTime Time
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        static void OnTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DateTimePicker;

            ctrl.Init();
        }

        public static readonly DependencyProperty YearProperty =
            DependencyProperty.Register("Year", typeof(int), typeof(DateTimePicker), new PropertyMetadata(DateTime.Now.Year, OnYearPropertyChanged));
        public int Year
        {
            get { return (int)GetValue(YearProperty); }
            set { SetValue(YearProperty, value); }
        }

        static void OnYearPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DateTimePicker;
            var year = (int)e.NewValue;

            if (ctrl.Time.Year != year)
                ctrl.Time = new DateTime(year, ctrl.Time.Month, Math.Min(DateTime.DaysInMonth(year, ctrl.Time.Month), ctrl.Time.Day), ctrl.Time.Hour, ctrl.Time.Minute, ctrl.Time.Second);
        }

        public static readonly DependencyProperty MonthProperty =
            DependencyProperty.Register("Month", typeof(int), typeof(DateTimePicker), new PropertyMetadata(DateTime.Now.Month, OnMonthPropertyChanged));
        public int Month
        {
            get { return (int)GetValue(MonthProperty); }
            set { SetValue(MonthProperty, value); }
        }

        static void OnMonthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DateTimePicker;
            var month = (int)e.NewValue;

            if (ctrl.Time.Month != month)
                ctrl.Time = new DateTime(ctrl.Time.Year, month, Math.Min(DateTime.DaysInMonth(ctrl.Time.Year, month), ctrl.Time.Day) , ctrl.Time.Hour, ctrl.Time.Minute, ctrl.Time.Second);
        }

        public static readonly DependencyProperty DayProperty =
            DependencyProperty.Register("Day", typeof(int), typeof(DateTimePicker), new PropertyMetadata(DateTime.Now.Day, OnDayPropertyChanged));
        public int Day
        {
            get { return (int)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        static void OnDayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DateTimePicker;
            var day = (int)e.NewValue;

            if (ctrl.Time.Day != day)
                ctrl.Time = new DateTime(ctrl.Time.Year, ctrl.Time.Month, day, ctrl.Time.Hour, ctrl.Time.Minute, ctrl.Time.Second);
        }

        public static readonly DependencyProperty HourProperty =
            DependencyProperty.Register("Hour", typeof(int), typeof(DateTimePicker), new PropertyMetadata(DateTime.Now.Hour, OnHourPropertyChanged));
        public int Hour
        {
            get { return (int)GetValue(HourProperty); }
            set { SetValue(HourProperty, value); }
        }

        static void OnHourPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DateTimePicker;
            var hour = (int)e.NewValue;

            if (ctrl.Time.Hour != hour)
                ctrl.Time = new DateTime(ctrl.Time.Year, ctrl.Time.Month, ctrl.Time.Day, hour, ctrl.Time.Minute, ctrl.Time.Second);
        }

        public static readonly DependencyProperty MinuteProperty =
            DependencyProperty.Register("Minute", typeof(int), typeof(DateTimePicker), new PropertyMetadata(DateTime.Now.Minute, OnMinutePropertyChanged));
        public int Minute
        {
            get { return (int)GetValue(MinuteProperty); }
            set { SetValue(MinuteProperty, value); }
        }

        static void OnMinutePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DateTimePicker;
            var minute = (int)e.NewValue;

            if (ctrl.Time.Minute != minute)
                ctrl.Time = new DateTime(ctrl.Time.Year, ctrl.Time.Month, ctrl.Time.Day, ctrl.Time.Hour, minute, ctrl.Time.Second);
        }

        public static readonly DependencyProperty SecondProperty =
            DependencyProperty.Register("Second", typeof(int), typeof(DateTimePicker), new PropertyMetadata(DateTime.Now.Second, OnSecondPropertyChanged));
        public int Second
        {
            get { return (int)GetValue(SecondProperty); }
            set { SetValue(SecondProperty, value); }
        }

        static void OnSecondPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DateTimePicker;
            var second = (int)e.NewValue;

            if (ctrl.Time.Second != second)
                ctrl.Time = new DateTime(ctrl.Time.Year, ctrl.Time.Month, ctrl.Time.Day, ctrl.Time.Hour, ctrl.Time.Minute, second);
        }
    }
}
