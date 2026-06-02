using Npgsql;
using StroyMaterialy.Models;

namespace StroyMaterialy.Services;

public static class ProductService
{
    public static async Task<List<Product>> GetAllAsync()
    {
        const string sql = """
            SELECT id, article, name, unit, price, supplier, manufacturer, category,
                   discount_percent, quantity_in_stock, COALESCE(description, ''), COALESCE(image_file, '')
            FROM products
            ORDER BY name
            """;

        var list = new List<Product>();
        await using var conn = DatabaseService.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(ReadProduct(reader));
        return list;
    }

    public static async Task SaveAsync(Product product)
    {
        if (product.Id == 0)
        {
            const string insert = """
                INSERT INTO products (article, name, unit, price, supplier, manufacturer, category,
                    discount_percent, quantity_in_stock, description, image_file)
                VALUES (@article, @name, @unit, @price, @supplier, @manufacturer, @category,
                    @discount, @qty, @description, @image)
                """;
            await ExecuteAsync(insert, product);
        }
        else
        {
            const string update = """
                UPDATE products SET article=@article, name=@name, unit=@unit, price=@price,
                    supplier=@supplier, manufacturer=@manufacturer, category=@category,
                    discount_percent=@discount, quantity_in_stock=@qty,
                    description=@description, image_file=@image
                WHERE id=@id
                """;
            await ExecuteAsync(update, product);
        }
    }

    public static async Task DeleteAsync(int id)
    {
        await using var conn = DatabaseService.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM products WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task ExecuteAsync(string sql, Product product)
    {
        await using var conn = DatabaseService.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("article", product.Article);
        cmd.Parameters.AddWithValue("name", product.Name);
        cmd.Parameters.AddWithValue("unit", product.Unit);
        cmd.Parameters.AddWithValue("price", product.Price);
        cmd.Parameters.AddWithValue("supplier", product.Supplier);
        cmd.Parameters.AddWithValue("manufacturer", product.Manufacturer);
        cmd.Parameters.AddWithValue("category", product.Category);
        cmd.Parameters.AddWithValue("discount", product.DiscountPercent);
        cmd.Parameters.AddWithValue("qty", product.QuantityInStock);
        cmd.Parameters.AddWithValue("description", product.Description);
        cmd.Parameters.AddWithValue("image", product.ImageFile);
        if (product.Id > 0)
            cmd.Parameters.AddWithValue("id", product.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static Product ReadProduct(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Article = reader.GetString(1),
        Name = reader.GetString(2),
        Unit = reader.GetString(3),
        Price = reader.GetDecimal(4),
        Supplier = reader.GetString(5),
        Manufacturer = reader.GetString(6),
        Category = reader.GetString(7),
        DiscountPercent = reader.GetInt32(8),
        QuantityInStock = reader.GetInt32(9),
        Description = reader.GetString(10),
        ImageFile = reader.GetString(11)
    };
}
