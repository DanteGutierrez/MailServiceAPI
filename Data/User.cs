using System.Text.Json.Serialization;

namespace MailServiceAPI.Data
{
    [Flags]
    public enum Roles
    {
        None = 0,
        User = 1,
        Admin = 2,
    }

    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Roles Role { get; set; }
        [JsonIgnore]
        public List<Message> SentMessages { get; set; } = Enumerable.Empty<Message>().ToList();
        [JsonIgnore]
        public List<Message> RecievedMessages { get; set; } = Enumerable.Empty<Message>().ToList();
    }
    public class UserAuth
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Auth
    {
        public string Token { get; set; } = string.Empty;
    }

    public class UserDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Roles Role { get; set; }
        public string RoleString { get { return Role.ToString(); } }
        public List<Message> SentMessages { get; set; } = Enumerable.Empty<Message>().ToList();
        public List<Message> RecievedMessages { get; set; } = Enumerable.Empty<Message>().ToList();
    }

    public class UserCreateUpdate
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Roles Role { get; set; }
    }
}
