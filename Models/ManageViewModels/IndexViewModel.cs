using Microsoft.AspNetCore.Identity;

namespace ToDoAndNotes3.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }

        public bool BrowserRemembered { get; set; }

        public string AuthenticatorKey { get; set; }
    }
}