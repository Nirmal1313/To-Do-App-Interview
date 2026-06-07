using ToDoApp.Domain.Common.Enums;
using ToDoApp.Domain.Entities.ToDoTask.TaskDetails;
using DomainTaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.Tests.TestHelpers
{
    public class TaskBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _title = "Default Task";
        private string _description = "Test";
        private DomainTaskStatus _status = DomainTaskStatus.Created;
        private Guid _ownerId = Guid.NewGuid();

        public static TaskBuilder Create() => new();

        public TaskBuilder WithId(Guid id) { _id = id; return this; }
        public TaskBuilder WithTitle(string title) { _title = title; return this; }
        public TaskBuilder WithDescription(string description) { _description = description; return this; }
        public TaskBuilder WithStatus(DomainTaskStatus status) { _status = status; return this; }
        public TaskBuilder CreatedBy(Guid userId) { _ownerId = userId; return this; }

        public ToDoTasks Build() => new()
        {
            Id = _id,
            Title = _title,
            Description = _description,
            Status = _status,
            CreatedDate = DateTime.UtcNow,
            UserId = _ownerId,
        };
    }
}
