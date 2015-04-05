using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlaylistManager.ApplicationServices.UnitTests
{
    [TestClass]
    public class AudioFileTests
    {
        public AudioFileTests()
        {
            _testFile = new AudioFile(@"D:\iTunes\Music\Parkway Drive\Deep Blue\03 Sleepwalker.m4a");
        }
        private BaseFile _testFile;

        [TestMethod]
        public void FullPathTest()
        {
            Assert.AreEqual(@"D:\iTunes\Music\Parkway Drive\Deep Blue\03 Sleepwalker.m4a", _testFile.FullPath);
        }

        [TestMethod]
        public void FileNameTest()
        {
            Assert.AreEqual("03 Sleepwalker.m4a", _testFile.FileNameWithExtension);
        }

        [TestMethod]
        public void FileNameWithoutExtensionTest()
        {
            Assert.AreEqual("03 Sleepwalker", _testFile.FileNameWithoutExtension);
        }

        [TestMethod]
        public void ExtensionFormatTest()
        {
            Assert.AreEqual(".m4a", _testFile.Extension);
        }
    }
}
