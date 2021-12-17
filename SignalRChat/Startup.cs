using System;
using System.Threading.Tasks;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(SignalRChat.Startup))]

namespace SignalRChat
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.UseCors(CorsOptions.AllowAll);
            var hubConfig = new HubConfiguration();
            hubConfig.EnableJavaScriptProxies = false;
            app.MapSignalR("/pvchat", hubConfig);
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
        }
    }
}
