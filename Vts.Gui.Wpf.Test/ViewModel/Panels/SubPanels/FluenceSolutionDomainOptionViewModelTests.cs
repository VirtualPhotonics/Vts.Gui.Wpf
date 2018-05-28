using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.SubPanels
{
    /// <summary>
    /// Summary description for FluenceSolutionDomainOptionViewModelTests
    /// </summary>
    [TestFixture]
    public class FluenceSolutionDomainOptionViewModelTests
    {
        public FluenceSolutionDomainOptionViewModelTests()
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
        /// Verifies that FluenceSolutionDomainOptionModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new FluenceSolutionDomainOptionViewModel();
            Assert.AreEqual(viewModel.FluenceOfRhoAndZOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZ]);
            Assert.AreEqual(viewModel.FluenceOfFxAndZOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZ]);
            Assert.AreEqual(viewModel.FluenceOfRhoAndZAndTimeOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndTime]);
            Assert.AreEqual(viewModel.FluenceOfFxAndZAndTimeOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndTime]);
            Assert.AreEqual(viewModel.FluenceOfRhoAndZAndFtOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndFt]);
            Assert.AreEqual(viewModel.FluenceOfFxAndZAndFtOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndFt]);
        }

        // could add test for results set by UpdateOptions
    }
}
