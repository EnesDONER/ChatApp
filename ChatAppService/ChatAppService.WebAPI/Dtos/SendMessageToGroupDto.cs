namespace ChatAppService.WebAPI.Dtos
{
      public sealed record SendMessageToGroupDto(
        Guid UserId,
        Guid GroupId,
        string Message
        );
}
