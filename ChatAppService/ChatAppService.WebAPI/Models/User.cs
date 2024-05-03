﻿namespace ChatAppService.WebAPI.Models
{
    public sealed class User
    {
        public User()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Offline;

        public List<Group>? Grups { get; set; }
    }

    public enum Status
    {
        Offline,
        Online
    }
}
