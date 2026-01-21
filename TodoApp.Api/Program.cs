using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TodoApp.Api.DTOs;        // Certifique-se que seus DTOs estão acessíveis
using TodoApp.Api.Validators;  // Certifique-se que seus Validators estão acessíveis
using TodoApp.Api.Controllers; // Para garantir acesso aos controllers
using TodoApp.Application.Services;
using TodoApp.Application.Services.Interfaces;
using TodoApp.Domain.Repositories.Interfaces;
using TodoApp.Infrastructure.Contexts;
using TodoApp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
} );

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("docs", (options) =>
    {
        options.WithTitle("TodoApp API");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();