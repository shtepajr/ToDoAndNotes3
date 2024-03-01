using System.ComponentModel.DataAnnotations;

namespace ToDoAndNotes3.Models
{
    public abstract class TdnSortElement
    {
        [DataType(DataType.Date)]
        virtual public DateOnly? DueDate { get; set; }
        [DataType(DataType.Time)]
        virtual public TimeOnly? DueTime { get; set; }
        virtual public bool IsCompleted { get; set; }
    }
}
