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
            Assert.AreEqual("AxisLabel", constantAxisViewModel.AxisLabel);
            Assert.AreEqual(IndependentVariableAxis.Rho, constantAxisViewModel.AxisType);
            Assert.AreEqual("mm", constantAxisViewModel.AxisUnits);
            Assert.AreEqual(4.0, constantAxisViewModel.AxisValue);
            Assert.AreEqual(2, constantAxisViewModel.ImageHeight);
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
