using NLog;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FinalProject.Model;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.CompilerServices;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

string choice = "";
logger.Info("Program started");
do
{
    Console.WriteLine("Select Mode:");
    Console.WriteLine("1) Categories");
    Console.WriteLine("2) Products");
    choice = Console.ReadLine() ?? "";
    Console.Clear();

    // user selected categories
    if (choice == "1")
    {
        logger.Info("Categories option selected");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Categories Selected!");
        Console.WriteLine("1) Display categories");
        Console.WriteLine("2) Add category");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Display all Categories and their related products");
        choice = Console.ReadLine() ?? "";
        Console.Clear();

        if (choice == "1")
        {
            logger.Info("Display Categories option selected");
            Console.WriteLine();
            Console.WriteLine();

            // display categories
            var configuration = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.json");

            var config = configuration.Build();

            var db = new DataContext();
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.WriteLine("Displaying Categories");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query) Console.WriteLine($"{item.CategoryName} - {item.Description}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (choice == "2")
        {
            logger.Info("Add Category option selected");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Adding new Category");
            // Add category
            Category category = new();
            Console.Write("Enter Category Name: ");
            category.CategoryName = Console.ReadLine() ?? "";
            Console.Write("Enter the Category Description: ");
            category.Description = Console.ReadLine() ?? "";
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                var db = new DataContext();
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", ["CategoryName"]));
                }
                else
                {
                    logger.Info($"Validation passed, adding category {category.CategoryName}");
                    db.addCategory(category);
                }
            }
            if (!isValid)
            {
                foreach (var result in results) logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
            }
        }
        else if (choice == "3")
        {
            logger.Info("Display category and related objects option selected");
            Console.WriteLine();
            Console.WriteLine();
            var db = new DataContext();
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query) Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine()!);
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Console.WriteLine();
            Console.WriteLine();
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
            Console.WriteLine($"Category: {category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products.Where(p => !p.Discontinued)) Console.WriteLine($"\t{p.ProductName}");
        }
        else if (choice == "4")
        {
            logger.Info("Display all Categories and objects option selected");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Displaying All Categories and Products");
            var db = new DataContext();
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products.Where(p => !p.Discontinued)) Console.WriteLine($"\t{p.ProductName}");
            }
        }
        else if (String.IsNullOrEmpty(choice))
        {
            break;
        }
        Console.WriteLine();
    }

    // user selected products
    else if (choice == "2")
    {
        logger.Info("Products option selected");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Products Selected!");
        Console.WriteLine("1) Display products");
        Console.WriteLine("2) Add Product");
        Console.WriteLine("3) Edit Product");
        choice = Console.ReadLine() ?? "";
        Console.Clear();
        if (choice == "1")
        {
            logger.Info("Display products option selected");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Display which products (d: discontinued, c: current, default: all):");
            choice = Console.ReadLine() ?? "";
            // display products
            var configuration = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.json");

            var config = configuration.Build();

            var db = new DataContext();
            var query = db.Products.OrderBy(p => p.ProductName).ToList();
            if (choice == "d") query = query.Where(p => p.Discontinued).ToList();
            else if (choice == "c") query = query.Where(p => !p.Discontinued).ToList();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                if (item.Discontinued) Console.ForegroundColor = ConsoleColor.Red;
                else Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{item.ProductName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        if (choice == "2")
        {
            var db = new DataContext();
            List<Category> categories = db.Categories.OrderBy(p => p.CategoryId).ToList();
            List<Supplier> suppliers = db.Suppliers.OrderBy(p => p.SupplierId).ToList();

            logger.Info("Add product option selected");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Adding Product");
            // Add product
            Product product = new();
            Console.Write("Enter Product Name: ");
            product.ProductName = Console.ReadLine() ?? "";
            Console.WriteLine("Categories");
            foreach (var item in categories) Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            Console.Write("Select product's Category ID: ");
            product.CategoryId = int.Parse(Console.ReadLine() ?? "0");
            Console.WriteLine("Suppliers");
            foreach (var item in suppliers) Console.WriteLine($"{item.SupplierId}) {item.CompanyName}");
            Console.Write("Select product's Supplier ID: ");
            product.SupplierId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Unit Price: $");
            product.UnitPrice = Decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Quantity Per Unit: ");
            product.QuantityPerUnit = Console.ReadLine();
            Console.Write("Enter Units In Stock: ");
            product.UnitsInStock = short.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Units On Order: ");
            product.UnitsOnOrder = short.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Reorder Level: ");
            product.ReorderLevel = short.Parse(Console.ReadLine() ?? "0");
            Console.Write("Discontinued? (y/n - default)");
            product.Discontinued = Console.ReadLine() == "y";

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(product, context, results, true);
            if (!categories.Exists(c => c.CategoryId == product.CategoryId))
            {
                isValid = false;
                results.Add(new ValidationResult("No such category", ["CategoryID"]));
            }
            if (!suppliers.Exists(c => c.SupplierId == product.SupplierId))
            {
                isValid = false;
                results.Add(new ValidationResult("No such supplier", ["SupplierID"]));
            }
            if (isValid)
            {
                logger.Info($"Validation passed, adding product {product.ProductName}");
                db.addProduct(product);
            }
            else
            {
                foreach (var result in results) logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
            }

        }
        if (choice == "3")
        {
            logger.Info("Edit product option selected");
            Console.WriteLine();
            Console.WriteLine();
            var db = new DataContext();
            List<Product> products = db.Products.OrderBy(p => p.ProductId).ToList();
            List<Category> categories = db.Categories.OrderBy(p => p.CategoryId).ToList();
            List<Supplier> suppliers = db.Suppliers.OrderBy(p => p.SupplierId).ToList();
            Console.WriteLine("Adding Product");
            // Add product
            Product product = new();
            Console.WriteLine("Products");
            foreach (var item in products) Console.WriteLine($"{item.ProductId}) {item.ProductName}");
            Console.Write("Select product to edit: ");
            product.ProductId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Product Name: ");
            product.ProductName = Console.ReadLine() ?? "";
            Console.WriteLine("Categories");
            foreach (var item in categories)Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            Console.Write("Select product's Category ID: ");
            product.CategoryId = int.Parse(Console.ReadLine() ?? "0");
            Console.WriteLine("Suppliers");
            foreach (var item in suppliers) Console.WriteLine($"{item.SupplierId}) {item.CompanyName}");
            Console.Write("Select product's Supplier ID: ");
            product.SupplierId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Unit Price: $");
            product.UnitPrice = Decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Quantity Per Unit: ");
            product.QuantityPerUnit = Console.ReadLine();
            Console.Write("Enter Units In Stock: ");
            product.UnitsInStock = short.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Units On Order: ");
            product.UnitsOnOrder = short.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Reorder Level: ");
            product.ReorderLevel = short.Parse(Console.ReadLine() ?? "0");
            Console.Write("Discontinued? (y/n - default)");
            product.Discontinued = Console.ReadLine() == "y";

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(product, context, results, true);
            if (!products.Exists(c => c.ProductId == product.ProductId))
            {
                isValid = false;
                results.Add(new ValidationResult("No existing product", ["ProductID"]));
            }
            if (!categories.Exists(c => c.CategoryId == product.CategoryId))
            {
                isValid = false;
                results.Add(new ValidationResult("No such category", ["CategoryID"]));
            }
            if (!suppliers.Exists(c => c.SupplierId == product.SupplierId))
            {
                isValid = false;
                results.Add(new ValidationResult("No such supplier", ["SupplierID"]));
            }
            if (isValid)
            {
                logger.Info($"Validation passed, editing product {product.ProductName}");
                db.editProduct(product);
            }
            else
            {
                foreach (var result in results) logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
            }

        }

    }
    else if (choice == "") break;
    Console.WriteLine();
    Console.Write("Press any key to continue...");
    Console.ReadKey();
    Console.Clear();
} while (true);


logger.Info("Program ended");

