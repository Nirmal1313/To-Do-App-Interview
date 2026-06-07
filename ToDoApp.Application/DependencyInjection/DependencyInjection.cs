using Microsoft.Extensions.DependencyInjection;
using ToDoApp.Application.ListManagement.Interface;
using ToDoApp.Application.ListManagement.Service;
using ToDoApp.Application.Mappings;
using ToDoApp.Application.SubTaskManagement.Interface;
using ToDoApp.Application.SubTaskManagement.Service;
using ToDoApp.Application.TagManagement.Interface;
using ToDoApp.Application.TagManagement.Service;
using ToDoApp.Application.TaskManagement.Interface;
using ToDoApp.Application.TaskManagement.Service;
using ToDoApp.Application.UserManagement.Interfaces;
using ToDoApp.Application.UserManagement.Services;

namespace ToDoApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITaskManagementService, TaskManagementService>();
            services.AddScoped<IToDoListService, ToDoListService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ISubTaskService, SubTaskService>();

            return services;
        }
    }
}
