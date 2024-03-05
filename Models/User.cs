using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ToDoAndNotes3.Models
{
    public class User : IdentityUser
    {
        [PersonalData]
        public string? CustomName { get; set; }
        public ICollection<Project>? Projects { get; set; }
        public ICollection<Label>? Labels { get; set; }
    }
}
