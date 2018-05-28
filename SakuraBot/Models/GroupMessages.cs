using System.ComponentModel.DataAnnotations;

namespace Sakura.Uwu.Models
{
    public class GroupMessages
    {
        [Key]
        public long ChatId { get; set; }
        public int? WelcomeMessage { get; set; }
        public int? RulesMessage { get; set; }
        public int? WelcomeMedia { get; set; }
        public GroupMessages() {}
    }
}