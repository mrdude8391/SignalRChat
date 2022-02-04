using PVChat.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.WPF.Commands
{
    public class ClearImageCommand : CommandBase
    {
        private readonly PVChatViewModel _viewModel;
        

        public ClearImageCommand(PVChatViewModel viewModel)
        {
            _viewModel = viewModel;
            
        }

        public override void Execute(object parameter)
        {
            try
            {
                _viewModel.Image = null;
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
