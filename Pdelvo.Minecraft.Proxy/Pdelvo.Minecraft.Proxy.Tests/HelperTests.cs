using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pdelvo.Minecraft.Proxy.Library;

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
                string hash = Session.GetSessionHash ();

                string hash2 = Session.GetSessionHash ();

                Assert.AreNotEqual(hash, hash2);
            }
        }

        [TestMethod]
        public void VerifySessionGeneratesCorrectOutput()
        {
            for (int i = 0; i < 1000; i++)
            {
                string hash = Session.GetSessionHash ();

                string hash2 = Session.GetSessionHash ();
                long lng = Convert.ToInt64(hash, 16);
                long lng2 = Convert.ToInt64(hash2, 16);

                Assert.IsTrue(lng >= 0);
                Assert.IsTrue(lng2 >= 0);
            }
        }

        [TestMethod]
        public async Task VerifyUserAccountCheckFailsOnWrongDetails()
        {
            Assert.IsTrue(
                await
                UserAccountServices.CheckAccountAsync("thisdoesnotexist", "acoolhash", useDefaultProxySettings: false) ==
                false);
        }
    }
}