using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;
using pharaohsLegacy.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace pharaohsLegacy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🆕 الإصلاح الأساسي لمشكلة "&#x627;&#x62E;..." بدل الحروف العربية:
            // الـ HtmlEncoder الافتراضي بيحول أي حرف مش Basic Latin (زي العربي) لـ HTML entity
            // كإجراء أمان افتراضي. ده شغال تمام لو اتحط في نص HTML عادي (المتصفح بيفكه ويعرضه صح)،
            // لكن لو نفس القيمة اتحطت جوه JS string (زي '@Html.L("...")' في <script>) هتفضل
            // زي ما هي حرفيًا (entity مش متفكوكة) لأنها مش HTML text node أصلاً.
            // بنوسع نطاق الترميز هنا ليشمل العربي فيتوقف عن عمل Encode لحروفها.
            builder.Services.AddSingleton(HtmlEncoder.Create(
                UnicodeRanges.BasicLatin,
                UnicodeRanges.Arabic,
                UnicodeRanges.ArabicSupplement,
                UnicodeRanges.ArabicExtendedA,
                UnicodeRanges.ArabicPresentationFormsA,
                UnicodeRanges.ArabicPresentationFormsB));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient("BankService", client =>
            {
                client.BaseAddress = new Uri("http://127.0.0.1:8001/");
            });
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();
            builder.Services.AddSingleton<LocalizationService>();

            // 🆕 Quiz System — بيقرا من AppDbContext (Scoped) عشان يولد الأسئلة وقت الطلب
            builder.Services.AddScoped<QuizQuestionGeneratorService>();

            builder.Services.AddHostedService<pharaohsLegacy.Services.BookingStatusUpdater>();
            builder.Services.AddHostedService<pharaohsLegacy.Services.PendingBookingCleanupService>();

            // 🆕 بيحول الحجوزات Cancelled لـ Refunded تلقائيًا بعد 24 ساعة (راجع BookingStatusService)
            builder.Services.AddHostedService<pharaohsLegacy.Services.BookingRefundBackgroundService>();

            // 🆕 نفس الفكرة بالظبط بس لأوردرات الشوب (راجع ShopOrderStatusService)
            builder.Services.AddHostedService<pharaohsLegacy.Services.ShopOrderRefundBackgroundService>();

            // 🆕 التحديث التلقائي لتراك الشحن (Processing → Shipped → Delivered) حسب المحافظة
            builder.Services.AddHostedService<pharaohsLegacy.Services.ShopOrderShippingBackgroundService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession(); 
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=User}/{action=Login}/{id?}");


          
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();

                if (!context.Pharaohs.Any())
                {
                    context.Pharaohs.AddRange(
                        new Pharaoh { Name = "Ramesses II", Dynasty = "19th Dynasty", Period = "1279–1213 BC", Description = "One of ancient Egypt's greatest pharaohs, known for his military campaigns and monumental building projects.", ImageUrl = "/images/pharaohs/ramesses2_child.jpg" },
                        new Pharaoh { Name = "Tutankhamun", Dynasty = "18th Dynasty", Period = "1332–1323 BC", Description = "The boy king whose intact tomb discovered in 1922 revealed extraordinary treasures of ancient Egypt.", ImageUrl = "/images/pharaohs/tutankhamun_mask.jpg" },
                        new Pharaoh { Name = "Cleopatra VII", Dynasty = "Ptolemaic Dynasty", Period = "51–30 BC", Description = "The last active ruler of the Ptolemaic Kingdom, known for her intelligence and political alliances.", ImageUrl = "/images/pharaohs/cleopatra_berlin.jpg" }
                    );
                    context.SaveChanges();
                }

                if (!context.Temples.Any())
                {
                    context.Temples.AddRange(
                        new Temple { Name = "Karnak Temple", Location = "Luxor", Period = "New Kingdom", Description = "The largest religious building ever constructed, dedicated to the god Amun.", ImageUrl = "/images/temples/karnak2.jpg" },
                        new Temple { Name = "Abu Simbel", Location = "Aswan", Period = "New Kingdom", Description = "Two massive rock temples built by Ramesses II, relocated to avoid flooding from the Nile.", ImageUrl = "/images/temples/abu_simbel_front.jpg" }
                    );
                    context.SaveChanges();
                }

                if (!context.Museums.Any())
                {
                    context.Museums.AddRange(
                        new Museum { Name = "Egyptian Museum", Location = "Cairo", Founded = "1902", Category = "Egyptian", Description = "Home to the world's largest collection of ancient Egyptian antiquities including Tutankhamun's treasures.", ImageUrl = "/images/museums/egyptian_museum2.jpg" },
                        new Museum { Name = "Grand Egyptian Museum", Location = "Giza", Founded = "2023", Category = "Egyptian", Description = "The world's largest archaeological museum, built near the Giza pyramids to house Egypt's ancient treasures.", ImageUrl = "/images/museums/grand_egyptian2.jpg" }
                    );
                    context.SaveChanges();
                }
            }

            app.Run();
        }
    }
}
