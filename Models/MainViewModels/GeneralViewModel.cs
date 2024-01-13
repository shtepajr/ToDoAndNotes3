namespace ToDoAndNotes3.Models.MainViewModels
{
    public class GeneralViewModel
    {
        public List<Project> Projects { get; set; } = new List<Project>();
        public List<Task> Tasks { get; set; } = new List<Task> { };
        public List<Note> Notes { get; set; } = new List<Note>();
        public List<Label> Labels { get; set; } = new List<Label>();
    }
}
