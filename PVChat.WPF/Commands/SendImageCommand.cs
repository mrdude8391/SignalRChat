using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.WPF.Commands
{
    public class SendImageCommand : CommandBase
    {
        private readonly PVChatViewModel _viewModel;
        private readonly ISignalRChatService _chatService;
        private readonly DialogService _dialogService;

        public SendImageCommand(PVChatViewModel viewModel, ISignalRChatService chatService)
        {
            _viewModel = viewModel;
            _chatService = chatService;
            _dialogService = new DialogService();
        }
        public override async void Execute(object parameter)
        {
            var pic = _dialogService.OpenFile("Select image file", "Images (*.jpg;*.png)|*.jpg;*.png");
            if (string.IsNullOrEmpty(pic)) return;

            var img = await Task.Run(() => File.ReadAllBytes(pic));

            try
            {
                var recepient = _viewModel.SelectedParticipant.Name;
                _viewModel.Image = img;
                if (string.IsNullOrEmpty(_viewModel.Message))
                {
                    _viewModel.Message = " ";
                }
                // _chatService.SendImage(recepient, img);
            }
            catch (Exception)
            {

                return;
            }
            finally
            {
                
            }
        }
    }
}
