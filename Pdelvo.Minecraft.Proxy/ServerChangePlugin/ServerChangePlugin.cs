using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Pdelvo.Minecraft.Protocol.Helper;
using Pdelvo.Minecraft.Protocol.Packets;
using Pdelvo.Minecraft.Proxy.Library;
using Pdelvo.Minecraft.Proxy.Library.Connection;
using Pdelvo.Minecraft.Proxy.Library.Plugins;
using Pdelvo.Minecraft.Proxy.Library.Plugins.Events;

namespace ServerChangePlugin
{
    public class ServerChangePlugin : PluginBase
    {
        ServerChangePacketListener _listener;
        PluginManager _manager;
        public ConcurrentDictionary<IProxyConnection, Tuple<int, int>> EntityIDMapping { get; set; }

        internal ILog Logger = LogManager.GetLogger("Server Change plugin");

        public ServerChangePlugin()
        {
            EntityIDMapping = new ConcurrentDictionary<IProxyConnection, Tuple<int, int>>();
        }

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

        public override void OnConnectionLost(UserEventArgs args)
        {
            //clean up
            TryRemoveConnection(args.Connection);
        }

        public bool TryRemoveConnection(IProxyConnection connection)
        {
            Tuple<int, int> result;
            return EntityIDMapping.TryRemove(connection, out result);
        }
    }

    public class ServerChangePacketListener : IPacketListener
    {
        public ServerChangePlugin Plugin { get; set; }

        Regex endPointRegex = new Regex("^\\[Redirect\\].*?\\:\\s*(?<ip>[a-zA-Z0-9\\.\\-]+)\\s*(\\:\\s*(?<port>[0-9]+))?\\s*;(?<version>[0-9]+)?\\s*$");

        public ServerChangePacketListener(ServerChangePlugin plugin)
        {
            Plugin = plugin;
        }

        public void ClientPacketReceived(PacketReceivedEventArgs e)
        {
            ApplyClientEntityIDFixes(e);
        }
        public async void ServerPacketReceived(PacketReceivedEventArgs e)
        {
            var packet = e.Packet as DisconnectPacket;
            if (packet != null)
            {
                var result = endPointRegex.Match(packet.Reason);

                if (result.Success)
                {
                    e.Handled = true;
                    var version = 0;
                    if (result.Groups["version"].Success)
                        version = int.Parse(result.Groups["version"].Value);
                    RemoteServerInfo info = new RemoteServerInfo(result.ToString(), 
                        new IPEndPoint((await Dns.GetHostEntryAsync(result.Groups["ip"].Value)).AddressList[0], result.Groups["port"].Success ? int.Parse(result.Groups["port"].Value) : 25565), version);

                    var connection = e.Connection;

                    var pa = await connection.InitializeServerAsync(info);

                    if (pa is DisconnectPacket)
                    {
                        await e.Connection.ClientEndPoint.SendPacketAsync(pa);
                        return;
                    }
                    var logonResponse = pa as LogOnResponse;
                    //Add entity id mapping
                    Plugin.EntityIDMapping.AddOrUpdate(e.Connection,  new Tuple<int, int>(e.Connection.EntityID, logonResponse.EntityId), (a, b) => b);

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

                    Plugin.Logger.InfoFormat("{0} got transferred to {1}", e.Connection.Username, info.EndPoint);
                }
            }

            ApplyServerEntityIDFixes(e);
        }

        private void ApplyServerEntityIDFixes(PacketReceivedEventArgs e)
        {
            Tuple<int, int> mapping;
            if (!Plugin.EntityIDMapping.TryGetValue(e.Connection, out mapping)) return;
            var packet = e.Packet;
            if (packet.Code == 0x05)
            {
                var entityEquipment = (EntityEquipment)packet;
                entityEquipment.EntityId = entityEquipment.EntityId == mapping.Item2 ? mapping.Item1 : entityEquipment.EntityId;
                entityEquipment.Changed = true;
            }
            else if (packet.Code == 0x11)
            {
                var useBed = (UseBed)packet;
                useBed.EntityId = useBed.EntityId == mapping.Item2 ? mapping.Item1 : useBed.EntityId;
                useBed.Changed = true;
            }
            else if (packet.Code == 0x1C)
            {
                var entityVelocity = (EntityVelocity)packet;
                entityVelocity.EntityId = entityVelocity.EntityId == mapping.Item2 ? mapping.Item1 : entityVelocity.EntityId;
                entityVelocity.Changed = true;
            }
            else if (packet.Code == 0x1F)
            {
                var entityRelativeMove = (EntityRelativeMove)packet;
                entityRelativeMove.EntityId = entityRelativeMove.EntityId == mapping.Item2 ? mapping.Item1 : entityRelativeMove.EntityId;
                entityRelativeMove.Changed = true;
            }
            else if (packet.Code == 0x20)
            {
                var entityLook = (EntityLook)packet;
                entityLook.EntityId = entityLook.EntityId == mapping.Item2 ? mapping.Item1 : entityLook.EntityId;
                entityLook.Changed = true;
            }
            else if (packet.Code == 0x21)
            {
                var entityLookAndRelativeMove = (EntityLookAndRelativeMove)packet;
                entityLookAndRelativeMove.EntityId = entityLookAndRelativeMove.EntityId == mapping.Item2 ? mapping.Item1 : entityLookAndRelativeMove.EntityId;
                entityLookAndRelativeMove.Changed = true;
            }
            else if (packet.Code == 0x22)
            {
                var entityTeleport = (EntityTeleport)packet;
                entityTeleport.EntityId = entityTeleport.EntityId == mapping.Item2 ? mapping.Item1 : entityTeleport.EntityId;
                entityTeleport.Changed = true;
            }
            else if (packet.Code == 0x26)
            {
                var entityStatus = (EntityStatus)packet;
                entityStatus.EntityId = entityStatus.EntityId == mapping.Item2 ? mapping.Item1 : entityStatus.EntityId;
                entityStatus.Changed = true;
            }
            else if (packet.Code == 0x27)
            {
                var attachEntity = (AttachEntity)packet;
                attachEntity.EntityId = attachEntity.EntityId == mapping.Item2 ? mapping.Item1 : attachEntity.EntityId;
                attachEntity.Changed = true;
            }
            else if (packet.Code == 0x28)
            {
                var entityMetadata = (EntityMetadata)packet;
                entityMetadata.EntityId = entityMetadata.EntityId == mapping.Item2 ? mapping.Item1 : entityMetadata.EntityId;
                entityMetadata.Changed = true;
            }
            else if (packet.Code == 0x29)
            {
                var entityEffect = (EntityEffect)packet;
                entityEffect.EntityId = entityEffect.EntityId == mapping.Item2 ? mapping.Item1 : entityEffect.EntityId;
                entityEffect.Changed = true;
            }
            else if (packet.Code == 0x2A)
            {
                var removeEntityEffect = (RemoveEntityEffect)packet;
                removeEntityEffect.EntityId = removeEntityEffect.EntityId == mapping.Item2 ? mapping.Item1 : removeEntityEffect.EntityId;
                removeEntityEffect.Changed = true;
            }
        }

