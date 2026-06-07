namespace ToDoApp.Domain.Entities.Tags
{
    public class TaskTag
    {
        public Guid TaskId { get; set; }

        public Guid TagId { get; set; }

        public bool IsDeleted { get; set; }

        public Guid? UpdatedBy { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public Tag Tag { get; set; } = null!;
    }
}
