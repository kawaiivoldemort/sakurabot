using System.ComponentModel.DataAnnotations;

namespace Sakura.Uwu.Models
{
    public class UserSavedMessages
    {
        [Key]
        public string MessageTag { get; set; }
        public int UserId { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        
        public UserSavedMessages() {}
        public UserSavedMessages(string tag, int uid, long gid, int mid)
        {
            this.MessageTag = tag;
            this.UserId = uid;
            this.ChatId = gid;
            this.MessageId = mid;
        }
    }
}