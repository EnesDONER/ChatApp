using ChatAppService.WebAPI.Context;
using ChatAppService.WebAPI.Dtos;
using ChatAppService.WebAPI.Hubs;
using ChatAppService.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading;
using GenericFileService.Files;

namespace ChatAppService.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public sealed class ChatsController(ApplicationDbContext context, IHubContext<ChatHub> hubContext) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            List<User> users = await context.Users.Include(u=>u.Groups).OrderBy(p => p.Name).ToListAsync();
            return Ok(users);
        }
        [HttpGet]
        public async Task<IActionResult> GetGroup(CancellationToken cancellationToken)
        {
            List<Group> groups = await context.Groups.ToListAsync(cancellationToken);
            return Ok(groups);
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupById(Guid userId, CancellationToken cancellationToken)
        {


            List<Group> groups = await context.Groups
                .Include(g => g.Users)
                .Where(g => g.Users.Any(u => u.Id == userId))
                .ToListAsync(cancellationToken);



            return Ok(groups);
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupChats(Guid groupId, CancellationToken cancellationToken)
        {
            List<GroupChatDto> chatDtos = await context.GroupChats
                 .Where(p => p.GroupId == groupId)
                 .OrderBy(p => p.Date)
                 .Join(context.Users,
                       chat => chat.UserId,
                       user => user.Id,
                       (chat, user) => new GroupChatDto
                       {
                           UserName = user.Name,
                           UserId = user.Id,
                           Date = chat.Date,
                           GroupId = groupId,
                           Message = chat.Message
                       })
                 .ToListAsync(cancellationToken);

            return Ok(chatDtos);

        }


        [HttpGet]
        public async Task<IActionResult> GetChats(Guid userId, Guid toUserId, CancellationToken cancellationToken)
        {
            List<Chat> chats = await context.Chats
                .Where(
                    p => p.UserId == userId
                    && p.ToUserId == toUserId
                    || p.ToUserId == userId
                    && p.UserId == toUserId)
                .OrderBy(p => p.Date)
                .ToListAsync(cancellationToken);
            return Ok(chats);
        }

        [HttpPost]
        public async Task<IActionResult> AddGroup([FromForm] GroupDto group, CancellationToken cancellationToken)
        {
            
            var users = await context.Users.Where(u => group.UserId.Contains(u.Id)).ToListAsync(cancellationToken);

            if (users.Count != group.UserId.Count)
            {
                var missingUserIds = group.UserId.Except(users.Select(u => u.Id));
                return BadRequest($"Kullanıcılar bulunamadı: {string.Join(",", missingUserIds)}");
            }

            string avatar = FileService.FileSaveToServer(group.File, "wwwroot/avatar/");
            Group addedGroup = new Group
            {
                Name = group.GroupName,
                Avatar = avatar
            };

            foreach (var user in users)
            {
                addedGroup.Users.Add(user);

            }

            await context.Groups.AddAsync(addedGroup, cancellationToken);
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

            string userName = (await context.Users.FirstOrDefaultAsync(u => u.Id == chat.UserId)).Name;
            GroupChatDto chatDto = new()
            {
                UserId = chat.UserId,
                GroupId = chat.GroupId,
                Message = chat.Message,
                Date = DateTime.UtcNow,
                UserName = userName
            };

            Group group = await context.Groups.Include(g=>g.Users).FirstAsync(u => u.Id == chat.GroupId);


            List<User> onlineUsers = group.Users.Where(u => u.Status == Status.Online && u.Id != sendMessageDto.UserId ).ToList();

            foreach (var onlineUser in onlineUsers)
            {
                string connectionId = ChatHub.Users.First(p => p.Value == onlineUser.Id).Key;
                await hubContext.Clients.Client(connectionId).SendAsync("GroupMessages", chatDto);
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
