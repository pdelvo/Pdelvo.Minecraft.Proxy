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
        public void VerifySessionGeneratesRandomOutput()
        {
            for (int i = 0; i < 10; i++)
            {
                var hash = Session.GetSessionHash();

                var hash2 = Session.GetSessionHash();

                Assert.AreNotEqual(hash, hash2);
            }
        }

        [TestMethod]
        public void VerifySessionGeneratesCorrectOutput()
        {
            for (int i = 0; i < 1000; i++)
            {
                var hash = Session.GetSessionHash();

                var hash2 = Session.GetSessionHash();
                var lng = Convert.ToInt64(hash, 16);
                var lng2 = Convert.ToInt64(hash2, 16);

                Assert.IsTrue(lng >= 0);
                Assert.IsTrue(lng2 >= 0);
            }
        }

        [TestMethod]
        public async Task VerifyUserAccountCheckFailsOnWrongDetails()
        {
            Assert.IsTrue(await UserAccountServices.CheckAccountAsync("thisdoesnotexist", "acoolhash", useDefaultProxySettings: false) == false);
        }
    }
}
