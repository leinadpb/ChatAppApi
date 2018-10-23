using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ChatAppApi.Models;

namespace ChatAppApi.Data
{
    public class Seeder
    {
        private readonly UserManager<UserApp> _userManager;
        private readonly AppDbContext _context;

        public Seeder(UserManager<UserApp> userManager, AppDbContext ctx)
        {
            _userManager = userManager;
            _context = ctx;
        }

        public async Task Seed()
        {
            _context.Database.EnsureCreated();

            UserApp user = await _userManager.FindByEmailAsync("admin@admin.com");
            if (user == null)
            {
                user = new UserApp() {
                    Firstname = "Daniel",
                    Lastname = "Peña",
                    Email = "admin@admin.com",
                    UserName = "admin@admin.com"
                };
                var result = await _userManager.CreateAsync(user, "P@sswo0rd!");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Couldn't create admin user!");
                }
            }
        }
    }
}
