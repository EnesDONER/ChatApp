namespace ChatAppService.WebAPI.Dtos
{
    public class GroupChatDto {

        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string UserName { get; set; } = string.Empty;

    }
}

