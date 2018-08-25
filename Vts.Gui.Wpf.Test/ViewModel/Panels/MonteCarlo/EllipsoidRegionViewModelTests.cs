﻿using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo
{
    /// <summary>
    /// Summary description for EllipsoidRegionViewModelTests
    /// </summary>
    [TestFixture]
    public class EllipsoidRegionViewModelTests
    {
        /// <summary>
        /// Verifies that EllipsoidRegionModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new EllipsoidRegionViewModel();
            Assert.AreEqual(viewModel.Name, StringLookup.GetLocalizedString("Label_Tissue"));
            Assert.IsTrue(viewModel.IsEllipsoid);
            Assert.IsFalse(viewModel.IsLayer);
            Assert.AreEqual(viewModel.Units, StringLookup.GetLocalizedString("Measurement_mm"));
            Assert.IsTrue(viewModel.OpticalPropertyVM != null);
        }
        
    }
}
