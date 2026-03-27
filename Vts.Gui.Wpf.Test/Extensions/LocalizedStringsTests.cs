using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Resources;

namespace Vts.Gui.Wpf.Test.Extensions;

[TestFixture]
internal class LocalizedStringsTests
{
    [Test]
    public void Verify_get_localized_string()
    {
        var strings = LocalizedStrings.MainResource;
        Assert.That(strings, Is.InstanceOf<Strings>());
        Assert.That(Strings.Button_Cancel, Is.InstanceOf<string>());
        Assert.That(Strings.Button_Cancel.Length > 0, Is.True);
    }
}
