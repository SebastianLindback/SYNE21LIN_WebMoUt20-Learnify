using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Entity
{
    public class User : IdentityUser
    {
        public ICollection<UserCourse>? UserCourses { get; set; }
    }
}