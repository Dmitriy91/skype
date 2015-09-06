using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkPackets.Model
{
    [Serializable]
    public class User : IEquatable<User>
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public byte[] ImageBytes { get; set; }
        public bool IsOnline { get; set; }
        public bool Equals(User other)
        {
            if (other == null)
                return false;

            return other.Id == this.Id;
        }
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
