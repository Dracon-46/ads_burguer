using Microsoft.EntityFrameworkCore;
using TotemPWA.Models; // Certifique-se de que este using está presente

namespace TotemPWA.Data
{
    public class ApplicationDbContext : DbContext
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
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Adicionado ou ajustado para garantir integridade.

            modelBuilder.Entity<Additional>()
                .HasOne(a => a.Ingredient)
                .WithMany(i => i.Additionals)
                .HasForeignKey(a => a.IngredientId)
                .OnDelete(DeleteBehavior.Cascade); // Adicionado ou ajustado para garantir integridade.


            // Exemplo para Combo (chave composta)
            modelBuilder.Entity<Combo>()
                .HasKey(c => new { c.ProductComboId, c.ProductId });

            modelBuilder.Entity<Combo>()
                .HasOne(c => c.ProductCombo)
                .WithMany(p => p.ProductCombos)
                .HasForeignKey(c => c.ProductComboId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Combo>()
                .HasOne(c => c.Product)
                .WithMany(p => p.ComposedCombos)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuração para Employee (ClientId como chave primária e estrangeira)
            modelBuilder.Entity<Employee>()
                .HasKey(e => e.ClientId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Client)
                .WithOne(c => c.Employee)
                .HasForeignKey<Employee>(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade); // Geralmente Cascade para relações One-to-One onde o dependente não existe sem o principal


            // Configuração para Category (hierarquia e Slug)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Subcategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // **IMPORTANTE: Configuração para Customize**
            // OrderItemId agora é Guid
            modelBuilder.Entity<Customize>()
                .HasOne(c => c.OrderItem)
                .WithMany(oi => oi.Customizations)
                .HasForeignKey(c => c.OrderItemId) // <<-- O tipo da FK aqui agora é Guid
                .OnDelete(DeleteBehavior.Cascade); // Normalmente Cascade para itens de detalhe de um pedido

            modelBuilder.Entity<Customize>()
                .HasOne(c => c.Ingredient)
                .WithMany(i => i.Customizations)
                .HasForeignKey(c => c.IngredientId)
                .OnDelete(DeleteBehavior.Cascade); // Normalmente Cascade

            // Configuração para Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Client)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ClientId)
                .OnDelete(DeleteBehavior.Cascade); // Geralmente Cascade para Orders com Client

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cupom)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CupomId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); // Se o cupom for opcional, SetNull é uma boa opção ao apagar o cupom


            // **IMPORTANTE: Configuração para OrderItem**
            // OrderItem.Id agora é Guid, o EF Core geralmente infere corretamente,
            // mas podemos reforçar ou apenas garantir que não haja configurações conflitantes.
            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.Id); // Garante que Id é a PK (agora Guid)

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Geralmente Cascade para itens de um pedido

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict aqui para não apagar o produto ao apagar um OrderItem


            // Configuração para Promotion
            modelBuilder.Entity<Promotion>()
                .HasOne(p => p.Product)
                .WithMany(prod => prod.Promotions)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Geralmente Cascade


            // Configuração para Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Geralmente Cascade
        }
    }
}