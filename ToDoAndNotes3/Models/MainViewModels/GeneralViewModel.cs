namespace ToDoAndNotes3.Models.MainViewModels
{
    public class GeneralViewModel
    {
        public List<Project> Projects { get; set; }
        public List<Task> Tasks { get; set; }
        public List<Note> Notes { get; set; }
        public List<Label> Labels { get; set; }
    }
}
