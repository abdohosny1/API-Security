using Microsoft.AspNetCore.Identity;

namespace API_Security.Models
{
    public class User :IdentityUser
    {
        public int Age { get; set; }
    }
}
