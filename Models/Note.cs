using System.ComponentModel.DataAnnotations;

namespace ToDoAndNotes3.Models
{
    public class Note
    {
        public int? NoteId { get; set; }
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? ShortDescription { get; set; } = default!;
        public NoteDescription? NoteDescription { get; set; } = new NoteDescription();
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        [DataType(DataType.Date)]
        public DateOnly? DueDate { get; set; }
        [DataType(DataType.Time)]
        public TimeOnly? DueTime { get; set; }
        public ICollection<NoteLabel>? NoteLabels { get; set; }
    }
}
