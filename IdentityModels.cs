using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JobBoardMVC.Models
{
    // You can add profile data for the user by adding more properties to your User class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser
    {
        [JsonProperty(PropertyName="FirstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName ="LastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName ="Location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName ="Languages")]
        public string Languages { get; set; }

        [JsonProperty(PropertyName ="Experience")]
        public string Experience { get; set; }

        //resume identity model addition
        [JsonProperty(PropertyName ="Resume")]
        public byte Resume { get; set; }

        public virtual ICollection<ResumeModel> Resumes { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class JobBoardMvcContext : IdentityDbContext<User>
    {
        public JobBoardMvcContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static JobBoardMvcContext Create()
        {
            return new JobBoardMvcContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("User").Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRole");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogin");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaim").Property(p => p.Id).HasColumnName("UserClaimId");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles").Property(p => p.Id).HasColumnName("RoleId");


        }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<SavedCompany> SavedCompanies { get; set; }
        public DbSet<ResumeModel> Resumes { get; set; }

    }
}