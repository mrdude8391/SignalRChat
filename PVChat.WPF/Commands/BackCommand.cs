using PVChat.Domain.Services;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;

namespace PVChat.WPF.Commands
{
    public class BackCommand : CommandBase
    {
        
        private readonly NavigationService _navService;
        private readonly ContactViewModel _viewModel;

        public BackCommand(NavigationService navService, ContactViewModel viewModel)
        {
           
            _navService = navService;
            _viewModel = viewModel;
        }

        public override void Execute(object parameter)
        {
            _navService.CurrentViewModel = _viewModel;
            _viewModel.SelectedParticipant = null;
        }
    }
}