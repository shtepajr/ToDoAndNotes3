namespace ToDoAndNotes3.Models
{
    public class Note
    {
        public int NoteId { get; set; }
        public int ProjectId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; } = default!;
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public ICollection<NoteLabel>? NoteLabels { get; set; }
    }
}
