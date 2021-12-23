using NUnit.Framework;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Test.Model
{
    /// <summary>
    /// Tests OptionModel classes
    /// </summary>
    [TestFixture]
    public class OptionModelTests
    {
        /// <summary>
        /// Verifies constructor sets properties correctly
        /// </summary>
        [Test]
        public void Verify_constructor_sets_properties_correctly()
        {
            const int id = 1;
            const bool enableMultiSelect = false;
            const int sortValue = 2;
            var optionModel = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, id, "groupName", enableMultiSelect, sortValue);
            Assert.AreEqual("displayName", optionModel.DisplayName);
            Assert.AreEqual(1, optionModel.Id);
            Assert.IsFalse(optionModel.MultiSelectEnabled);
            Assert.AreEqual(2, optionModel.SortValue);
        }
    }
}
