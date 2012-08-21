using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library
{
    /// <summary>
    /// Provides functionality to generate connection session hashes which can be used within the minecraft login process
    /// </summary>
    public static class Session
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Returns a valid minecraft session hash
        /// </summary>
        /// <returns>A valid minecraft session hash</returns>
        public static string GetSessionHash()
        {
            var buffer = new byte[4];
            Random.NextBytes(buffer);
            //buffer = MD5.Create().ComputeHash(buffer);
            buffer[0] = (byte)(buffer[0] % 128);
            return BitConverter.ToString(buffer).Replace("-", "").ToLower();
        }
    }
}
