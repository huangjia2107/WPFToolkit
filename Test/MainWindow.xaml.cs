using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIResources.Controls;
using UIResources.Datas;
using UIResources.Extensions;
using UIResources.Helps;
using UIResources.Panels;
using MvvmUtils.ViewModel;

namespace Test
{
    public class Number : ViewModelBase
    {
        private double _MiniValue = 0;
        public double MiniValue
        {
            get { return _MiniValue; }
            set
            {
                _MiniValue = value;
                RaisePropertyChanged();
            }
        }

        private double _MaxValue = 100;
        public double MaxValue
        {
            get { return _MaxValue; }
            set
            {
                _MaxValue = value;
                RaisePropertyChanged();
            }
        }

        private double _Value;
        public double Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                RaisePropertyChanged();
            }
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Number number = null;
        public MainWindow()
        {
            InitializeComponent();

            number = new Number
            {
                MiniValue = 0,
                Value = 0,
                MaxValue = 100
            };

            this.DataContext = number;
        }

        private void aa()
        {
            Trace.WriteLine("asaaaaaa");
        }

        public Action action;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //             this.DataContext = new Number
            //             {
            //                 MiniValue = 0,
            //                 Value = 30,
            //                 MaxValue = 80
            //             }; 
            AnchorPopup.Show(testBtn, "This is a test.sdcw dwdwdwd wdwdw dwdwdwdwj dwjwi jdwijdw0000");

            /*
            Action action1 = aa;

            Action eventHandler = this.action;
            Action eventHandler2;
            do
            {
                eventHandler2 = eventHandler;
                Action value2 = (Action)Delegate.Combine(eventHandler2, action1);
                eventHandler = Interlocked.CompareExchange<Action>(ref this.action, value2, eventHandler2);
            }
            while (eventHandler != eventHandler2);
            */

            //Action action = () => Trace.WriteLine("ddd");
            //             MessageBox.Show(string.Format("action.Target = {0},action.IsStatic = {1},  action1.Target = {2}, action1.IsStatic = {3}",
            //                 action.Target,
            //                 action.Method.IsStatic,
            //                 action1.Target,
            //                 action1.Method.IsStatic));
        } 
    }
}
