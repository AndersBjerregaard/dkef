using AutoMapper;
using Dkef.Data;
using Dkef.Domain;
using Dkef.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => {
        options
            .WithTitle("dkef-api")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
