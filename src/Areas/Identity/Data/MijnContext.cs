using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using src.Models;

public class MijnContext : IdentityDbContext<srcUser>{

    public MijnContext(DbContextOptions<MijnContext> options) : base(options)
    {
    }
    public DbSet<Afspraak> Afspraken{get; set;}
    public DbSet<Chat> Chat {get;set;}
    public DbSet<Message> Messages {get;set;}
    public DbSet<ChatUser> ChatUsers{get;set;}
    public DbSet<Melding> Meldingen{get;set;}
    public DbSet<Aanmelding> Aanmeldingen{get;set;}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ChatUser>()
                        .HasKey(x=>new{x.UserId, x.ChatId});
        builder.Entity<srcUser>()
                    .HasMany(x=>x.Childeren)
                    .WithOne(x=>x.Parent);
        builder.Entity<srcUser>()
                    .HasMany(x=>x.Clients)
                    .WithOne(x=>x.Specialist);
        builder.Entity<Aanmelding>()
                    .HasOne(x=>x.Client)
                    .WithMany(x=>x.AanmeldingenClients)
                    .HasForeignKey(x=>x.ClientId);
        builder.Entity<Aanmelding>()
                    .HasOne(x=>x.Pedagoog)
                    .WithMany(x=>x.AanmeldingPedagoog)
                    .HasForeignKey(x=>x.PedagoogId);
    }
}