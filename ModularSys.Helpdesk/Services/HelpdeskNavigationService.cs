using System;

namespace ModularSys.Helpdesk.Services
{
    public class HelpdeskNavigationService
    {
        public string CurrentModule { get; private set; } = "dashboard";
        public event Action? OnChange;

        public void SelectModule(string module)
        {
            CurrentModule = module;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
