using System.ComponentModel.DataAnnotations;

namespace Sakura.Uwu.Models
{
    // Configurations Settings Data Model
    public class UserWarns
    {
        [Key]
        public int UserId { get; set; }
        public long GroupId { get; set; }
        public int WarnCount { get; set; }
        public UserWarns(long gid) 
        {
            this.UserId = 0;
            this.WarnCount = 1;
        }
        public UserWarns(long gid, int uid)
        {
            this.UserId = uid;
            this.GroupId = gid;
            this.WarnCount = 1;
        }
    }
}