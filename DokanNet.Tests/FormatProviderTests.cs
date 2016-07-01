using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokanNet.Tests
{
    [TestClass]
    public sealed class FormatProviderTests
    {
        [TestMethod, TestCategory(TestCategories.Success)]
        public void Null_values_should_be_visibles()
        {
            DateTime? obj = null;
            Assert.AreEqual(FormatProviders.NullStringRapresentation, String.Format(FormatProviders.DefaultFormatProvider, "{0}", obj));
        }
    }
}
