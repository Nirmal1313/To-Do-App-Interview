using AutoMapper;
using ToDoApp.Application.DTOs.ListDto;
using ToDoApp.Application.DTOs.LoginDto;
using ToDoApp.Application.DTOs.RegistrationDto;
using ToDoApp.Application.DTOs.SubTaskDto;
using ToDoApp.Application.DTOs.TagDto;
using ToDoApp.Application.DTOs.TaskDto;
using ToDoApp.Domain.Entities.Lists;
using ToDoApp.Domain.Entities.SubTasks;
using ToDoApp.Domain.Entities.Tags;
using ToDoApp.Domain.Entities.ToDoTask.TaskDetails;
using ToDoApp.Domain.Entities.User;

namespace ToDoApp.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, RegisterDto>().ReverseMap();
            CreateMap<User, LoginDto>().ReverseMap();

            CreateMap<ToDoTasks, ToDoTaskDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId.ToString()))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy.HasValue ? src.UpdatedBy.Value.ToString() : string.Empty))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                    src.TaskTags.Select(tt => new TagDto
                    {
                        Id = tt.Tag.Id,
                        Name = tt.Tag.Name,
                        Color = tt.Tag.Color
                    }).ToList()))
                .ForMember(dest => dest.SubTasks, opt => opt.MapFrom(src => src.SubTasks))
                .ReverseMap()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.TaskTags, opt => opt.Ignore())
                .ForMember(dest => dest.SubTasks, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<ToDoList, ToDoListDto>()
                .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count(t => !t.IsDeleted)))
                .ReverseMap()
                .ForMember(dest => dest.Tasks, opt => opt.Ignore());

            CreateMap<Tag, TagDto>().ReverseMap();

            CreateMap<SubTask, SubTaskDto>().ReverseMap();
        }
    }
}
