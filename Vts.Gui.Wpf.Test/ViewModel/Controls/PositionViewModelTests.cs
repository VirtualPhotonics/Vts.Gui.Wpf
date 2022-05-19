using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Controls
{
    [TestFixture]
    internal class PositionViewModelTests
    {
        [Test]
        public void Verify_default_constructor()
        {
            var positionViewModel = new PositionViewModel();
            Assert.AreEqual(0, positionViewModel.X);
            Assert.AreEqual(0, positionViewModel.Y);
            Assert.AreEqual(0, positionViewModel.Z);
            Assert.AreEqual("mm", positionViewModel.Units);
            Assert.AreEqual("Position:", positionViewModel.Title);
        }

        [Test]
        public void Verify_changing_point_values()
        {
            var positionViewModel = new PositionViewModel {X = 1, Y = 2, Z = 3};
            Assert.AreEqual(1, positionViewModel.X);
            Assert.AreEqual(2, positionViewModel.Y);
            Assert.AreEqual(3, positionViewModel.Z);
        }

        [Test]
        public void Verify_get_position()
        {
            var positionViewModel = new PositionViewModel { X = 2, Y = 4, Z = 6 };
            var position = positionViewModel.GetPosition();
            Assert.AreEqual(2.0d, position.X);
            Assert.AreEqual(4.0d, position.Y);
            Assert.AreEqual(6.0d, position.Z);
        }

        [Test]
        public void Verify_check_title()
        {
            var positionViewModel = new PositionViewModel();
            Assert.IsTrue(positionViewModel.ShowTitle);
        }
    }
}
