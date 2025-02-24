using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel
{
    [TestFixture]
    internal class WindowViewModelTests
    {
        [Test]
        public void Verify_version_returns_value()
        {
            var windowViewModel = new WindowViewModel();
            Assert.That(windowViewModel.Version, Is.Not.Null);
        }
    }
}
