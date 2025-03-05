using System;
using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.SubPanels
{
    [TestFixture]
    internal class ConstantAxisViewModelTests
    {
        [Test]
        public void Verify_Constant_axis()
        {
            var constantAxisViewModel = new ConstantAxisViewModel
            {
                AxisLabel = "AxisLabel",
                AxisType = IndependentVariableAxis.Rho,
                AxisUnits = "mm",
                AxisValue = 4.0,
                ImageHeight = 2
            };
            Assert.That(constantAxisViewModel.AxisLabel, Is.EqualTo("AxisLabel"));
            Assert.That(constantAxisViewModel.AxisType, Is.EqualTo(IndependentVariableAxis.Rho));
            Assert.That(constantAxisViewModel.AxisUnits, Is.EqualTo("mm"));
            Assert.That(constantAxisViewModel.AxisValue, Is.EqualTo(4.0));
            Assert.That(constantAxisViewModel.ImageHeight, Is.EqualTo(2));
        }

        [Test]
        public void Verify_Constant_axis_wavelength()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var constantAxisViewModel = new ConstantAxisViewModel
                {
                    AxisLabel = "Wavelength",
                    AxisType = IndependentVariableAxis.Wavelength,
                    AxisUnits = "nm",
                    AxisValue = 700,
                    ImageHeight = 2
                };
            });
        }
    }
}
