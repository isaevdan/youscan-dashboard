using AutoMapper;
using Dashboard.Application.Widgets;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Application.Tests;

internal static class TestMapper
{
    public static IMapper Instance { get; } = BuildMapper();

    private static IMapper BuildMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<WidgetMappingProfile>());
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }
}
