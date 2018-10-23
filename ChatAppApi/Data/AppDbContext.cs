using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAppApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ChatAppApi.Data
{
    public class AppDbContext : IdentityDbContext<UserApp>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Message> Messages { get; set; }

    }
}
