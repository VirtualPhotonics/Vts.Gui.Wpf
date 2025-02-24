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
            Assert.That(positionViewModel.X, Is.EqualTo(0));
            Assert.That(positionViewModel.Y, Is.EqualTo(0));
            Assert.That(positionViewModel.Z, Is.EqualTo(0));
            Assert.That(positionViewModel.Units, Is.EqualTo("mm"));
            Assert.That(positionViewModel.Title, Is.EqualTo("Position:"));
        }

        [Test]
        public void Verify_changing_point_values()
        {
            var positionViewModel = new PositionViewModel {X = 1, Y = 2, Z = 3};
            Assert.That(positionViewModel.X, Is.EqualTo(1));
            Assert.That(positionViewModel.Y, Is.EqualTo(2));
            Assert.That(positionViewModel.Z, Is.EqualTo(3));
        }

        [Test]
        public void Verify_get_position()
        {
            var positionViewModel = new PositionViewModel { X = 2, Y = 4, Z = 6 };
            var position = positionViewModel.GetPosition();
            Assert.That(position.X, Is.EqualTo(2.0d));
            Assert.That(position.Y, Is.EqualTo(4.0d));
            Assert.That(position.Z, Is.EqualTo(6.0d));
        }

        [Test]
        public void Verify_check_title()
        {
            var positionViewModel = new PositionViewModel();
            Assert.That(positionViewModel.ShowTitle, Is.True);
        }
    }
}
