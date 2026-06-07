using System.ComponentModel.DataAnnotations;
using ToDoApp.Domain.Common;
using ToDoApp.Domain.Common.Enums;
using ToDoApp.Domain.Entities.SubTasks;
using ToDoApp.Domain.Entities.Tags;
using TaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.Domain.Entities.ToDoTask.TaskDetails
{
    public class ToDoTasks : OwnedEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public TaskStatus Status { get; set; } = TaskStatus.Created;
        public TaskPriority Priority { get; set; } = TaskPriority.Low;
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public string? AssignedTo { get; set; }

        public Guid? ListId { get; set; }
        public Lists.ToDoList? List { get; set; }

        public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}
