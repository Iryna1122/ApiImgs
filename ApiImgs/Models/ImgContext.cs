using ApiImgs.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Image> Images { get; set; } = null!;


    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
        Database.EnsureCreated();   // создаем базу данных при первом обращении
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.UseSqlServer("MyConnectionString");
    //}
}