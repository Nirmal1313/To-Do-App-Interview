using System.ComponentModel.DataAnnotations;
using ToDoApp.Domain.Common;

namespace ToDoApp.Domain.Entities.Lists
{
    public class ToDoList : OwnedEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Color { get; set; } = "#000000";

        public ICollection<ToDoTask.TaskDetails.ToDoTasks> Tasks { get; set; } = new List<ToDoTask.TaskDetails.ToDoTasks>();
    }
}
