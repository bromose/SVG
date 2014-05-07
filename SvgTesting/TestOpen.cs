using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Svg
{
    [TestClass]
    public class TestOpen:TestBase
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
        [TestMethod]
        public void TestParseAddFragment()
        {
            var doc = new SvgDocument();
            var group = doc.Children.Add(TestingSources.SvgFragmentGroup);
            SaveText(doc.ToString(), "ParseAddFragment.svg");
        }
    }
}
