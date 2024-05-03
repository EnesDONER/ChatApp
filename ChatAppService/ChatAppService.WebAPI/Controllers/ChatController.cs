using ChatAppService.WebAPI.Context;
using ChatAppService.WebAPI.Dtos;
using ChatAppService.WebAPI.Hubs;
using ChatAppService.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;

namespace ChatAppService.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public sealed class ChatsController(ApplicationDbContext context,IHubContext<ChatHub> hubContext) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            List<User> users = await context.Users.OrderBy(p=> p.Name).ToListAsync();
            return Ok(users);
        }
        [HttpGet]
        public async Task<IActionResult> GetGroup(CancellationToken cancellationToken)
        {
            List<Group> groups = await context.Groups.ToListAsync(cancellationToken);
            return Ok(groups);
        }
        [HttpGet]
        public async Task<IActionResult> GetGroupChats(Guid userId, Guid groupId, CancellationToken cancellationToken)
        {
            List<GroupChat> chats = await context.GroupChats
                  .Where(
                    p => p.UserId == userId
                    && p.GroupId == groupId
                    || p.GroupId == userId
                    && p.UserId == groupId)
                .OrderBy(p => p.Date)
                .ToListAsync(cancellationToken);
            return Ok(chats);
        }

        [HttpGet]
        public async Task<IActionResult> GetChats(Guid userId, Guid toUserId, CancellationToken cancellationToken)
        {
            List<Chat> chats = await context.Chats
                .Where(
                    p=> p.UserId == userId 
                    && p.ToUserId == toUserId 
                    || p.ToUserId == userId 
                    && p.UserId ==toUserId )
                .OrderBy(p=>p.Date)
                .ToListAsync(cancellationToken);
            return Ok(chats);
        }

        [HttpPost] 
        public async Task<IActionResult> AddGroup(GroupDto group,CancellationToken cancellationToken)
        {
            List<User> users = new();
            foreach (var userId in group.UserId)
            {
                User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId,cancellationToken);
                if (user != null)
                {
                    users.Add(user);
                }
            }
           
            Group addedGroup = new Group()
            {
                Name = group.GroupName,
                Users = users
            };
            await context.Groups.AddAsync(addedGroup,cancellationToken);
            await context.SaveChangesAsync(cancellationToken);



            return Ok(addedGroup);
        }


        [HttpPost]
        public async Task<IActionResult> SendMessageToGroup(SendMessageToGroupDto sendMessageDto,CancellationToken cancellationToken)
        {
            GroupChat chat = new()
            {
                UserId = sendMessageDto.UserId,
                GroupId = sendMessageDto.GroupId,
                Message = sendMessageDto.Message,
                Date = DateTime.UtcNow
            };
            await context.AddAsync(chat,cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            Group group = await context.Groups.FirstAsync(u => u.Id == chat.GroupId);
        

            List<User> onlineUsers = group.Users.Where(u=>u.Status == Status.Online).ToList();

            foreach (var onlineUser in onlineUsers)
            {
                string connectionId = ChatHub.Users.First(p => p.Value == onlineUser.Id).Key;
                await hubContext.Clients.Client(connectionId).SendAsync("GroupMessages", chat);
            }

            return Ok(chat);
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto sendMessageDto, CancellationToken cancellationToken)
        {
            Chat chat = new()
            {
                UserId = sendMessageDto.UserId,
                ToUserId = sendMessageDto.ToUserId,
                Message = sendMessageDto.Message,
                Date = DateTime.UtcNow
            };
            await context.AddAsync(chat, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            Status userStatus = context.Users.First(p => p.Id == chat.ToUserId).Status;
            if (userStatus == Status.Online)
            {
                string connectionId = ChatHub.Users.First(p => p.Value == chat.ToUserId).Key;

                await hubContext.Clients.Client(connectionId).SendAsync("Messages", chat);
            }


            return Ok(chat);
        }
    }
}
