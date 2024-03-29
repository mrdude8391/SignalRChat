﻿using Microsoft.AspNet.SignalR.Client;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PVChat.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        NotificationService notifService = new NotificationService();

        MainWindow window { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            HubConnection connection = new HubConnection("http://localhost:9999/pvchat", false);
            SignalRChatService chatService = new SignalRChatService(connection);
            NavigationService navService = new NavigationService();

            navService.CurrentViewModel = new LoginViewModel(navService, chatService, notifService);

            window = new MainWindow
            {
                DataContext = new MainViewModel(navService, chatService, notifService)
            };
            window.Show();

            
            
        }

        // enables and disables windows notifications depending on if the chat client is open or not
        protected override void OnActivated(EventArgs e)
        {
            notifService.IsFocused = true;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            notifService.IsFocused = false;
        }

        

    }
}
