namespace ChatAppService.WebAPI.Models
{
    public sealed class Group
    {
        public Group()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }


    }
}
