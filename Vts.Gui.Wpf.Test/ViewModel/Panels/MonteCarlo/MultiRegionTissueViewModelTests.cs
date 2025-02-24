using System.Linq;
using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo
{
    /// <summary>
    /// Summary description for MultiRegionTissueViewModelTests
    /// </summary>
    [TestFixture]
    public class MultiRegionTissueViewModelTests
    {
        /// <summary>
        /// Verifies that MultiRegionTissueModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            ITissueInput tissueInput;
            // verify MultiLayerTissueInput
            tissueInput = new MultiLayerTissueInput();
            var viewModel = new MultiRegionTissueViewModel(tissueInput); 
            Assert.That(viewModel.RegionsVM != null, Is.True);
            var listOfTissueRegions = viewModel.RegionsVM;
            Assert.That(((LayerRegionViewModel)listOfTissueRegions[1]).IsLayer, Is.True);
            // verify SingleEllipsoidTissueInput
            tissueInput = new SingleEllipsoidTissueInput();
            viewModel = new MultiRegionTissueViewModel(tissueInput);
            Assert.That(viewModel.RegionsVM != null, Is.True);
            listOfTissueRegions = viewModel.RegionsVM;
            Assert.That(((EllipsoidRegionViewModel) listOfTissueRegions[3]).IsEllipsoid, Is.True);
            // verify SingleVoxelTissueInput
            tissueInput = new SingleVoxelTissueInput();
            viewModel = new MultiRegionTissueViewModel(tissueInput);
            Assert.That(viewModel.RegionsVM != null, Is.True);
            listOfTissueRegions = viewModel.RegionsVM;
            Assert.That(((VoxelRegionViewModel)listOfTissueRegions[3]).IsVoxel, Is.True);
        }
   
        // The following tests verify the Relay Commands
        /// <summary>
        /// no relay commands in this class
        /// </summary>
   
    }
}
