using System;
using System.Linq;
using NUnit.Framework;
using Vts;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// Summary description for SpectralMappingViewModelTests
    /// </summary>
    [TestFixture]
    public class SpectralMappingViewModelTests
    {
        public SpectralMappingViewModelTests()
        {
            // constructor logic if needed goes here
        }

        /// <summary>
        /// setup and tear down
        /// </summary>
        [OneTimeSetUp]
        public void setup()
        {
            //clear_folders_and_files();

        }

        [OneTimeTearDown]
        public void clear_folders_and_files()
        {
            //foreach (var folder in listOfInfileFolders)
            //{
            //    if (Directory.Exists(folder))
            //    {
            //        Directory.Delete(folder);
            //    }
            //}

        }

        /// <summary>
        /// Verifies that MapViewModel default constructor 
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            // setting the SelectedTissue involves having the WindowViewModel instantiated
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.SpectralMappingVM;
            Assert.IsTrue(viewModel.ScatteringTypeVM != null);
            Assert.IsTrue(viewModel.BloodConcentrationVM != null);
            Assert.IsTrue(viewModel.WavelengthRangeVM != null);
            var listOfTissueTypes = viewModel.Tissues.Select(t => t.TissueType).ToList();
            CollectionAssert.AreEquivalent( new[]
                {
                    TissueType.Skin,
                    TissueType.BrainWhiteMatter,
                    TissueType.BrainGrayMatter,
                    TissueType.BreastPreMenopause,
                    TissueType.BreastPostMenopause,
                    TissueType.Liver,
                    TissueType.IntralipidPhantom,
                    TissueType.Custom
                }, 
                listOfTissueTypes);
            Assert.IsTrue(viewModel.SelectedTissue.TissueType == TissueType.Skin);
            Assert.AreEqual(viewModel.ScatteringTypeVM.SelectedValue, viewModel.SelectedTissue.ScattererType);
            Assert.AreEqual(viewModel.ScatteringTypeName, "Vts.SpectralMapping.Tissue");
            Assert.IsTrue(Math.Abs(viewModel.OpticalProperties.Mua - 0.1677) < 0.0001);
            Assert.IsTrue(Math.Abs(viewModel.OpticalProperties.Musp - 2.212) < 0.001);
            Assert.AreEqual(viewModel.OpticalProperties.G, 0.8);
            Assert.AreEqual(viewModel.OpticalProperties.N, 1.4);
            Assert.AreEqual(viewModel.Wavelength, 650);
        }

        // The following tests verify the Relay Commands

        /// <summary>
        /// Verifies that the PlotMuaSpectrumCommand returns correct values
        /// </summary>
        [Test]
        public void verify_PlotMuaSpectrumCommand_returns_correct_values()
        {
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.SpectralMappingVM;
            viewModel.PlotMuaSpectrumCommand.Execute(null);
            Assert.AreEqual(windowViewModel.PlotVM.Labels[0], "μa spectra");
            Assert.AreEqual(windowViewModel.PlotVM.Title, "μa [mm-1] versus λ [nm]");
        }


        /// <summary>
        /// Verifies that the PlotMusSpectrumCommand returns correct values
        /// </summary>
        [Test]
        public void verify_PlotMusSpectrumCommand_returns_correct_values()
        {
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.SpectralMappingVM;
            viewModel.PlotMuspSpectrumCommand.Execute(null);
            Assert.AreEqual(windowViewModel.PlotVM.Labels[0], "μs' spectra");
            Assert.AreEqual(windowViewModel.PlotVM.Title, "μs' [mm-1] versus λ [nm]");
        }
    }
}