namespace ChatAppService.WebAPI.Models
{
    public sealed class GroupChat
    {
        public GroupChat()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
