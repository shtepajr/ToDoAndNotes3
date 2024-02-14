namespace ToDoAndNotes3.Models.MainViewModels
{
    public class NoteLabelsViewModel
    {
        public Note Note { get; set; } = new Note();
        public List<Label> Labels { get; set; } = new List<Label>();
        public List<Project> Projects { get; set; } = new List<Project>();
        public string? SelectedLabelsId { get; set; }
    }
}
