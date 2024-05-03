namespace ChatAppService.WebAPI.Dtos
{
    public sealed record GroupDto(
        List<Guid> UserId,
        string GroupName
        );
}
