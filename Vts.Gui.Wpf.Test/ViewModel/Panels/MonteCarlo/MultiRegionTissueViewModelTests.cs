using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel.Panels.MonteCarlo;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo;

/// <summary>
/// Summary description for MultiRegionTissueViewModelTests
/// </summary>
[TestFixture]
public class MultiRegionTissueViewModelTests
{
    /// <summary>
    /// Verifies that MultiRegionTissueModel default constructor instantiates sub view models
    /// </summary>
    [Test]
    public void Verify_default_constructor_sets_properties_correctly()
    {
        ITissueInput tissueInput =
            // verify MultiLayerTissueInput
            new MultiLayerTissueInput();
        var viewModel = new MultiRegionTissueViewModel(tissueInput); 
        Assert.That(viewModel.RegionsVm != null, Is.True);
        var listOfTissueRegions = viewModel.RegionsVm;
        Assert.That(((LayerRegionViewModel)listOfTissueRegions![1]).IsLayer, Is.True);
        // verify SingleEllipsoidTissueInput
        tissueInput = new SingleEllipsoidTissueInput();
        viewModel = new MultiRegionTissueViewModel(tissueInput);
        Assert.That(viewModel.RegionsVm != null, Is.True);
        listOfTissueRegions = viewModel.RegionsVm;
        Assert.That(listOfTissueRegions, Is.Not.Null);
        Assert.That(((EllipsoidRegionViewModel) listOfTissueRegions[3]).IsEllipsoid, Is.True);
        // verify SingleVoxelTissueInput
        tissueInput = new SingleVoxelTissueInput();
        viewModel = new MultiRegionTissueViewModel(tissueInput);
        Assert.That(viewModel.RegionsVm != null, Is.True);
        listOfTissueRegions = viewModel.RegionsVm;
        Assert.That(listOfTissueRegions, Is.Not.Null);
        Assert.That(((VoxelRegionViewModel)listOfTissueRegions[3]).IsVoxel, Is.True);
    }

    // The following tests verify the Relay Commands
    // no relay commands in this class
}
