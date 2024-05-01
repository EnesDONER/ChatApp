namespace ChatAppService.WebAPI.Dtos
{
    public sealed record RegisterDto(
        string Name, IFormFile File);
}
