using System.Globalization;
using AutoMapper;
using Dkef.Configuration;
using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.AspNetCore;
using Scalar.AspNetCore;

var CorsPolicy = "_dkefOrigins";

var builder = WebApplication.CreateBuilder(args);

var origins = builder.Configuration.GetValue<string>("AllowedHosts");

// Cors policy must be set to specific origins
// in order to allow credentials
builder.Services.AddCors(options => {
    options.AddPolicy(name: CorsPolicy,
        policy => {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins([origins!]);
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
var mapper = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Contact, Contact>();
    cfg.CreateMap<EventDto, Event>()
        .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => $"{minioConString}/events/{src.ThumbnailId}"))
        .ForMember(dest => dest.DateTime, opt => opt.MapFrom(src => DateTime.Parse(src.DateTime, CultureInfo.InvariantCulture, DateTimeStyles.None)));
}).CreateMapper();

builder.Services.AddSingleton<IMapper>(mapper);

var contactMapper = new MapperConfiguration(cfg => cfg.CreateMap<Contact, Contact>())
    .CreateMapper();

// Repositories
builder.Services.AddTransient<IContactRepository, ContactRepository>();
builder.Services.AddTransient<IEventsRepository, EventsRepository>();

// Services
builder.Services.AddTransient<IBucketService, MinioBucketService>();

// Configuration
builder.Services.AddSingleton<DomainUrlConfiguration>(x => new DomainUrlConfiguration(minioConString!));

var app = builder.Build();

// // Database migration
// using (var scope = app.Services.CreateScope()) {
//     var context = scope.ServiceProvider.GetService<ContactContext>();
//     context!.Database.Migrate();
//     if (app.Environment.IsDevelopment()) {
//         await ContactContext.SeedAsync(context);
//     }
// }

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
