using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace JobApplicationSpam.Models
{
    public class JobApplicationSpamDbContext : IdentityDbContext<AppUser>
    {
        public JobApplicationSpamDbContext(DbContextOptions<JobApplicationSpamDbContext> options)
            : base(options)
        {
            /*
            var document =
                new Document
                { JobName = "Fachinfo"
                };
            this.Documents.Add(document);
            this.SaveChanges();
            */
        }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<DocumentEmail> DocumentEmail { get; set; }
        public virtual DbSet<CustomVariable> CustomVariables { get; set; }
        public virtual DbSet<DocumentFile> DocumentFiles { get; set; }
        public virtual DbSet<AppUser> AppUsers { get; set; }
        public virtual DbSet<SentApplication> SentApplications { get; set; }
        public virtual DbSet<UserValues> UserValues { get; set; }
        public virtual DbSet<Employer> Employers { get; set; }
    }
}
