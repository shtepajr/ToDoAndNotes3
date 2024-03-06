namespace ToDoAndNotes3.Models.MainViewModels
{
    public class BinViewModel
    {
        public List<Project> ActiveProjects { get; set; } = new List<Project>();
        public List<Project> DeletedProjects { get; set; } = new List<Project>();
        public List<TdnSortElement> TdnElements { get; set; } = new List<TdnSortElement> { };
        public List<Label> Labels { get; set; } = new List<Label>();
        public ManageViewModels.IndexViewModel Manage { get; set; }
    }
}
