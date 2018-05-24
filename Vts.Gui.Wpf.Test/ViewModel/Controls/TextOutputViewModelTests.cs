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
        public void verify_default_constructor_relay_command_correctly()
        {
            var textOutputVM = new TextOutputViewModel();
            Assert.IsTrue(textOutputVM.TextOutput_PostMessage != null);
        }

        /// <summary>
        /// Verifies that TextOutputViewModel relay command works correctly
        /// </summary>
        [Test]
        public void verify_relay_command_works_correctly()
        {
            var windowViewModel = new WindowViewModel();
            var textOutputVM = windowViewModel.TextOutputVM;
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("UnitTest:");
            Assert.IsTrue(textOutputVM.Text == "UnitTest:");
        }

    }
}
