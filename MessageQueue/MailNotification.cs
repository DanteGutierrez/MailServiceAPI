namespace MailServiceAPI.MessageQueue
{
    public class MailNotification
    {
        public long Id { get; set; }
        public long SenderId { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public long RecieverId { get; set; }
        public string RecieverEmail { get; set; } = string.Empty;
        public string RecieverName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
