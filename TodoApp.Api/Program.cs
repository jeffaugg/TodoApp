using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TodoApp.Api.Extensions;
using TodoApp.Api.Validators;
using TodoApp.Application.Services;
using TodoApp.Application.Services.Interfaces;
using TodoApp.Domain.Repositories.Interfaces;
using TodoApp.Infrastructure.Contexts;
using TodoApp.Infrastructure.Repositories;
using TodoApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
} );

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<ITarefaService, TarefaService>();


builder.Services.AddJwtAuthentication(builder.Configuration);


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