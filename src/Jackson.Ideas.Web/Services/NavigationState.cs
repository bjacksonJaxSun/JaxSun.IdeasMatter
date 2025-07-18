using System;

namespace Jackson.Ideas.Web.Services
{
    public class NavigationState
    {
        public event Action<string>? OnNavigationChanged;
        
        private string _currentPage = "home";
        
        public string CurrentPage => _currentPage;
        
        public void NavigateTo(string page)
        {
            if (_currentPage != page)
            {
                _currentPage = page;
                OnNavigationChanged?.Invoke(page);
            }
        }
        
        public bool IsCurrentPage(string page)
        {
            return _currentPage.Equals(page, StringComparison.OrdinalIgnoreCase);
        }
    }
}