using System.ComponentModel.DataAnnotations;

namespace Sakura.Uwu.Models
{
    public class AdminSavedMessages
    {
        [Key]
        public string MessageTag { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        
        public AdminSavedMessages() {}
        public AdminSavedMessages(string tag, long gid, int mid)
        {
            this.MessageTag = tag;
            this.ChatId = gid;
            this.MessageId = mid;
        }
    }
}