using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Test.Extensions
{
    /// <summary>
    /// Tests StringLookup classe
    /// </summary>
    [TestFixture]
    public class StringLookuplTests
    {
        /// <summary>
        /// Verifies method GetLocalizedString works properly for string in Resources/Strings.resx
        /// </summary>
        [Test]
        public void verify_method_GetLocalizedString_works_correctly_for_existing_string()
        {
            var stringLookup = StringLookup.GetLocalizedString("Label_ForwardSolver");
            Assert.AreEqual(stringLookup, "Fwd Solver:");
        }
        [Test]
        public void verify_method_GetLocalizedString_works_correctly_for_nonexisting_string()
        {
            var stringLookup = StringLookup.GetLocalizedString("nonexistingString");
            Assert.IsEmpty(stringLookup);
        }
    }
}
