using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public static class Session
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// Gets the session hash.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
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
