using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.WPF.ViewModels
{
    public class MainViewModel
    {
        public PVChatViewModel PVChatViewModel { get; }

        public MainViewModel(PVChatViewModel chatViewModel)
        {
            PVChatViewModel = chatViewModel;
        }
    }
}
