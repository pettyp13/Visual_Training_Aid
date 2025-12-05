using NUnit.Framework;

public class EditModeTests
{
    [Test]
    public void Addition_Works()
    {
        int result = 2 + 2;
        Assert.AreEqual(4, result);
    }

    [Test]
    public void Boolean_Logic_Works()
    {
        bool a = true;
        bool b = false;
        Assert.IsTrue(a && !b);
    }

    [Test]
    public void String_Concatenation_Works()
    {
        string hello = "Hello";
        string world = "World";
        string combined = hello + " " + world;
        Assert.AreEqual("Hello World", combined);
    }
}
