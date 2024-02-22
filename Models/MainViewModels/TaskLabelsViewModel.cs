namespace ToDoAndNotes3.Models.MainViewModels
{
    public class TaskLabelsViewModel
    {
        public Task Task { get; set; } = new Task();
        public List<Label>? Labels { get; set; } = new List<Label>();
        public List<Project>? Projects { get; set; } = new List<Project>();
        public string? SelectedLabelsId { get; set; }
    }
}
