using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Svg
{
    [TestClass]
    public class TestOpen
    {
        [TestMethod]
        public void TestOpenPath()
        {
            new SvgBuilder().OpenPath("Rect.svg");
        }

        [TestMethod]
        public void TestParseFragment()
        {
            var doc = new SvgDocument();
            var group = doc.ParseFragment(TestingSources.SvgFragmentGroup, null);
        }
    }
}
