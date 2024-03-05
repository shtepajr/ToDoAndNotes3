using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace ToDoAndNotes3.Models.ManageViewModels
{
    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        public IList<AuthenticationScheme> OtherLogins { get; set; }
    }

}