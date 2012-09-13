using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library
{
    /// <summary>
    /// Provides access to the minecraft session apis to validate user accounts
    /// </summary>
    public static class UserAccountServices
    {
        /// <summary>
        /// Checks if a user account is valid  using the user name and the connection Hash
        /// </summary>
        /// <param name="username">The name of the user</param>
        /// <param name="hash">The calculated connection hash. See http://wiki.vg/Authentication#Server_operation for more information</param>
        /// <param name="useDefaultProxySettings">If this value is true the proxy will search for default proxy details</param>
        /// <returns>A task representing the result of this operation. true if the user account is valid, otherwise false</returns>
        public static async Task<bool?> CheckAccountAsync(string username, string hash, bool useDefaultProxySettings = true)
        {
            return await CheckAccountAsync(username, hash, new HttpClientHandler { UseProxy = useDefaultProxySettings });
        }

        /// <summary>
        /// Checks if a user account is valid  using the user name and the connection Hash
        /// </summary>
        /// <param name="username">The name of the user</param>
        /// <param name="hash">The calculated connection hash. See http://wiki.vg/Authentication#Server_operation for more information</param>
        /// <param name="clientHandler">Specify advanced options on how the request should be sent</param>
        /// <returns>A task representing the result of this operation. true if the user account is valid, otherwise false</returns>
        public static async Task<bool?> CheckAccountAsync(string username, string hash, HttpClientHandler clientHandler)
        {
            string url = "https://session.minecraft.net/game/checkserver.jsp?user={0}&serverId={1}";
            url = String.Format(url, Uri.EscapeDataString(username), Uri.EscapeDataString(hash));
            var client = new HttpClient(clientHandler);
            var result = await client.GetAsync(url);
            result.EnsureSuccessStatusCode();
            var textResult = await result.Content.ReadAsStringAsync();
            if (textResult == "YES") return true;
            else if (textResult == "NO") return false;
            return null;
        }
    }
}
