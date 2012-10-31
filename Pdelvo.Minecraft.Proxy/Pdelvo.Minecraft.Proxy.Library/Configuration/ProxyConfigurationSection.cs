using System;
using System.Configuration;

namespace Pdelvo.Minecraft.Proxy.Library.Configuration
{
    /// <summary>
    ///   A xml configuration section to configure the proxy server
    /// </summary>
    public class ProxyConfigurationSection : ConfigurationSection
    {
        private static readonly ProxyConfigurationSection settings
            = ConfigurationManager.GetSection("proxy") as ProxyConfigurationSection;

        /// <summary>
        ///   A singleton to access the current settings
        /// </summary>
        public static ProxyConfigurationSection Settings
        {
            get { return settings; }
        }

        /// <summary>
        ///   The message of the day of the server
        /// </summary>
        [ConfigurationProperty("motd", DefaultValue = "Proxy Server", IsRequired = false)]
        public string Motd
        {
            get { return (string) this["motd"]; }
            set { this["motd"] = value; }
        }

        /// <summary>
        ///   The local end point the proxy server should bind to
        /// </summary>
        [ConfigurationProperty("localEndPoint", DefaultValue = "0.0.0.0:25565", IsRequired = false)]
        public string LocalEndPoint
        {
            get { return (string) this["localEndPoint"]; }
            set { this["localEndPoint"] = value; }
        }

        /// <summary>
        ///   the maximum users the proxy server should allow to join
        /// </summary>
        [ConfigurationProperty("maxPlayers", DefaultValue = "100", IsRequired = false)]
        public int MaxPlayers
        {
            get { return (int) this["maxPlayers"]; }
            set { this["maxPlayers"] = value; }
        }

        /// <summary>
        ///   true if the server should use the online mode, otherwise false
        /// </summary>
        [ConfigurationProperty("onlineMode", DefaultValue = true, IsRequired = false)]
        public bool OnlineMode
        {
            get { return (bool) this["onlineMode"]; }
            set { this["onlineMode"] = value; }
        }

        /// <summary>
        ///   The list of the backend servers
        /// </summary>
        [ConfigurationProperty("server", IsDefaultCollection = false)]
        public ServerCollection Server
        {
            get { return (ServerCollection) this["server"]; }
            set { this["server"] = value; }
        }
    }

    /// <summary>
    ///   The collection of minecraft backend servers
    /// </summary>
    public class ServerCollection : ConfigurationElementCollection
    {
        /// <summary>
        ///   The type of the collection, always AddRemoveClearMap
        /// </summary>
        public override
            ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return
                    ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        /// <summary>
        ///   The name of the add element xml node. Default is 'add'
        /// </summary>
        public new string AddElementName
        {
            get { return base.AddElementName; }

            set { base.AddElementName = value; }
        }

        /// <summary>
        ///   The name of the clear element xml node. Default is 'clear'
        /// </summary>
        public new string ClearElementName
        {
            get { return base.ClearElementName; }

            set { base.AddElementName = value; }
        }

        /// <summary>
        ///   The name of the remove element xml node. Default is 'remove'
        /// </summary>
        public new string RemoveElementName
        {
            get { return base.RemoveElementName; }
        }

        /// <summary>
        ///   Get the current number of elements
        /// </summary>
        public new int Count
        {
            get { return base.Count; }
        }

        /// <summary>
        ///   Returns the ServerElement at the specific position
        /// </summary>
        /// <param name="index"> The position of the ServerElement in this collection </param>
        /// <returns> The ServerElement at index </returns>
        public ServerElement this[int index]
        {
            get { return (ServerElement) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        /// <summary>
        ///   Returns the ServerElement with the specific name
        /// </summary>
        /// <param name="name"> The name of the ServerElement </param>
        /// <returns> The ServerElement with the specific name </returns>
        public new ServerElement this[string name]
        {
            get { return (ServerElement) BaseGet(name); }
        }

        /// <summary>
        ///   Creates a new empty ServerElement (same as using the default constructor)
        /// </summary>
        /// <returns> A new instance of the ServerElement class </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerElement ();
        }


        /// <summary>
        ///   cCeates a new empty ServerElement with the specific name
        /// </summary>
        /// <param name="elementName"> The name of the backend server </param>
        /// <returns> A new instance of the ServerElement class </returns>
        protected override
            ConfigurationElement CreateNewElement(
            string elementName)
        {
            return new ServerElement {Name = elementName};
        }

        /// <summary>
        ///   Returns the position of the specific ServerElement
        /// </summary>
        /// <param name="element"> The element whish should be looked up </param>
        /// <returns> The index of the element </returns>
        public int IndexOf(ServerElement element)
        {
            return BaseIndexOf(element);
        }

        /// <summary>
        ///   Add a new element to the collection. If there is already a element with the same name in the collection it will be overridden
        /// </summary>
        /// <param name="element"> </param>
        public void Add(ServerElement element)
        {
            BaseAdd(element);

            // Add custom code here.
        }


        /// <summary>
        ///   Add a new element to the collection. If there is already a element with the same name in the collection it will be overridden
        /// </summary>
        /// <param name="element"> </param>
        protected override void
            BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
            // Add custom code here.
        }

        /// <summary>
        ///   Remove a element from the collection
        /// </summary>
        /// <param name="element"> The element which should be removed </param>
        public void Remove(ServerElement element)
        {
            if (BaseIndexOf(element) >= 0)
                BaseRemove(element.Name);
        }

        /// <summary>
        ///   Remove a element at the specific position
        /// </summary>
        /// <param name="index"> The position of the item </param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        /// <summary>
        ///   Remove a item with the specific name
        /// </summary>
        /// <param name="name"> The name of the item </param>
        public void Remove(string name)
        {
            BaseRemove(name);
        }

        /// <summary>
        ///   Removes all items from the collection
        /// </summary>
        public void Clear()
        {
            BaseClear ();
            // Add custom code here.
        }

        /// <summary>
        ///   Get the key = the name of a item
        /// </summary>
        /// <param name="element"> </param>
        /// <returns> </returns>
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((ServerElement) element).Name;
        }
    }

    /// <summary>
    ///   A element in the backend server list
    /// </summary>
    public class ServerElement : ConfigurationElement
    {
        /// <summary>
        ///   The name of the server
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        ///   The remote end point of the server
        /// </summary>
        [ConfigurationProperty("endPoint", IsRequired = true)]
        public string EndPoint
        {
            get { return (string) this["endPoint"]; }
            set { this["endPoint"] = value; }
        }

        /// <summary>
        ///   true if the server is the default server a client should connect to, otherwise false
        /// </summary>
        [ConfigurationProperty("isDefault", DefaultValue = false)]
        public bool IsDefault
        {
            get { return (bool) this["isDefault"]; }
            set { this["isDefault"] = value; }
        }

        /// <summary>
        ///   The DNS name of the server. With that the server can provide different servers for different sub domains, or top level domains
        /// </summary>
        [ConfigurationProperty("dnsName", DefaultValue = null)]
        public string DnsName
        {
            get { return (string) this["dnsName"]; }
            set { this["dnsName"] = value; }
        }

        /// <summary>
        ///   The minecraft version of the server. 1.3 is currently minecraft version 39
        /// </summary>
        [ConfigurationProperty("minecraftVersion", DefaultValue = null, IsRequired = true)]
        public int MinecraftVersion
        {
            get { return (int) this["minecraftVersion"]; }
            set { this["minecraftVersion"] = value; }
        }
    }
}