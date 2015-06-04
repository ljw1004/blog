using System.Runtime.CompilerServices;

class Program
{
    static void Main()
    {
        f();
    }

    static bool b = false;

    static void f()
    {
        Windows.Stuff.Dummy.dummy2();

        if (b)
        {
            Windows.Stuff.Dummy.dummy2();
        }
    }
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
}


namespace Windows.Stuff
{
    public class Dummy
    {
        public void dummy1() { }
        public static void dummy2() { }
    }
}

namespace Windows.Foundation.Metadata
{
    public class ApiInformation
    {
        public static bool IsTypePresent(string s) => true;
    }
}

