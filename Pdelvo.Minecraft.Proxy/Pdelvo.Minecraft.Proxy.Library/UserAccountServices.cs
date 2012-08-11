using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library
{
    public static class UserAccountServices
    {
        public static async Task<bool> CheckAccountAsync(string username, string hash)
        {
            try
            {
                string url = "https://session.minecraft.net/game/checkserver.jsp?user={0}&serverId={1}";
                url = String.Format(url, Uri.EscapeDataString(username), Uri.EscapeDataString(hash));
                var client = new HttpClient();
                var result = await client.GetStringAsync(url);
                return result == "YES";
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
