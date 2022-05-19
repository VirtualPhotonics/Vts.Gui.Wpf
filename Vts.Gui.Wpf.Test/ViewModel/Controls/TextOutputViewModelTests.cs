using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Controls
{
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
            Assert.IsTrue(textOutputVm.TextOutput_PostMessage != null);
        }

        /// <summary>
        /// Verifies that TextOutputViewModel relay command works correctly
        /// </summary>
        [Test]
        public void Verify_relay_command_works_correctly()
        {
            var windowViewModel = new WindowViewModel();
            var textOutputVm = windowViewModel.TextOutputVM;
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("UnitTest:");
            Assert.AreEqual("UnitTest:", textOutputVm.Text);
        }

        [Test]
        public void Verify_append_text_appends_text()
        {
            var windowViewModel = new WindowViewModel();
            var textOutputVm = windowViewModel.TextOutputVM;
            textOutputVm.AppendText("Hello");
            Assert.AreEqual("Hello", textOutputVm.Text);
        }
    }
}