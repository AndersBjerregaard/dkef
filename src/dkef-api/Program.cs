using System.Globalization;
using System.Reflection;
using System.Text;
using AutoMapper;
using Dkef.Configuration;
using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;
using Dkef.Services.Interfaces;
using Ganss.Xss;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        .MinimumLevel.Override("Microsoft.AspNetCore.Cors", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
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
    builder.Services.AddDbContext<ForgotPasswordContext>(options => options.UseNpgsql(dbConString));

    builder.Services.AddTransient<DbSet<Contact>>(x =>
        x.GetRequiredService<ContactContext>()
        .Contacts);
    builder.Services.AddTransient<DbSet<Event>>(x =>
        x.GetRequiredService<EventsContext>()
        .Events);

    // Identity
    builder.Services.AddIdentity<Contact, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true; // Set to true if you want email confirmation
        
        // Configure token provider settings
        options.Tokens.PasswordResetTokenProvider = "DatabaseTokenProvider";
    })
        .AddEntityFrameworkStores<ContactContext>()
        .AddDefaultTokenProviders()
        .AddTokenProvider<DatabaseTokenProvider>("DatabaseTokenProvider");
    
    // Register the DatabaseTokenProvider's dependency
    builder.Services.AddScoped<DatabaseTokenProvider>();

    // Authentication
    JwtConfig jwtConfig = new(
    
        Key: builder.Configuration.GetSection("JwtSettings")["Key"]!,
        Issuer: builder.Configuration.GetSection("JwtSettings")["Issuer"]!,
        Audience: builder.Configuration.GetSection("JwtSettings")["Audience"]!,
        ExpiryMinutes: int.Parse(builder.Configuration.GetSection("JwtSettings")["ExpiryMinutes"] ?? "60")
    );

    builder.Services.AddSingleton(jwtConfig);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
            ClockSkew = TimeSpan.Zero // Optional: reduce the default clock skew of 5 minutes
        };
    });

    builder.Services.AddAuthorization();

    // Minio (S3 compatible storage)
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
    builder.Services.AddScoped<IContactRepository, ContactRepository>();
    builder.Services.AddScoped<IEventsRepository, EventsRepository>();
    builder.Services.AddScoped<ForgotPasswordRepository>();

    // Services
    builder.Services.AddTransient<IBucketService, MinioBucketService>();
    builder.Services.AddScoped<IJwtService, JwtService>();

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
        var contactContext = scope.ServiceProvider.GetService<ContactContext>();
        contactContext!.Database.Migrate();
        var eventsContext = scope.ServiceProvider.GetService<EventsContext>();
        eventsContext!.Database.Migrate();
        var forgotPasswordContext = scope.ServiceProvider.GetService<ForgotPasswordContext>();
        forgotPasswordContext!.Database.Migrate();
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

    app.UseAuthentication();
    app.UseAuthorization();

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