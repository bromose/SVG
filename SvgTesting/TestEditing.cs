﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Linq;

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
        [TestMethod]
        public void TestAddingPath()
        {
            var doc = new SvgDocument();
            var elements = doc.ParseFragment(TestingSources.SvgFragmentGroup);
            var epath = (SvgPath)elements[0].Children[0];
            var path = new SvgPath();
            doc.Children.Add(path);
            path.PathData.Add(epath.Path);
            SaveText(doc.ToString(), "PathDataAddPath.svg");
        }
        [TestMethod]
        public void TestAllChildren()
        {
            var doc = new SvgBuilder().Open(Encoding.UTF8.GetString(TestingSources.Basic_Shapes));
            Assert.AreEqual(1, doc.Children.Count);
            Assert.AreEqual(7, doc.AllChildren().Count());
        }
    }
}
