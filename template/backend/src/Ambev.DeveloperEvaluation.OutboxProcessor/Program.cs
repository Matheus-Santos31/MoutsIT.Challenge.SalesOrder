using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.MessageBus;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.OutboxProcessor;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<DefaultContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
    )
);

builder.Services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();
builder.Services.AddMessageBus(builder.Configuration);
builder.Services.Configure<OutboxProcessorOptions>(builder.Configuration.GetSection(OutboxProcessorOptions.SectionName));

builder.Services.AddHostedService<OutboxDispatcherService>();

var host = builder.Build();
host.Run();
