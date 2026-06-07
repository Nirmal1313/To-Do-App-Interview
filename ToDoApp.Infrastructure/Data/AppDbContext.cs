using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities.Lists;
using ToDoApp.Domain.Entities.SubTasks;
using ToDoApp.Domain.Entities.Tags;
using ToDoApp.Domain.Entities.ToDoTask.TaskDetails;
using ToDoApp.Domain.Entities.User;

namespace ToDoApp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ToDoTasks> ToDoTasks { get; set; }
        public DbSet<ToDoList> ToDoLists { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TaskTag> TaskTags { get; set; }
        public DbSet<SubTask> SubTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskTag>()
                .HasKey(tt => new { tt.TaskId, tt.TagId });

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TagId);

            modelBuilder.Entity<ToDoTasks>()
                .HasOne(t => t.List)
                .WithMany(l => l.Tasks)
                .HasForeignKey(t => t.ListId)
                .IsRequired(false);

            modelBuilder.Entity<SubTask>()
                .HasOne(s => s.ParentTask)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(s => s.ParentTaskId);
        }
    }
}
