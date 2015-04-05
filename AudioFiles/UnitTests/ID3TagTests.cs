using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlaylistManager.ApplicationServices;

namespace PlaylistManager.ApplicationServices.UnitTests
{
    [TestClass]
    public class ID3TagTests
    {
        private readonly AudioFile _testFile;
        public ID3TagTests()
        {
            _testFile = new AudioFile(@"D:\test.mp3");
        }

        [TestMethod]
        public void EditTagTests()
        {
            _testFile.EditId3Tag("Title", "Test");
            _testFile.EditId3Tag("Artist", "Test");

            Assert.AreEqual("Test", _testFile.Tag.Artist);
            Assert.AreEqual("Test", _testFile.Tag.Artist);
        }

    }
}
