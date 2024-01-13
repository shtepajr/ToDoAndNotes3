namespace ToDoAndNotes3.Models
{
    public class NoteLabel
    {
        public int? NoteLabelId { get; set; }
        public Note? Task { get; set; }
        public Label? Label { get; set; }
    }
}
