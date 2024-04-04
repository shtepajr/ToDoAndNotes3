namespace ToDoAndNotes3.Models
{
    public class NoteDescription
    {
        public int? NoteDescriptionId { get; set; }
        public int? NoteId { get; set; }
        public Note? Note { get; set; }
        public string? Description { get; set; } = default!;
    }
}
