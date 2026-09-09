using ClyvoVet.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.API.Infrastructure.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<Tutor> Tutores { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Medicacao> Medicacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tutor>(entity =>
            {
                entity.Property(x => x.IdTutor).ValueGeneratedNever();
                entity.Property(x => x.Nome).HasColumnType("VARCHAR2(100)");
                entity.Property(x => x.Email).HasColumnType("VARCHAR2(100)");
                entity.Property(x => x.Telefone).HasColumnType("VARCHAR2(20)");
                entity.Property(x => x.Cpf).HasColumnType("VARCHAR2(14)");
                entity.Property(x => x.Senha).HasColumnType("VARCHAR2(100)");
            });

            modelBuilder.Entity<Pet>(entity =>
            {
                entity.Property(x => x.IdPet).ValueGeneratedNever();
                entity.Property(x => x.Nome).HasColumnType("VARCHAR2(100)");
                entity.Property(x => x.Especie).HasColumnType("VARCHAR2(50)");
                entity.Property(x => x.Raca).HasColumnType("VARCHAR2(50)");
                entity.Property(x => x.DataNascimento).HasColumnType("DATE");
                entity.Property(x => x.PesoKg).HasColumnType("NUMBER(5,2)").HasPrecision(5, 2);

                entity.HasOne(x => x.Tutor)
                    .WithMany(x => x.Pets)
                    .HasForeignKey(x => x.IdTutor)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_PET_TUTOR");
            });

            modelBuilder.Entity<Consulta>(entity =>
            {
                entity.Property(x => x.IdConsulta).ValueGeneratedNever();
                entity.Property(x => x.DataConsulta).HasColumnType("DATE");
                entity.Property(x => x.Veterinario).HasColumnType("VARCHAR2(100)");
                entity.Property(x => x.Observacoes).HasColumnType("VARCHAR2(300)");

                entity.HasOne(x => x.Pet)
                    .WithMany(x => x.Consultas)
                    .HasForeignKey(x => x.IdPet)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_CONSULTA_PET");
            });

            modelBuilder.Entity<Medicacao>(entity =>
            {
                entity.Property(x => x.IdMedicacao).ValueGeneratedNever();
                entity.Property(x => x.Nome).HasColumnType("VARCHAR2(100)");
                entity.Property(x => x.Dose).HasColumnType("VARCHAR2(50)");
                entity.Property(x => x.Frequencia).HasColumnType("VARCHAR2(50)");
                entity.Property(x => x.DataInicio).HasColumnType("DATE");
                entity.Property(x => x.DataFim).HasColumnType("DATE");

                entity.HasOne(x => x.Pet)
                    .WithMany(x => x.Medicacoes)
                    .HasForeignKey(x => x.IdPet)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_MEDICACAO_PET");
            });
        }
    }
}