        private void ApplyClientEntityIDFixes(PacketReceivedEventArgs e)
        {
            Tuple<int, int> mapping;
            if (!Plugin.EntityIDMapping.TryGetValue(e.Connection, out mapping)) return;
            var packet = e.Packet;
            if (packet.Code == 0x05)
            {
                var entityEquipment = (EntityEquipment)packet;
                entityEquipment.EntityId = entityEquipment.EntityId == mapping.Item1 ? mapping.Item2 : entityEquipment.EntityId;
                entityEquipment.Changed = true;
            }
            else if (packet.Code == 0x05)
            {
                var useEntity = (UseEntity)packet;
                useEntity.UserEntity = useEntity.UserEntity == mapping.Item1 ? mapping.Item2 : useEntity.UserEntity;
                useEntity.Changed = true;
            }
            else if (packet.Code == 0x1C)
            {
                var entityVelocity = (EntityVelocity)packet;
                entityVelocity.EntityId = entityVelocity.EntityId == mapping.Item1 ? mapping.Item2 : entityVelocity.EntityId;
                entityVelocity.Changed = true;
            }
            else if (packet.Code == 0x1F)
            {
                var entityRelativeMove = (EntityRelativeMove)packet;
                entityRelativeMove.EntityId = entityRelativeMove.EntityId == mapping.Item1 ? mapping.Item2 : entityRelativeMove.EntityId;
                entityRelativeMove.Changed = true;
            }
            else if (packet.Code == 0x20)
            {
                var entityLook = (EntityLook)packet;
                entityLook.EntityId = entityLook.EntityId == mapping.Item1 ? mapping.Item2 : entityLook.EntityId;
                entityLook.Changed = true;
            }
            else if (packet.Code == 0x21)
            {
                var entityLookAndRelativeMove = (EntityLookAndRelativeMove)packet;
                entityLookAndRelativeMove.EntityId = entityLookAndRelativeMove.EntityId == mapping.Item1 ? mapping.Item2 : entityLookAndRelativeMove.EntityId;
                entityLookAndRelativeMove.Changed = true;
            }
            else if (packet.Code == 0x22)
            {
                var entityTeleport = (EntityTeleport)packet;
                entityTeleport.EntityId = entityTeleport.EntityId == mapping.Item1 ? mapping.Item2 : entityTeleport.EntityId;
                entityTeleport.Changed = true;
            }
            else if (packet.Code == 0x26)
            {
                var entityStatus = (EntityStatus)packet;
                entityStatus.EntityId = entityStatus.EntityId == mapping.Item1 ? mapping.Item2 : entityStatus.EntityId;
                entityStatus.Changed = true;
            }
            else if (packet.Code == 0x27)
            {
                var attachEntity = (AttachEntity)packet;
                attachEntity.EntityId = attachEntity.EntityId == mapping.Item1 ? mapping.Item2 : attachEntity.EntityId;
                attachEntity.Changed = true;
            }
            else if (packet.Code == 0x28)
            {
                var entityMetadata = (EntityMetadata)packet;
                entityMetadata.EntityId = entityMetadata.EntityId == mapping.Item1 ? mapping.Item2 : entityMetadata.EntityId;
                entityMetadata.Changed = true;
            }
            else if (packet.Code == 0x29)
            {
                var entityEffect = (EntityEffect)packet;
                entityEffect.EntityId = entityEffect.EntityId == mapping.Item1 ? mapping.Item2 : entityEffect.EntityId;
                entityEffect.Changed = true;
            }
            else if (packet.Code == 0x2A)
            {
                var removeEntityEffect = (RemoveEntityEffect)packet;
                removeEntityEffect.EntityId = removeEntityEffect.EntityId == mapping.Item1 ? mapping.Item2 : removeEntityEffect.EntityId;
                removeEntityEffect.Changed = true;
            }
        }
    }

}
