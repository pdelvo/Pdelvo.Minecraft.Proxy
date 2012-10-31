using System.ComponentModel;
using System.Configuration.Install;

namespace Pdelvo.Minecraft.Proxy.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent ();
        }
    }
}