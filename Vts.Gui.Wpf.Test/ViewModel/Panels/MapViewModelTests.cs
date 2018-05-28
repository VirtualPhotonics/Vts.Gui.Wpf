using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// Summary description for MapViewModelTests
    /// </summary>
    [TestFixture]
    public class MapViewModelTests
    {
        public MapViewModelTests()
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
            var viewModel = new MapViewModel();
            Assert.AreEqual(viewModel.MinValue, 1e-9);
            Assert.AreEqual(viewModel.MaxValue, 1.0);
            Assert.IsTrue(viewModel.ScalingTypeOptionVM != null);
            Assert.IsTrue(viewModel.ColormapTypeOptionVM != null);
        }

        // The following tests verify the Relay Commands

        /// <summary>
        /// Verifies that the ClearMapCommand returns correct values
        /// </summary>
        [Test]
        public void verify_ClearMapCommand_returns_correct_values()
        {
            var viewModel = new MapViewModel();
            viewModel.PlotMap.Execute(null);
            Assert.AreEqual(viewModel.Bitmap, null);

        }
    }
}