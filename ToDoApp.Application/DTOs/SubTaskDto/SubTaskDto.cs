using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Application.DTOs.SubTaskDto
{
    public class SubTaskDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }

        public Guid ParentTaskId { get; set; }
    }
}
