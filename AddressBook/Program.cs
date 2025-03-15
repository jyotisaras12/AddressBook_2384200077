using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Context;
using ModelLayer.Validator;
using FluentValidation;
using BusinessLayer.AutoMapper;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddDbContext<AddressBookContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

// Register AutoMapper

var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<Mapping>(); 
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// Register FluentValidation

builder.Services.AddValidatorsFromAssemblyContaining<AddressBookValidator>();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
