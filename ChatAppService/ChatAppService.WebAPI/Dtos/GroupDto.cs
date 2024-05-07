namespace ChatAppService.WebAPI.Dtos
{
    public sealed record GroupDto(
        IList<Guid> UserId,
        string GroupName,
        IFormFile File
    );
}
