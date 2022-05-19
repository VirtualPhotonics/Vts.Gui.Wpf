﻿using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Resources;

namespace Vts.Gui.Wpf.Test.Extensions
{
    [TestFixture]
    internal class LocalizedStringsTests
    {
        [Test]
        public void Verify_get_localized_string()
        {
            var localizedStrings = new LocalizedStrings();
            var strings = localizedStrings.MainResource;
            Assert.IsInstanceOf<Strings>(strings);
            Assert.AreEqual("Cancel", Strings.Button_Cancel);
        }
    }
}
