using Microsoft.EntityFrameworkCore;
using TotemPWA.Models; // Certifique-se de que este using está presente

namespace TotemPWA.Data
{
    public class ApplicationDbContext : DbContext // O nome da sua classe de contexto
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Seus DbSets para os modelos
        public DbSet<Additional> Additionals { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<Cupom> Cupons { get; set; }
        public DbSet<Customize> Customizes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        // Se você tiver um modelo Payment, adicione-o aqui também:
        // public DbSet<Payment> Payments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração para chaves compostas e relações
            // Exemplo para Additional (chave composta)
            modelBuilder.Entity<Additional>()
                .HasKey(a => new { a.ProductId, a.IngredientId });

            modelBuilder.Entity<Additional>()
                .HasOne(a => a.Product)
                .WithMany(p => p.Additionals)
                .HasForeignKey(a => a.ProductId);

            modelBuilder.Entity<Additional>()
                .HasOne(a => a.Ingredient)
                .WithMany(i => i.Additionals)
                .HasForeignKey(a => a.IngredientId);

            // Exemplo para Combo (chave composta)
            modelBuilder.Entity<Combo>()
                .HasKey(c => new { c.ProductComboId, c.ProductId }); // Chave composta para ProductComboId e ProductId

            modelBuilder.Entity<Combo>()
                .HasOne(c => c.ProductCombo)
                .WithMany(p => p.ProductCombos)
                .HasForeignKey(c => c.ProductComboId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata acidental

            modelBuilder.Entity<Combo>()
                .HasOne(c => c.Product)
                .WithMany(p => p.ComposedCombos)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata acidental

            // Configuração para Employee (ClientId como chave primária e estrangeira)
            modelBuilder.Entity<Employee>()
                .HasKey(e => e.ClientId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Client)
                .WithOne(c => c.Employee)
                .HasForeignKey<Employee>(e => e.ClientId);

            // Configuração para Category (hierarquia e Slug)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Subcategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Ou .OnDelete(DeleteBehavior.SetNull); se preferir que subcategorias fiquem sem pai ao excluir o pai.
            
            // Garante que o slug é único
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            // Configuração para Cupom (código único)
            modelBuilder.Entity<Cupom>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Configuração para Ingredient (nome único)
            modelBuilder.Entity<Ingredient>()
                .HasIndex(i => i.Name)
                .IsUnique();

            // Configuração para Customize
            modelBuilder.Entity<Customize>()
                .HasOne(c => c.OrderItem)
                .WithMany(oi => oi.Customizations)
                .HasForeignKey(c => c.OrderItemId);

            modelBuilder.Entity<Customize>()
                .HasOne(c => c.Ingredient)
                .WithMany(i => i.Customizations)
                .HasForeignKey(c => c.IngredientId);

            // Configuração para Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Client)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ClientId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cupom)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CupomId)
                .IsRequired(false); // Cupom é opcional

            // Configuração para OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);

            // Configuração para Promotion
            modelBuilder.Entity<Promotion>()
                .HasOne(p => p.Product)
                .WithMany(prod => prod.Promotions)
                .HasForeignKey(p => p.ProductId);

                // Dentro de OnModelCreating(ModelBuilder modelBuilder)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId);


        }
    }
}