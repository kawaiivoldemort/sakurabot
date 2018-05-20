using System.ComponentModel.DataAnnotations;

namespace Sakura.Uwu.Models
{
    public class GroupWelcomeMessages
    {
        [Key]
        public long ChatId { get; set; }
        public string Text { get; set; }
        public GroupWelcomeMessages() {}
        public GroupWelcomeMessages(long chatId, string text) 
        {
            this.ChatId = chatId;
            this.Text = text;
        }
    }
}