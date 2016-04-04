using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DemoAppStoreCS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
    }
}

class C : Control
{
    public static DependencyProperty AgeProperty = DependencyProperty.Register("Age", typeof(int), typeof(C), null);

    public int Age
    {
        get { return (int)GetValue(AgeProperty); }
        set { SetValue(AgeProperty, value); }
    }
}

class MyViewModel : INotifyPropertyChanged
{
    private int _Age;
    public int Age
    {
        get
        {
            return _Age;
        }
        set 
        {
            _Age = value;
var e = PropertyChanged;
if (e!=null) e(this, _AgeChanged);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private static PropertyChangedEventArgs _AgeChanged = new PropertyChangedEventArgs("Age");
}
