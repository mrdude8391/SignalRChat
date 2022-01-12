﻿using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;

namespace PVChat.WPF.Commands
{
    public class SendMessageCommand : CommandBase
    {
        private readonly PVChatViewModel _viewModel;
        private readonly ISignalRChatService _chatService;

        public SendMessageCommand(PVChatViewModel viewModel, ISignalRChatService chatService)
        {
            _viewModel = viewModel;
            _chatService = chatService;
        }

        public override async void Execute(object parameter)
        {
            try
            {
                var recepient = _viewModel.SelectedParticipant;
                MessageModel newMessage = new MessageModel
                {
                    //MessageId = Default
                    //SenderId = Get in hub
                    ReceiverId = _viewModel.SelectedParticipant.Id,
                    ReceiverName = _viewModel.SelectedParticipant.Name,
                    Message = _viewModel.Message,
                    //Status = default,
                    //CreatedTime = default
                    //SentTime = not yet
                    //DeliveredTime = not yet
                    IsOriginNative = true,
                };
                //_viewModel.SelectedMessages.Add(newMessage);
                _viewModel.SelectedParticipant.Messages.Add(newMessage);

                _viewModel.ErrorMessage = string.Empty;
                _viewModel.Message = string.Empty;
                await _chatService.SendMessage(recepient, newMessage);
            }
            catch (Exception)
            {
                _viewModel.ErrorMessage = "Unable to send message.";
            }
        }
    }
}