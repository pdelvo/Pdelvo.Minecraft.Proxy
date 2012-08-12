using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pdelvo.Minecraft.Protocol.Helper;
using Pdelvo.Minecraft.Protocol.Packets;
using Pdelvo.Minecraft.Proxy.Library;
using Pdelvo.Minecraft.Proxy.Library.Plugins;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace ServerChangePlugin
{
    public class ServerChangePlugin : PluginBase
    {
        ServerChangePacketListener _listener;
        PluginManager _manager;

        public override string Name
        {
            get { return "Server Change Plug in"; }
        }

        public override Version Version
        {
            get { return System.Version.Parse("0.0.0.1"); }
        }

        public override string Author
        {
            get { return "pdelvo"; }
        }

        public override void Load(PluginManager manager)
        {
            _manager = manager;
            _listener = new ServerChangePacketListener(this);
            _manager.RegisterPacketListener(_listener);
        }
    }

    public class ServerChangePacketListener : IPacketListener
    {
        public ServerChangePlugin Plugin { get; set; }

        Regex endPointRegex = new Regex("(?<ip>[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3})\\:(?<port>[0-9]+)"); //currently supports ipv4

        public ServerChangePacketListener(ServerChangePlugin plugin)
        {
            Plugin = plugin;
        }

        public void ClientPacketReceived(PacketReceivedEventArgs e)
        {
            // I'm not interested in them
        }
        bool writing = false;
        public async void ServerPacketReceived(PacketReceivedEventArgs e)
        {
            var packet = e.Packet as DisconnectPacket;
            if(writing)
            Console.WriteLine(e.Packet);
            if (packet != null)
            {
                var result = endPointRegex.Match(packet.Reason);

                if (result.Success)
                {
                    writing = true;
                    e.Handled = true;
                    RemoteServerInfo info = new RemoteServerInfo(result.ToString(), 
                        new IPEndPoint(IPAddress.Parse(result.Groups["ip"].Value), int.Parse(result.Groups["port"].Value)), ProtocolInformation.MaxSupportedServerVersion);

                    var connection = e.Connection;

                    var pa = await connection.InitializeServerAsync(info);

                    if (pa is DisconnectPacket)
                    {
                        await e.Connection.ClientEndPoint.SendPacketAsync(pa);
                        return;
                    }
                    var logonResponse = pa as LogOnResponse;

                    var state = new InvalidState { Reason = 2 }; // Stop raining
                    await connection.ClientEndPoint.SendPacketAsync(state);

                    var respawn = new Respawn
                    {
                        World = logonResponse.Dimension == 0 ? -1 : 0,//Force chunk and entity unload on client
                        CreativeMode = (byte)logonResponse.ServerMode,
                        Difficulty = logonResponse.Difficulty,
                        Generator = logonResponse.Generator,//for compatibility
                        MapSeed = logonResponse.MapSeed,//for compatibility
                        WorldHeight = (short)logonResponse.WorldHeight
                    };
                    await connection.ClientEndPoint.SendPacketAsync(respawn);
                    //now send the correct world
                    respawn.World = logonResponse.Dimension;
                    await connection.ClientEndPoint.SendPacketAsync(respawn);
                    await Task.Delay(500); // I don't like this too :(
                    connection.StartServerListening();
                }
            }
        }
    }

}
