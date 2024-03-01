using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ToDoAndNotes3.Models
{
    public class User : IdentityUser
    {
        [PersonalData]
        public string? CustomName { get; set; }
    }
}
