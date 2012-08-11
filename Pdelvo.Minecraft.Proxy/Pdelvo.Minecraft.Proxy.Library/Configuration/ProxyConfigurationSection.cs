using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pdelvo.Minecraft.Proxy.Library.Configuration
{
    public class ProxyConfigurationSection : ConfigurationSection
    {
        private static ProxyConfigurationSection settings
              = ConfigurationManager.GetSection("proxy") as ProxyConfigurationSection;

    public static ProxyConfigurationSection Settings
        {
            get
            {
                return settings;
            }
        }

        [ConfigurationProperty("motd", DefaultValue = "Proxy Server", IsRequired = false)]
        public string Motd
        {
            get
            {
                return (string)this["motd"];
            }
            set
            {
                this["motd"] = value;
            }
        }

        [ConfigurationProperty("localEndPoint", DefaultValue = "0.0.0.0:25565", IsRequired = false)]
        public string LocalEndPoint
        {
            get
            {
                return (string)this["localEndPoint"];
            }
            set
            {
                this["localEndPoint"] = value;
            }
        }

        [ConfigurationProperty("maxPlayers", DefaultValue = "100", IsRequired = false)]
        public int MaxPlayers
        {
            get
            {
                return (int)this["maxPlayers"];
            }
            set
            {
                this["maxPlayers"] = value;
            }
        }

        [ConfigurationProperty("server", IsDefaultCollection = false)]
        public ServerCollection Server
        {
            get
            {
                return (ServerCollection)this["server"];
            }
            set
            {
                this["server"] = value;
            }
        }
    }
    public class ServerCollection : ConfigurationElementCollection
    {
        public ServerCollection()
        {
        }

        public override
            ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return
                    ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerElement();
        }

        protected override
            ConfigurationElement CreateNewElement(
            string elementName)
        {
            return new ServerElement { Name = elementName };
        }




        public new string AddElementName
        {
            get
            { return base.AddElementName; }

            set
            { base.AddElementName = value; }
        }



        public new string ClearElementName
        {
            get
            {
                return base.ClearElementName;
            }

            set
            {
                base.AddElementName = value;
            }
        }


        public new string RemoveElementName
        {
            get
            {
                return base.RemoveElementName;
            }
        }



        public new int Count
        {

            get
            {
                return base.Count;
            }
        }



        public ServerElement this[int index]
        {
            get
            {
                return (ServerElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }


        new public ServerElement this[string Name]
        {
            get
            {
                return (ServerElement)BaseGet(Name);
            }
        }


        public int IndexOf(ServerElement url)
        {
            return BaseIndexOf(url);
        }

        public void Add(ServerElement url)
        {
            BaseAdd(url);

            // Add custom code here.
        }

        protected override void
            BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
            // Add custom code here.
        }

        public void Remove(ServerElement url)
        {
            if (BaseIndexOf(url) >= 0)
                BaseRemove(url.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
            // Add custom code here.
        }
        protected override Object
          GetElementKey(ConfigurationElement element)
        {
            return ((ServerElement)element).Name;
        }
    }
    public class ServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("endPoint", IsRequired = true)]
        public string EndPoint
        {
            get
            {
                return (string)this["endPoint"];
            }
            set
            {
                this["endPoint"] = value;
            }
        }

        [ConfigurationProperty("isDefault", DefaultValue = false)]
        public bool IsDefault
        {
            get
            {
                return (bool)this["isDefault"];
            }
            set
            {
                this["isDefault"] = value;
            }
        }

        [ConfigurationProperty("dnsName", DefaultValue = null)]
        public string DnsName
        {
            get
            {
                return (string)this["dnsName"];
            }
            set
            {
                this["dnsName"] = value;
            }
        }

        [ConfigurationProperty("minecraftVersion", DefaultValue = null, IsRequired = true)]
        public int MinecraftVersion
        {
            get
            {
                return (int)this["minecraftVersion"];
            }
            set
            {
                this["minecraftVersion"] = value;
            }
        }
    }
}
