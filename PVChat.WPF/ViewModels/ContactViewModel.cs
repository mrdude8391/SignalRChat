using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PVChat.WPF.ViewModels
{
    public class ContactViewModel : ViewModelBase
    {
        public ObservableCollection<ParticipantModel> Participants { get; } = new ObservableCollection<ParticipantModel>();

        private ParticipantModel _selectedParticipant;

        public ParticipantModel SelectedParticipant
        {
            get { return _selectedParticipant; }
            set
            {
                _selectedParticipant = value;
                OnPropertyChanged(nameof(SelectedParticipant));
                OpenChat(SelectedParticipant);
            }
        }

        private List<UserModel> _users;

        public List<UserModel> Users
        {
            get { return _users; }
            set { _users = value; }
        }


        private readonly ISignalRChatService _chatService;
        private readonly NavigationService _navService;

      
        public ContactViewModel(ISignalRChatService chatService, NavigationService navService, List<UserModel> Users)
        {
            _chatService = chatService;
            _navService = navService;
            _users = Users;

            _chatService.LoggedIn += OtherUserLoggedIn;
            _chatService.ParticipantLogout += OtherUserLoggedOut;

            Users.ForEach(u => Participants.Add(new ParticipantModel { Name = u.Name }));
        }
        private void OpenChat(ParticipantModel selectedParticipant)
        {
            if(selectedParticipant != null)
            {
            //_navService.CurrentViewModel = new PVChatViewModel(_chatService, _navService, this);
            }

        }

        private void OtherUserLoggedIn(UserModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Participants.Add(new ParticipantModel
                {
                    Name = user.Name,
                });

                Users.Add(new UserModel
                {
                    Name = user.Name,
                    Id = user.Id,
                });

            });
        }

        private void OtherUserLoggedOut(UserModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Participants.Remove(Participants.Where(o => o.Name == user.Name).SingleOrDefault());

                Users.Remove(Users.Where(o => o.Name == user.Name).SingleOrDefault());

            });
        }
    }
}