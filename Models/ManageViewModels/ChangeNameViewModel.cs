using System.ComponentModel.DataAnnotations;

namespace ToDoAndNotes3.Models.ManageViewModels
{
    public class ChangeNameViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string OldName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [Display(Name = "Name")]
        public string NewName { get; set; }
    }
}
