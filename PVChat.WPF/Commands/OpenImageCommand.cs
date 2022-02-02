using PVChat.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.WPF.Commands
{
    //Command will open images in default image viewer
    public class OpenImageCommand : CommandBase
    {
        private readonly PVChatViewModel _viewModel;

        public OpenImageCommand(PVChatViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void Execute(object parameter)
        {
            try
            {
                var img = (byte[])parameter;
                //if (string.IsNullOrEmpty(img) || !File.Exists(img)) return;
                //Process.Start(img);
            }
            catch (Exception)
            {

                return;
            }
        }
    }
}
