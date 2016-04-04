//using Finglebing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        var s = "Main";
    }
}

//class C : INotifyPropertyChanged
//{
//    private int _p;
//    public int p {
//        get {
//            var s = "hello \{p}";
//            return _p;
//        }
//        set {
//            _p = value;
//            var e = PropertyChanged;
//            if (e != null) e(this, new PropertyChangedEventArgs("p"));
//        }
//    }

//    private int _q;
//    public int q {
//        get {
//            return _q;
//        }
//        set {
//            _q = value;
//            var e = PropertyChanged;
//            if (e != null) e(this, new PropertyChangedEventArgs(nameof(q)));

//            if (e != null) e(this, new PropertyChangedEventArgs(nameof(C.q)));
//        }
//    }
//    public event PropertyChangedEventHandler PropertyChanged;

//}

//namespace NS1
//{
//    class C<T>
//    {
//        void f<U>(int arg)
//        {
//            Console.WriteLine("NS1"); // not implemented
//            Console.WriteLine("T"); // not implemented
//            Console.WriteLine("U"); // not implemented
//            Console.WriteLine("C"); // not implemented - neither type nor constructor
//            Console.WriteLine("f");
//            Console.WriteLine("arg");
//            Console.WriteLine("p");
//        }
//        int p { get; set; }
//        C() { }
//    }

//    struct S
//    {
//        void f(int arg)
//        {
//            Console.WriteLine("S"); // not implemented
//            Console.WriteLine("f");
//            Console.WriteLine("arg");
//            Console.WriteLine("p");
//        }
//        int p { get; set; }
//    }
//}
