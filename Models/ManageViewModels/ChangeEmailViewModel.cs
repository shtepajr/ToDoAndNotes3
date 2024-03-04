using System.ComponentModel.DataAnnotations;

namespace ToDoAndNotes3.Models.ManageViewModels
{
    public class ChangeEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string OldEmail { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string NewEmail { get; set; }
    }
}
