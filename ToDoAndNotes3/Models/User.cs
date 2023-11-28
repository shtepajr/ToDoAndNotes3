using Microsoft.AspNetCore.Identity;

namespace ToDoAndNotes3.Models
{
    public class User : IdentityUser<int>
    {
        [PersonalData]
        public override int Id { get; set; }
    }
}
