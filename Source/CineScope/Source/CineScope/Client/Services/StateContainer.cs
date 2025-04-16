using System;

namespace CineScope.Client.Services
{
    public class StateContainer
    {
        private string _userProfilePicture = "default.svg";
        public string UserProfilePicture
        {
            get => _userProfilePicture;
            set
            {
                _userProfilePicture = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
} 