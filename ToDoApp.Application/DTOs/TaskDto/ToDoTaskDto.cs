using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;
using ToDoApp.Domain.Common.Enums;
using TagDtoClass = ToDoApp.Application.DTOs.TagDto.TagDto;
using SubTaskDtoClass = ToDoApp.Application.DTOs.SubTaskDto.SubTaskDto;

namespace ToDoApp.Application.DTOs.TaskDto
{
    public class ToDoTaskDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        [EnumDataType(typeof(TaskStatus), ErrorMessage = "Invalid status value.")]
        public TaskStatus Status { get; set; } = TaskStatus.Created;

        [EnumDataType(typeof(TaskPriority), ErrorMessage = "Invalid priority value.")]
        public TaskPriority Priority { get; set; } = TaskPriority.Low;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public bool IsCompleted { get; set; }
        public string? AssignedTo { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "A list must be selected for the task.")]
        public Guid? ListId { get; set; }

        public List<TagDtoClass> Tags { get; set; } = new();
        public List<SubTaskDtoClass> SubTasks { get; set; } = new();
    }
}
