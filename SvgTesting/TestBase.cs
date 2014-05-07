using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg
{
    public class TestBase
    {
        public TestContext TestContext { get; set; }

        public string SaveBitmap(Bitmap image, string name)
        {
            string saveto = Path.Combine(TestContext.ResultsDirectory, name);
            image.Save(saveto, ImageFormat.Png);
            TestContext.AddResultFile(saveto);
            return saveto;
        }

        public string SaveText(string text, string name)
        {
            string saveto = Path.Combine(TestContext.ResultsDirectory, name);
            File.WriteAllText(saveto, text);
            TestContext.AddResultFile(saveto);
            return saveto;
        }
    }
}
