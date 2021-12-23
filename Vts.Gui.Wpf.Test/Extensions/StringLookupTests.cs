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
            Assert.AreEqual("Fwd Solver:", stringLookup);
        }
        [Test]
        public void Verify_method_GetLocalizedString_works_correctly_for_nonexisting_string()
        {
            var stringLookup = StringLookup.GetLocalizedString("nonexistingString");
            Assert.IsEmpty(stringLookup);
        }
    }
}
