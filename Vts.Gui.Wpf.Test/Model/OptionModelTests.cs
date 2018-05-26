using System;
using System.Numerics;
using NUnit.Framework;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Test.Model
{
    /// <summary>
    /// Tests OptionModel classe
    /// </summary>
    [TestFixture]
    public class OptionModelTests
    {
        /// <summary>
        /// Verifies constructor sets properties correctly
        /// </summary>
        [Test]
        public void verify_constructor_sets_properties_correctly()
        {
            int id = 1;
            bool enableMultiSelect = false;
            int sortValue = 2;
            var optionModel = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, id, "groupName", enableMultiSelect, sortValue);
            Assert.AreEqual(optionModel.DisplayName, "displayName");
            Assert.AreEqual(optionModel.ID, 1);
            Assert.AreEqual(optionModel.MultiSelectEnabled, false);
            Assert.AreEqual(optionModel.SortValue, 2);
        }
    }
}
