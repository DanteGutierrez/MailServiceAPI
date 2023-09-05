using MailServiceAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using FluentEmail.Core;
using MailServiceAPI.MessageQueue;

namespace MailServiceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : Controller
    {
        private readonly ILogger<MessageController> logger;
        private MailDBContext db;
        private IMailNotificationProducer producer;
        private IFluentEmail email;
        public MessageController(ILogger<MessageController> logger, MailDBContext db, IMailNotificationProducer producer, IFluentEmail email)
        {
            this.logger = logger;
            this.db = db;
            this.producer = producer;
            this.email = email;
        }

        [AllowAnonymous]
        [HttpGet("")]
        public string Index()
        {
            return "Message Endpoint Online";
        }

        [Authorize]
        [HttpGet("messages")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesAsync()
        {
            var result = db.Messages.Include(x => x.Sender).Include(x => x.Reciever).Select(x => new MessageDTO()
            {
                Id = x.Id,
                RecieverId = x.RecieverId,
                SenderId = x.SenderId,
                Reciever = x.Reciever,
                Sender = x.Sender,
                Date = x.Date,
                Subject = x.Subject,
                Content = x.Content
            });

            if (result is null) return BadRequest();

            return Ok(result);
        }

        [Authorize]
        [HttpGet("messages/{id}")]
        public async Task<ActionResult<MessageDTO>> GetMessageByIdAsync([FromRoute] long id)
        {
            var result = await db.Messages.Include(x => x.Sender).Include(x => x.Reciever).FirstOrDefaultAsync(x => x.Id == id);

            if (result is null) return NotFound();

            MessageDTO dto = new()
            {
                Id = result.Id,
                RecieverId = result.RecieverId,
                SenderId = result.SenderId,
                Reciever = result.Reciever,
                Sender = result.Sender,
                Date = result.Date,
                Subject = result.Subject,
                Content = result.Content
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpPost("messages")]
        public async Task<ActionResult<MessageDTO>> PostMessageAsync([FromBody] MessageCreateUpdate message)
        {
            Message createdMessage = new()
            {
                RecieverId = message.RecieverId,
                SenderId = message.SenderId,
                Date = message.Date ?? DateTime.Now,
                Subject = message.Subject,
                Content = message.Content
            };

            var result = await db.Messages.AddAsync(createdMessage);

            if (result is null) return BadRequest();

            await db.SaveChangesAsync();

            var sender = await db.Users.FirstOrDefaultAsync(x => x.Id == result.Entity.SenderId);
            var reciever = await db.Users.FirstOrDefaultAsync(x => x.Id == result.Entity.RecieverId);

            if (sender is not null && reciever is not null)
            {
                MailNotification notification = new()
                {
                    Id = result.Entity.Id,
                    Content = result.Entity.Content,
                    Date = result.Entity.Date,
                    RecieverEmail = reciever.Email,
                    RecieverId = reciever.Id,
                    RecieverName = reciever.Name,
                    SenderEmail = sender.Email,
                    SenderId = sender.Id,
                    SenderName = sender.Name,
                    Subject = result.Entity.Subject
                };
                producer.SendMessage(notification);
            }

            return await GetMessageByIdAsync(result.Entity.Id);
        }

        [Authorize]
        [HttpDelete("messages/{id}")]
        public async Task<ActionResult> DeleteMessageByIdAsync([FromRoute] long id)
        {
            var result = await db.Messages.FirstOrDefaultAsync(x => x.Id == id);

            if (result is null) return NotFound();

            db.Messages.Remove(result);

            await db.SaveChangesAsync();

            return Ok();
        }
    }
}
