using System.ComponentModel.DataAnnotations;
using ToDoApp.Domain.Common;

namespace ToDoApp.Domain.Entities.SubTasks
{
    public class SubTask : AuditableEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }

        public Guid ParentTaskId { get; set; }

        public ToDoTask.TaskDetails.ToDoTasks ParentTask { get; set; } = null!;
    }
}
