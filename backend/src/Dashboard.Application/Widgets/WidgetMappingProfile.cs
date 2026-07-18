using System.Text.Json;
using AutoMapper;
using Dashboard.Domain.Entities;

namespace Dashboard.Application.Widgets;

public sealed class WidgetMappingProfile : Profile
{
    public WidgetMappingProfile()
    {
        CreateMap<Widget, WidgetDto>()
            .ForCtorParam(nameof(WidgetDto.Row), opt => opt.MapFrom(src => src.Order / Widget.ColumnsPerRow))
            .ForCtorParam(nameof(WidgetDto.Column), opt => opt.MapFrom(src => src.Order % Widget.ColumnsPerRow))
            .ForCtorParam(nameof(WidgetDto.Data), opt => opt.MapFrom(src => JsonSerializer.Deserialize<JsonElement>(src.DataJson, (JsonSerializerOptions?)null)));
    }
}
