using System.ComponentModel.DataAnnotations;

namespace Sakura.Uwu.Models
{
    // Configurations Settings Data Model
    public class UserLookup
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public UserLookup() {}
        public UserLookup(int uid, string uname)
        {
            this.UserId = uid;
            this.UserName = uname;
        }
    }
}