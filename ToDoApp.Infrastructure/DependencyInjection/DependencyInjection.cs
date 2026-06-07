using Microsoft.Extensions.DependencyInjection;
using ToDoApp.Application.Auth.Interface;
using ToDoApp.Application.Auth.Service;
using ToDoApp.Domain.Interfaces.Lists;
using ToDoApp.Domain.Interfaces.SubTasks;
using ToDoApp.Domain.Interfaces.Tags;
using ToDoApp.Domain.Interfaces.TaskManagment;
using ToDoApp.Domain.Interfaces.UserManagment;
using ToDoApp.Infrastructure.Repositories.Lists;
using ToDoApp.Infrastructure.Repositories.SubTasks;
using ToDoApp.Infrastructure.Repositories.Tags;
using ToDoApp.Infrastructure.Repositories.TaskManagement;
using ToDoApp.Infrastructure.Repositories.UserManagement;

namespace ToDoApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITaskManagmentRepository, TaskManagementRepository>();
            services.AddScoped<IToDoListRepository, ToDoListRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ISubTaskRepository, SubTaskRepository>();

            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<ITokenStore, InMemoryTokenStore>();

            return services;
        }
    }
}
