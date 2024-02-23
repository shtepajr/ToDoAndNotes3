namespace ToDoAndNotes3.Models.MainViewModels
{
    public class GeneralViewModel
    {
        public List<Project> Projects { get; set; } = new List<Project>();
        public List<TdnElement> TdnElements { get; set; } = new List<TdnElement> { };
        public List<Label> Labels { get; set; } = new List<Label>();
    }
}
