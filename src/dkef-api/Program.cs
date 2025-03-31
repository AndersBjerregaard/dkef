using AutoMapper;
using Dkef.Data;
using Dkef.Domain;
using Dkef.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var CorsPolicy = "_dkefOrigins";

var builder = WebApplication.CreateBuilder(args);

// Cors policy must be set to specific origins
// in order to allow credentials
builder.Services.AddCors(options => {
    options.AddPolicy(name: CorsPolicy,
        policy => {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
    });
});

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var dbConString = builder.Configuration.GetConnectionString("PostgresDb");

builder.Services.AddDbContext<ContactContext>(options => options.UseNpgsql(dbConString));

var contactMapper = new MapperConfiguration(cfg => cfg.CreateMap<Contact, Contact>())
    .CreateMapper();

builder.Services.AddTransient<IContactRepository>(x => {
    var context = x.GetService<ContactContext>();
    return new ContactRepository(context!, contactMapper);
});

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
    app.MapScalarApiReference(options => {
        options
            .WithTitle("dkef-api")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

// app.UseAuthorization();

app.MapControllers();

app.Run();
