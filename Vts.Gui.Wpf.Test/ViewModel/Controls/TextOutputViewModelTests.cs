using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;
using Vts.Gui.Wpf.ViewModel.Controls;

namespace Vts.Gui.Wpf.Test.ViewModel.Controls;

/// <summary>
/// Tests for TextOutputViewModel class
/// </summary>
[TestFixture]
public class TextOutputViewModelTests
{
    /// <summary>
    /// Verifies that TextOutputViewModel default constructor instantiates RelayCommand
    /// </summary>
    [Test]
    public void Verify_default_constructor_relay_command_correctly()
    {
        var textOutputVm = new TextOutputViewModel();
        Assert.That(textOutputVm.TextOutputPostMessage != null, Is.True);
    }

    /// <summary>
    /// Verifies that TextOutputViewModel relay command works correctly
    /// </summary>
    [Test]
    public void Verify_relay_command_works_correctly()
    {
        var windowViewModel = new WindowViewModel();
        var textOutputVm = windowViewModel.TextOutputVm;
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute("UnitTest:");
        Assert.That(textOutputVm.Text, Is.EqualTo("UnitTest:"));
    }

    [Test]
    public void Verify_append_text_appends_text()
    {
        var windowViewModel = new WindowViewModel();
        var textOutputVm = windowViewModel.TextOutputVm;
        textOutputVm.AppendText("Hello");
        Assert.That(textOutputVm.Text, Is.EqualTo("Hello"));
    }
}