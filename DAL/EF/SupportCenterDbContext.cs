using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC.BL.Domain;

namespace SC.DAL.EF
{
  [DbConfigurationType(typeof(SupportCenterDbConfiguration))]
  internal class SupportCenterDbContext : DbContext /* 'public' for testing with project 'DAL-Testing'! */
  {
    public SupportCenterDbContext() 
      : base("SupportCenterDB_EFCodeFirst")
    {
      //Database.SetInitializer<SupportCenterDbContext>(new SupportCenterDbInitializer()); // moved to 'SupportCenterDbConfiguration'
    }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<HardwareTicket> HardwareTickets { get; set; }
    public DbSet<TicketResponse> TicketResponses { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      //base.OnModelCreating(modelBuilder); // does nothing! (empty body)

      // Remove pluralizing tablenames
      modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

      // Remove cascading delete for all required-relationships
      modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
      modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

      // 'Ticket.TicketNumber' as unique identifier
      modelBuilder.Entity<Ticket>().HasKey(t => t.TicketNumber);

      // 'Ticket.State' as index
      modelBuilder.Entity<Ticket>().Property(t => t.State)
                                   .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute()));
    }
  }
}
