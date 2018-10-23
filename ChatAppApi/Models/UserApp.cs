using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAppApi.Models
{
    public class UserApp : IdentityUser
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }
    }
}
