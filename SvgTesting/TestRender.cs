using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using System.IO;
using System.Drawing.Imaging;

namespace Svg
{
    [TestClass]
    public class TestRender : TestBase
    {
        [TestMethod]
        public void TestRenderRect()
        {
            var doc = new SvgBuilder().OpenPath("rect.svg");
            using (var bmp = new Bitmap(800, 800))
            {
                var render = SvgRenderer.FromImage(bmp);
                doc.RenderElement(render);
            }
        }

        const string NASALIZA = "Nasalization";
        [TestMethod]
        public void TestRenderCustomFont()
        {
            var builder = new SvgBuilder();
            builder.FontFamilyLookup += TestRenderCustomFont_FontFamilyLookup;
            var doc = new SvgDocument { SvgBuilder = builder };
            var text = new SvgText("Hello World") { SvgBuilder = builder };
            doc.Children.Add(text);
            text.Font = NASALIZA;
            text.FontSize = new SvgUnit(22);
            text.Y = new SvgUnit(100);
            Assert.AreEqual(text.Font, NASALIZA);
            SaveBitmap(doc.Draw(), "TestRenderCustomFont.png");
        }

        void TestRenderCustomFont_FontFamilyLookup(object sender, FontFamilyLookupArgs e)
        {
            if (e.Name == NASALIZA)
            {
                var content = TestingSources.NASALIZA;
                // pin array so we can get its address
                var handle = GCHandle.Alloc(content, GCHandleType.Pinned);
                try
                {
                    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(content, 0);
                    var fontCollection = new PrivateFontCollection();
                    fontCollection.AddMemoryFont(ptr, content.Length);
                    e.FontFamily = fontCollection.Families[0];
                }
                finally
                {
                    // don't forget to unpin the array!
                    handle.Free();
                }
            }
        }
    }
}
