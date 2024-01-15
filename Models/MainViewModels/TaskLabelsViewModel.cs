namespace ToDoAndNotes3.Models.MainViewModels
{
    public class TaskLabelsViewModel
    {
        public Task Task { get; set; } = new Task();
        public List<Label> Labels { get; set; } = new List<Label>();
        public string? SelectedLabelsId { get; set; }
    }
}
