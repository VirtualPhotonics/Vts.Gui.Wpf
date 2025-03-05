using System;
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
        private OptionModel<ColormapType> _optionModel;

        [OneTimeSetUp]
        public void Setup()
        {
            const int id = 1;
            const bool enableMultiSelect = false;
            const int sortValue = 2;
            _optionModel = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, id, "groupName", enableMultiSelect, sortValue);
        }

        /// <summary>
        /// Verifies constructor sets properties correctly
        /// </summary>
        [Test]
        public void Verify_constructor_sets_properties_correctly()
        {
            Assert.That(_optionModel.DisplayName, Is.EqualTo("displayName"));
            Assert.That(_optionModel.Id, Is.EqualTo(1));
            Assert.That(_optionModel.MultiSelectEnabled, Is.False);
            Assert.That(_optionModel.SortValue, Is.EqualTo(2));
            Assert.That(_optionModel.GroupName, Is.EqualTo("groupName"));
            Assert.That(_optionModel.IsEnabled, Is.True);
        }

        [Test]
        public void Verify_CompareTo_returns_minus_1()
        {
            var optionModel = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, 2, "groupName", false, 3);
            var val = _optionModel.CompareTo(optionModel);
            Assert.That(val, Is.EqualTo(-1));
        }

        [Test]
        public void Verify_CompareTo_null_object_returns_minus_1()
        {
            OptionModel<ColormapType> optionModel = null;
            var val = _optionModel.CompareTo(optionModel);
            Assert.That(val, Is.EqualTo(-1));
        }

        [Test]
        public void Verify_CompareTo_sort_values_returns_0()
        {
            var optionModel = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, 2, "groupName", false, 2);
            var val = _optionModel.CompareTo(optionModel);
            Assert.That(val, Is.EqualTo(0));
        }

        [Test]
        public void Verify_CompareTo_sort_value_returns_minus_1()
        {
            var optionModel = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, 2, "groupName", false, int.MinValue);
            var val = _optionModel.CompareTo(optionModel);
            Assert.That(val, Is.EqualTo(-1));
        }

        [Test]
        public void Verify_CompareTo_min_sort_value_returns_0()
        {
            var optionModel = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, 1, "groupName", false, int.MinValue);
            var optionModelCompare = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, 2, "groupName", false, int.MinValue);
            var val = optionModel.CompareTo(optionModelCompare);
            Assert.That(val, Is.EqualTo(0));
        }

        [Test]
        public void Verify_CompareTo_min_sort_value_returns_1()
        {
            var optionModel = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, 1, "groupName", false, int.MinValue);
            var optionModelCompare = new OptionModel<ColormapType>(
                "displayName", ColormapType.Binary, 2, "groupName", false, 2);
            var val = optionModel.CompareTo(optionModelCompare);
            Assert.That(val, Is.EqualTo(1));
        }

        [Test]
        public void Verify_CreateAvailableOptions_throws_error()
        {
            Assert.Throws<ArgumentException>(() => OptionModel<OpticalProperties>.CreateAvailableOptions(null,
                "groupName", new OpticalProperties(),
                new[] {new OpticalProperties()}, false));
        }
    }
}
