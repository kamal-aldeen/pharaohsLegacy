using Microsoft.EntityFrameworkCore;

namespace pharaohsLegacy.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Monument> Monuments { get; set; }
        public DbSet<Pharaoh> Pharaohs { get; set; }
        public DbSet<Temple> Temples { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Museum> Museums { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<God> Gods { get; set; }
        public DbSet<Artifact> Artifacts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewHelpful> ReviewHelpfuls { get; set; }
        public DbSet<ReviewReport> ReviewReports { get; set; }
        public DbSet<Dynasty> Dynasties { get; set; }

        public DbSet<HistoricalEvent> HistoricalEvents { get; set; }

        public DbSet<DailyFact> DailyFacts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ShopOrder> ShopOrders { get; set; }
        public DbSet<ShopPayment> ShopPayments { get; set; }
        public DbSet<CartItem> CartItems { get; set; } // 🆕 سلة المشتريات
        public DbSet<ShopOrderItem> ShopOrderItems { get; set; } // 🆕 عناصر الأوردر (منتج واحد أو أكتر)



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Pharaoh>().HasData(
                new Pharaoh
                {
                    Id = 1,
                    Name = "Ramesses II",
                    Dynasty = "19th Dynasty",
                    Period = "1279–1213 BC",
                    Description = "Known as Ramesses the Great, he reigned for 66 years and built the temples of Abu Simbel.",
                    ImageUrl = "/images/pharaohs/ramesses2_child2.jpg"
                },
                new Pharaoh
                {
                    Id = 2,
                    Name = "Tutankhamun",
                    Dynasty = "18th Dynasty",
                    Period = "1332–1323 BC",
                    Description = "The boy pharaoh whose intact tomb, discovered in 1922, revealed the splendors of ancient Egypt.",
                    ImageUrl = "/images/pharaohs/tutankhamun_crop.jpg"
                },
                new Pharaoh
                {
                    Id = 3,
                    Name = "Cleopatra VII",
                    Dynasty = "Ptolemaic Dynasty",
                    Period = "51–30 BC",
                    Description = "The last active ruler of the Ptolemaic Kingdom of Egypt, known for her intelligence and alliances with Rome.",
                    ImageUrl = "/images/pharaohs/cleopatra.jpg"
                }
            );

            
            modelBuilder.Entity<Temple>().HasData(
                new Temple
                {
                    Id = 1,
                    Name = "Karnak Temple",
                    Location = "Luxor, Egypt",
                    Period = "2055–100 BC",
                    Description = "The largest ancient religious site in the world, dedicated to the god Amun.",
                    ImageUrl = "/images/temples/karnak3.jpg"
                    
                },
                new Temple
                {
                    Id = 2,
                    Name = "Abu Simbel",
                    Location = "Aswan, Egypt",
                    Period = "1264–1244 BC",
                    Description = "Two massive rock temples built by Ramesses II, relocated to avoid flooding from the Nile.",
                    ImageUrl = "/images/temples/abusimbel.jpg"
                   
                },
                new Temple
                {
                    Id = 3,
                    Name = "Luxor Temple",
                    Location = "Luxor, Egypt",
                    Period = "1400 BC",
                    Description = "A large ancient Egyptian temple complex located on the east bank of the Nile River.",
                    ImageUrl = "/images/temples/luxor2.jpg"
                    
                }
            );
        }
    }
}