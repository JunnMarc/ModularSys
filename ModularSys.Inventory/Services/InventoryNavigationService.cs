namespace ModularSys.Inventory.Services
{
    public class InventoryNavigationService
    {
        public event Action<string>? OnModuleChanged;
        
        private string _selectedModule = "dashboard";
        
        public string SelectedModule
        {
            get => _selectedModule;
            set
            {
                if (_selectedModule != value)
                {
                    _selectedModule = value;
                    OnModuleChanged?.Invoke(_selectedModule);
                }
            }
        }
        
        public void SelectModule(string module)
        {
            SelectedModule = module;
        }
    }
}
