using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DokanNet.Tests
{
    [TestClass]
    public sealed class FormatProviderTests
    {
        [TestMethod, TestCategory(TestCategories.Success)]
        public void NullValuesShouldBeVisible()
        {
            DateTime? obj = null;
            Assert.AreEqual(FormatProviders.NullStringRapresentation, string.Format(FormatProviders.DefaultFormatProvider, "{0}", obj));
        }
    }
}