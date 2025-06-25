using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Models;

namespace WEB_BanAccGame.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Create database if not exists
                await context.Database.EnsureCreatedAsync();

                // Check if we already have data
                if (context.GameAccounts.Any())
                {
                    return; // DB has been seeded
                }

                // Seed sample game accounts
                var sampleAccounts = new GameAccount[]
                {
                    // MMORPG accounts
                    new GameAccount
                    {
                        Title = "Account Liên Quân VIP - Rank Cao Thủ",
                        Description = "Account Liên Quân Mobile rank Cao Thủ với đầy đủ tướng và skin hiếm. Có 120 tướng, 85 skin.",
                        Price = 2500000,
                        Server = "Server 1",
                        Level = 30,
                        CategoryId = 1,
                        ImageUrl = "https://via.placeholder.com/400x300/FF6B6B/ffffff?text=Lien+Quan+VIP",
                        Username = "lqmvip123",
                        Password = "password123",
                        IsAvailable = true
                    },
                    new GameAccount
                    {
                        Title = "Account PUBG Mobile - Conqueror",
                        Description = "Account PUBG Mobile đã đạt Conqueror nhiều mùa. Có nhiều skin súng và outfit hiếm.",
                        Price = 3500000,
                        Server = "Asia",
                        Level = 65,
                        CategoryId = 4,
                        ImageUrl = "https://via.placeholder.com/400x300/4ECDC4/ffffff?text=PUBG+Conqueror",
                        Username = "pubgpro456",
                        Password = "password456",
                        IsAvailable = true
                    },
                    new GameAccount
                    {
                        Title = "Account Free Fire Max - Full Skin",
                        Description = "Account Free Fire với full skin súng, nhân vật và pet. Đã nạp hơn 50 triệu.",
                        Price = 1800000,
                        Server = "Vietnam",
                        Level = 80,
                        CategoryId = 4,
                        ImageUrl = "https://via.placeholder.com/400x300/95E1D3/ffffff?text=Free+Fire+Max",
                        Username = "ffmax789",
                        Password = "password789",
                        IsAvailable = true
                    },
                    new GameAccount
                    {
                        Title = "Account Genshin Impact AR 58",
                        Description = "Account Genshin Impact AR 58 với nhiều nhân vật 5 sao và vũ khí 5 sao. C6 nhiều nhân vật.",
                        Price = 5000000,
                        Server = "Asia",
                        Level = 58,
                        CategoryId = 1,
                        ImageUrl = "https://via.placeholder.com/400x300/F38181/ffffff?text=Genshin+AR58",
                        Username = "genshin58",
                        Password = "genshinpass",
                        IsAvailable = true
                    },
                    new GameAccount
                    {
                        Title = "Account Valorant - Immortal 3",
                        Description = "Account Valorant rank Immortal 3 với full agent và nhiều skin súng cao cấp.",
                        Price = 2800000,
                        Server = "Singapore",
                        Level = 120,
                        CategoryId = 2,
                        ImageUrl = "https://via.placeholder.com/400x300/AA96DA/ffffff?text=Valorant+Immortal",
                        Username = "valorantpro",
                        Password = "valopass123",
                        IsAvailable = true
                    },
                    new GameAccount
                    {
                        Title = "Account CS:GO Prime - Global Elite",
                        Description = "Account CS:GO Prime Status với rank Global Elite. Có nhiều skin knife và AWP đắt tiền.",
                        Price = 4200000,
                        Server = "SEA",
                        Level = 40,
                        CategoryId = 2,
                        ImageUrl = "https://via.placeholder.com/400x300/FCBAD3/ffffff?text=CSGO+Global",
                        Username = "csgoglobal",
                        Password = "csgopass456",
                        IsAvailable = true
                    },
                    new GameAccount
                    {
                        Title = "Account Mobile Legends - Mythic Glory",
                        Description = "Account Mobile Legends rank Mythic Glory 1000 points. Có 110 hero và nhiều skin Epic, Legend.",
                        Price = 2200000,
                        Server = "Vietnam",
                        Level = 30,
                        CategoryId = 3,
                        ImageUrl = "https://via.placeholder.com/400x300/FFFFD2/ffffff?text=ML+Mythic",
                        Username = "mlmythic",
                        Password = "mlpass789",
                        IsAvailable = true
                    },
                    new GameAccount
                    {
                        Title = "Account Minecraft Premium Full Access",
                        Description = "Account Minecraft Premium với full quyền truy cập. Có thể đổi tên, skin thoải mái.",
                        Price = 500000,
                        Server = "Global",
                        Level = 1,
                        CategoryId = 5,
                        ImageUrl = "https://via.placeholder.com/400x300/A8E6CF/ffffff?text=Minecraft+Premium",
                        Username = "mcpremium",
                        Password = "mcpass123",
                        IsAvailable = true
                    }
                };

                await context.GameAccounts.AddRangeAsync(sampleAccounts);
                await context.SaveChangesAsync();
            }
        }

        public static async Task CreateRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "Customer" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user
            var adminEmail = "admin@gamestore.vn";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true
                };

                var createAdmin = await userManager.CreateAsync(admin, "Admin@123");
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
} 