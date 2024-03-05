using System.ComponentModel.DataAnnotations;

namespace ToDoAndNotes3.Models
{
    public class Project
    {
        public int? ProjectId { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string? Title { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        public ICollection<Task>? Tasks { get; set; } = new List<Task>();
        public ICollection<Note>? Notes { get; set; } = new List<Note>();

    }
}
