using ChatAppService.WebAPI.Context;
using ChatAppService.WebAPI.Dtos;
using ChatAppService.WebAPI.Models;
using GenericFileService.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAppService.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public sealed class AuthController(ApplicationDbContext context) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterDto registerDto, CancellationToken cancellationToken)
        {
            bool isNameExists = await context.Users.AnyAsync(p=> p.Name == registerDto.Name,cancellationToken);
            if (isNameExists)
            {
                return BadRequest(new { Message = "This name already exists" });
            }
            string avatar = FileService.FileSaveToServer(registerDto.File, "wwwroot/avatar/");
            User user = new()
            {
                Name = registerDto.Name,
                Avatar = avatar

            };
            await context.AddAsync(user,cancellationToken);
            await context.SaveChangesAsync();
            return Ok(user);
        }
        [HttpGet]
        public async Task<IActionResult> Login ( string name, CancellationToken cancellationToken)
        {
            User? user = await context.Users.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
            if (user == null)
            {
                return BadRequest(new { Message = "User can not found" });
            }

            user.Status = "online";
            await context.SaveChangesAsync(cancellationToken);
            
            return Ok(user);
        }
    }
}
