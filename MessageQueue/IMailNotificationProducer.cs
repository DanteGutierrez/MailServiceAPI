namespace MailServiceAPI.MessageQueue
{
    public interface IMailNotificationProducer
    {
        void SendMessage<T>(T message);
    }
}
