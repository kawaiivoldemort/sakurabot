using System.ComponentModel.DataAnnotations;

namespace Sakura.Uwu.Models
{
    public class GroupSavedMessages
    {
        [Key]
        public string MessageTag { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        
        public GroupSavedMessages() {}
        public GroupSavedMessages(string tag, long gid, int mid)
        {
            this.MessageTag = tag;
            this.ChatId = gid;
            this.MessageId = mid;
        }
    }
}