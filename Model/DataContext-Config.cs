using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace FinalProject.Model;
public partial class DataContext : DbContext
{
  public DataContext() { }
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");
    var config = configuration.Build();
    optionsBuilder.UseSqlServer(@config["Northwind:ConnectionString"]);
  }

  public void addCategory(Category category){
    Categories.Add(category);
    SaveChanges();
  }

  public void addProduct(Product product){
    Products.Add(product);
    SaveChanges();
  }
}