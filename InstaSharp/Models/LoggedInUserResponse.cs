using System;

namespace InstaSharp.Models
{
    public class LoggedInUserResponse : EventArgs
    {
        public string Username { get; set; }
        public bool HasAnonymousProfilePicture { get; set; }
        public string ProfilePicUrl { get; set; }
        public string FullName { get; set; }
        public long Pk { get; set; }
        public bool IsPrivate { get; set; }
    }
}