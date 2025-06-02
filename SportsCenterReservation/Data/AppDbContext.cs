using Microsoft.EntityFrameworkCore;
using SportsCenterReservation.Models;

namespace SportsCenterReservation.Data
{
    public class AppDbContext : DbContext
    {
        // Tabela pentru gestionarea utilizatorilor
        public DbSet<User> Users { get; set; }

        // Tabela pentru gestionarea rezervarilor
        public DbSet<Rezervare> Rezervari { get; set; }

        // Tabela pentru serviciile oferite (ex: tenis, fitness)
        public DbSet<Serviciu> Servicii { get; set; }

        // Constructor care primeste optiuni de configurare pentru baza de date
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurarea relatiei dintre Rezervare si User
            modelBuilder.Entity<Rezervare>()
                .HasOne(r => r.User) // O rezervare apartine unui singur User
                .WithMany() // Un User poate avea mai multe Rezervari (daca exista navigare inversa)
                .HasForeignKey(r => r.UserId) // Cheia straina este UserId
                .OnDelete(DeleteBehavior.Restrict); // Blocheaza stergerea Userului daca are rezervari
        }
    }
}