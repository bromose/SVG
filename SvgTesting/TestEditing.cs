using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Svg
{
    [TestClass]
    public class TestEditing : TestBase
    {
        [TestMethod]
        public void TestOffset()
        {
            var doc = new SvgBuilder().Open(Encoding.UTF8.GetString(TestingSources.Basic_Shapes));
            SaveBitmap(doc.Draw(), "before.png");
            doc.EditOffset(130, 300);
            SaveBitmap(doc.Draw(), "after.png");
        }
        [TestMethod]
        public void TestScale()
        {
            var doc = new SvgBuilder().Open(Encoding.UTF8.GetString(TestingSources.Basic_Shapes));
            SaveBitmap(doc.Draw(), "before.png");
            doc.EditScale(2);
            SaveBitmap(doc.Draw(), "after.png");
        }
    }
}
