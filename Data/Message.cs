using System.Text.Json.Serialization;

namespace MailServiceAPI.Data
{
    public class Message
    {
        public long Id { get; set; }
        public long SenderId { get; set; }
        public long RecieverId { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        [JsonIgnore]
        public User Sender { get; set; }
        [JsonIgnore]
        public User Reciever { get; set; }
    }
    public class MessageDTO
    {
        public long Id { get; set; }
        public long SenderId { get; set; }
        public long RecieverId { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public User Sender { get; set; }
        public User Reciever { get; set; }
    }
    public class MessageCreateUpdate
    {
        public long SenderId { get; set; }
        public long RecieverId { get; set; }
        public DateTime? Date { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

    }
}
