using MongoDB.Bson;

namespace Sakura.Uwu.Models {
    // Configurations Settings Data Model
    public class UserWarns {
        public ObjectId _id { get; set; }
        public int UserId { get; set; }
        public int WarnCount { get; set; }
        public UserWarns(int uid) {
            this._id = new ObjectId();
            this.UserId = uid;
            this.WarnCount = 1;
        }
    }
}