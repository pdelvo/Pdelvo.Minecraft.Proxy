using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pdelvo.Minecraft.Proxy.Library;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Tests
{
    [TestClass]
    public class HelperTests
    {
        [TestMethod]
        public void SessionTest()
        {
            var hash = Session.GetSessionHash();

            var hash2 = Session.GetSessionHash();

            Assert.AreNotEqual(hash, hash2);

            var lng = Convert.ToInt64(hash, 16);
            var lng2 = Convert.ToInt64(hash2, 16);

            Assert.IsTrue(lng >= 0);
            Assert.IsTrue(lng2 >= 0);
        }
        [TestMethod]
        public async Task UserAccountTest()
        {
            Assert.IsFalse(await UserAccountServices.CheckAccountAsync("thisdoesnotexist", "acoolhash"));
        }
    }
}
