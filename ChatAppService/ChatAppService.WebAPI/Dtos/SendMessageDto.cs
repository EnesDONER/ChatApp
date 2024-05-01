namespace ChatAppService.WebAPI.Dtos
{
    public sealed record SendMessageDto(
        Guid UserId,
        Guid ToUserId,
        string Message
        );
}
