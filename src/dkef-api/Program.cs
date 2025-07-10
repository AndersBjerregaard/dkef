using System.Globalization;
using System.Reflection;
using AutoMapper;
using Dkef.Configuration;
using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.AspNetCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

var CorsPolicy = "_dkefOrigins";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // builder.Configuration.AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true);

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new ExpressionTemplate(
            // Include trace and span ids when present.
            "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
            theme: TemplateTheme.Code
        ))
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", Serilog.Events.LogEventLevel.Warning)
        .Enrich.WithClientIp()
        .Enrich.WithCorrelationId()
    // Enrich.WithRequestHeader("Header-Name1")
    );

    var origins = builder.Configuration.GetValue<string>("AllowedHosts");

    // Cors policy must be set to specific origins
    // in order to allow credentials
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: CorsPolicy,
            policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(origins!.Split(',', StringSplitOptions.RemoveEmptyEntries));
            });
    });

    builder.Services.AddControllers();

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var dbConString = builder.Configuration.GetConnectionString("PostgresDb");

    // Contexts
    builder.Services.AddDbContext<ContactContext>(options => options.UseNpgsql(dbConString));
    builder.Services.AddDbContext<EventsContext>(options => options.UseNpgsql(dbConString));

    builder.Services.AddTransient<DbSet<Contact>>(x =>
        x.GetRequiredService<ContactContext>()
        .Contacts);
    builder.Services.AddTransient<DbSet<Event>>(x =>
        x.GetRequiredService<EventsContext>()
        .Events);

    // Minio
    var minioConString = builder.Configuration.GetConnectionString("Minio");
    var minioAccessKey = builder.Configuration.GetSection("Minio")["AccessKey"];
    var minioSecretKey = builder.Configuration.GetSection("Minio")["SecretKey"];
    var minioSecure = bool.Parse(builder.Configuration.GetSection("Minio")["Secure"]!);

    builder.Services.AddMinio(configureClient => configureClient
        .WithEndpoint(minioConString)
        .WithCredentials(minioAccessKey, minioSecretKey)
        .WithSSL(minioSecure)
        .Build());

    // AutoMapper
    var thumbnailHttpProtocol = minioSecure ? "https" : "http";
    var thumbnailPrefix = $"{thumbnailHttpProtocol}://{minioConString}";

    var mapper = new MapperConfiguration(cfg =>
    {
        cfg.CreateMap<Contact, Contact>();
        cfg.CreateMap<ContactDto, Contact>();
        cfg.CreateMap<Event, Event>();
        cfg.CreateMap<EventDto, Event>()
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => $"{thumbnailPrefix}/events/{src.ThumbnailId}"))
            .ForMember(dest => dest.DateTime, opt => opt.MapFrom(src => DateTime.Parse(src.DateTime, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal).ToUniversalTime()));
    }).CreateMapper();

    builder.Services.AddSingleton<IMapper>(mapper);

    // Repositories
    builder.Services.AddTransient<IContactRepository, ContactRepository>();
    builder.Services.AddTransient<IEventsRepository, EventsRepository>();

    // Services
    builder.Services.AddTransient<IBucketService, MinioBucketService>();

    // Security
    builder.Services.AddSingleton<HtmlSanitizer>(x => new());
    builder.Services.AddTransient<QueryableService<Event>>();
    builder.Services.AddTransient<QueryableService<Contact>>();

    // Configuration
    builder.Services.AddSingleton<SortablePropertyConfig>(x => new(Assembly.GetExecutingAssembly()));

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    // Database migration
    using (var scope = app.Services.CreateScope())
    {
        Log.Information("Migrating database...");
        var context = scope.ServiceProvider.GetService<ContactContext>();
        context!.Database.Migrate();
        // if (app.Environment.IsDevelopment()) {
        //     await ContactContext.SeedAsync(context);
        // }
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("dkef-api")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }
    else
    {
        app.UseHttpsRedirection();
    }


    app.UseCors(CorsPolicy);

    // app.UseAuthorization();

    app.MapControllers();

    app.Run();

    app.Logger.LogInformation("Web application ready");
}
catch (System.Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}