using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Test.Extensions
{
    /// <summary>
    /// Tests StringLookup classes
    /// </summary>
    [TestFixture]
    public class StringLookupTests
    {
        /// <summary>
        /// Verifies method GetLocalizedString works properly for string in Resources/Strings.resx
        /// </summary>
        [Test]
        public void Verify_method_GetLocalizedString_works_correctly_for_existing_string()
        {
            var stringLookup = StringLookup.GetLocalizedString("Label_FwdSolver");
            Assert.That(stringLookup, Is.EqualTo("Fwd Solver:"));
        }

        [Test]
        public void Verify_method_GetLocalizedString_works_correctly_for_non_existent_string()
        {
            var stringLookup = StringLookup.GetLocalizedString("non_existent_string");
            Assert.That(stringLookup, Is.Empty);
        }

        [Test]
        public void Verify_GetLocalizedString_overload_returns_string()
        {
            var stringLookup = StringLookup.GetLocalizedString("Button", "Cancel");
            Assert.That(stringLookup, Is.InstanceOf<string>());
            Assert.That(stringLookup.Length > 0, Is.True);
        }

        [Test]
        public void Verify_GetLocalizedString_enum_returns_empty_string()
        {
            var stringLookup = AbsorptionWeightingType.Analog.GetLocalizedString();
            Assert.That(stringLookup, Is.Empty);
        }

        [Test]
        public void Verify_GetLocalizedString_enum_returns_string()
        {
            var stringLookup = IndependentVariableAxis.Time.GetLocalizedString();
            Assert.That(stringLookup, Is.InstanceOf<string>());
            Assert.That(stringLookup.Length > 0, Is.True);
        }
    }
}
