using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GurmederApi.Models;

namespace GurmederApi.Data;

public class GurmederDbContext : DbContext
{
    public GurmederDbContext(DbContextOptions<GurmederDbContext> options) : base(options) { }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasIndex(r => r.Name).IsUnique();

            entity.HasOne(r => r.Category)
                .WithMany(c => c.Recipes)
                .HasForeignKey(r => r.CategoryId)
                // Bir kategoriye bağlı tarif varsa o kategori silinemesin —
                // CategoriesController zaten bunu 409 ile önceden engelliyor,
                // bu Restrict veritabanı seviyesinde ikinci bir güvenlik ağı.
                .OnDelete(DeleteBehavior.Restrict);

            // Tags basit bir string listesi; ayrı bir tablo yerine JSON
            // metni olarak tek sütunda saklanır. Dönüşüm ve karşılaştırma
            // mantığı, olası aşırı yükleme (overload) belirsizliğinden
            // kaçınmak için önce ayrı, somut nesneler olarak kuruluyor,
            // sonra property'ye uygulanıyor.
            var tagsConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            // EF Core, dönüştürülmüş (converted) koleksiyon özelliklerinde
            // değişiklikleri doğru izleyebilmek için bir ValueComparer
            // ister — bu olmadan EF Core listenin içeriği değişse bile
            // "değişmedi" sanabilir.
            var tagsComparer = new ValueComparer<List<string>>(
                (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
                v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                v => v.ToList());

            entity.Property(r => r.Tags)
                .HasConversion(tagsConverter)
                .Metadata.SetValueComparer(tagsComparer);

            entity.HasMany(r => r.Ingredients)
                .WithOne(i => i.Recipe)
                .HasForeignKey(i => i.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Steps)
                .WithOne(s => s.Recipe)
                .HasForeignKey(s => s.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeIngredient>()
            .HasIndex(i => i.RecipeId);

        modelBuilder.Entity<RecipeStep>()
            .HasIndex(s => s.RecipeId);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            // FavoriteRecipeIds, Tags ile aynı desende (bkz. yukarısı):
            // ayrı bir join tablosu yerine tek bir JSON sütun. Burada
            // List<int> olduğundan Tags'ın List<string> converter/comparer
            // çiftinden ayrı, kendi somut nesneleri gerekir (jenerik tipler
            // farklı olduğu için doğrudan paylaşılamazlar).
            var favConverter = new ValueConverter<List<int>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>());

            var favComparer = new ValueComparer<List<int>>(
                (a, b) => (a ?? new List<int>()).SequenceEqual(b ?? new List<int>()),
                v => v.Aggregate(0, (hash, id) => HashCode.Combine(hash, id)),
                v => v.ToList());

            entity.Property(u => u.FavoriteRecipeIds)
                .HasConversion(favConverter)
                .Metadata.SetValueComparer(favComparer);
        });
    }
}
