using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoAppWpfCS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}

class D : DependencyObject
{
    public static DependencyProperty AgeProperty = DependencyProperty.Register("Age", typeof(int), typeof(D));

    public int Age
    {
        get { return (int)GetValue(AgeProperty); }
        set { SetValue(AgeProperty, value);  }
    }
}


class C : INotifyPropertyChanged
{
    private int _p;
    public int p
    {
        get
        {
            return _p;
        }
        set
        {
            _p = value;
            var e = PropertyChanged;
            if (e != null) e(this, new PropertyChangedEventArgs(nameof(p)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
