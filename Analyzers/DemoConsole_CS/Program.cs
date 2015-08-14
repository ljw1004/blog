using System.Runtime.CompilerServices;

class Program
{
    static void Main()
    {
        f();
    }

    static bool b = false;

    static void f(int i) { }

    static void f()
    {
        Windows.Stuff.Dummy.dummy2();

        if (b)
        {
            Windows.Stuff.Dummy.dummy2();
        }
    }

    int p { get { return q; }  set { } }

    int q => Windows.Stuff.Dummy.dummy2();
}

class C
{
    void g()
    {
        if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("a"))
        {
            Windows.Stuff.Dummy.dummy2();
        }

        Windows.Stuff.Dummy.dummy2();
    }

    public C() { }
}


namespace Windows.Stuff
{
    public class Dummy
    {
        public void dummy1() { }
        public static int dummy2() { return 1; }
        public static event System.Action e;
    }
}

namespace Windows.Foundation.Metadata
{
    public class ApiInformation
    {
        public static bool IsTypePresent(string s) => true;
    }
}

