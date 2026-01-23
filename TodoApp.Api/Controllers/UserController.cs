using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.DTOs;
using TodoApp.Application.DTOs;
using TodoApp.Application.Services.Interfaces;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public UserController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        try 
        {
            var createDto = new CreateUserDto(dto.Name, dto.Email, dto.Password);

            var userId = await _userService.AddAsync(createDto);

            var response = new UserResponseDto(userId, dto.Name, dto.Email);
            return CreatedAtAction(nameof(Register), new { id = userId }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponseDto(ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto dto)
    {
        try
        {
            var user = await _userService.LoginAsync(dto.Email, dto.Password);

            var token =  _tokenService.GenerateToken(user);

            var response = new LoginResponseDto(
                "Login realizado com sucesso!",
                token,
                user.Id,
                user.Name
            );
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new ErrorResponseDto(ex.Message));
        }
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMyProfile()
    {
        var nome = User.Identity?.Name;

        return Ok(new { message = $"Olá, {nome}! Você acessou uma rota protegida." });
    }
}