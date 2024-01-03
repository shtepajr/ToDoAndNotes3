namespace ToDoAndNotes3.Models
{
    public class Label
    {
        public int? LabelId { get; set; }
        public string? UserId { get; set; }
        public string? Title { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<NoteLabel>? NoteLabels { get; set; }
        public ICollection<TaskLabel>? TaskLabels { get; set; }
    }
}
