using System.ComponentModel.DataAnnotations;

namespace Sakura.Uwu.Models
{
    // Configurations Settings Data Model
    public class UserWarns
    {
        [Key]
        public int UserId { get; set; }
        public int WarnCount { get; set; }
        public UserWarns() 
        {
            this.UserId = 0;
            this.WarnCount = 1;
        }
        public UserWarns(int uid)
        {
            this.UserId = uid;
            this.WarnCount = 1;
        }
    }
}