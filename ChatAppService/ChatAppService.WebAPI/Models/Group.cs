namespace ChatAppService.WebAPI.Models
{
    public sealed class Group
    {
        public Group()
        {
            Id = Guid.NewGuid();
            Users = new HashSet<User>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }= string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public ICollection<User> Users { get; set; }

    }
}
