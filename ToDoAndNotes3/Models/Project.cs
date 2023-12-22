namespace ToDoAndNotes3.Models
{
    public class Project
    {
        public int? ProjectId { get; set; }
        public string? UserId { get; set; }
        public string? Title { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public bool? IsDeleted { get; set; }
        public ICollection<Task>? Tasks { get; set; }
        public ICollection<Note>? Notes { get; set; }

    }
}
