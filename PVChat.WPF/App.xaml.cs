using Microsoft.AspNet.SignalR.Client;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PVChat.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            HubConnection connection = new HubConnection("http://localhost:9999/pvchat", false);
            SignalRChatService chatService = new SignalRChatService(connection);
            NavigationService navService = new NavigationService();

            //PVChatViewModel viewModel = new PVChatViewModel(chatService);
            //LoginViewModel loginViewModel = new LoginViewModel(navService, chatService);

            navService.CurrentViewModel = new LoginViewModel(navService, chatService);

            MainWindow window = new MainWindow
            {
                DataContext = new MainViewModel(navService, chatService)
            };

            window.Show();
        }
        
    }
}
