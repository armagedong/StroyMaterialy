using Npgsql;
using StroyMaterialy.Models;

namespace StroyMaterialy.Services;

public static class OrderService
{
    public static async Task<List<Order>> GetAllAsync()
    {
        const string sql = """
            SELECT o.id, o.order_number, o.order_date, o.delivery_date,
                   COALESCE(pp.address, ''), o.client_full_name, COALESCE(o.pickup_code, ''), o.status,
                   o.pickup_point_id
            FROM orders o
            LEFT JOIN pickup_points pp ON pp.id = o.pickup_point_id
            ORDER BY o.order_number
            """;

        var orders = new List<Order>();
        await using var conn = DatabaseService.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(new Order
            {
                Id = reader.GetInt32(0),
                OrderNumber = reader.GetInt32(1),
                OrderDate = reader.GetDateTime(2),
                DeliveryDate = reader.GetDateTime(3),
                PickupAddress = reader.GetString(4),
                ClientFullName = reader.GetString(5),
                PickupCode = reader.GetString(6),
                Status = reader.GetString(7),
                PickupPointId = reader.IsDBNull(8) ? null : reader.GetInt32(8)
            });
        }

        foreach (var order in orders)
            order.Items = await GetItemsAsync(order.Id);

        foreach (var order in orders)
            order.ItemsDescription = string.Join(", ",
                order.Items.Select(i => $"{i.Article} x{i.Quantity}"));

        return orders;
    }

    public static async Task<List<PickupPoint>> GetPickupPointsAsync()
    {
        var list = new List<PickupPoint>();
        await using var conn = DatabaseService.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT id, address FROM pickup_points ORDER BY id", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(new PickupPoint { Id = reader.GetInt32(0), Address = reader.GetString(1) });
        return list;
    }

    public static async Task SaveAsync(Order order)
    {
        await using var conn = DatabaseService.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            if (order.Id == 0)
            {
                const string insert = """
                    INSERT INTO orders (order_number, order_date, delivery_date, pickup_point_id,
                        client_full_name, pickup_code, status)
                    VALUES (@num, @od, @dd, @pp, @client, @code, @status)
                    RETURNING id
                    """;
                await using var cmd = new NpgsqlCommand(insert, conn, tx);
                AddOrderParams(cmd, order);
                order.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            else
            {
                const string update = """
                    UPDATE orders SET order_number=@num, order_date=@od, delivery_date=@dd,
                        pickup_point_id=@pp, client_full_name=@client, pickup_code=@code, status=@status
                    WHERE id=@id
                    """;
                await using var cmd = new NpgsqlCommand(update, conn, tx);
                cmd.Parameters.AddWithValue("id", order.Id);
                AddOrderParams(cmd, order);
                await cmd.ExecuteNonQueryAsync();

                await using var del = new NpgsqlCommand("DELETE FROM order_items WHERE order_id=@id", conn, tx);
                del.Parameters.AddWithValue("id", order.Id);
                await del.ExecuteNonQueryAsync();
            }

            foreach (var item in order.Items)
            {
                await using var itemCmd = new NpgsqlCommand(
                    "INSERT INTO order_items (order_id, product_id, quantity) VALUES (@oid, @pid, @qty)",
                    conn, tx);
                itemCmd.Parameters.AddWithValue("oid", order.Id);
                itemCmd.Parameters.AddWithValue("pid", item.ProductId);
                itemCmd.Parameters.AddWithValue("qty", item.Quantity);
                await itemCmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public static async Task DeleteAsync(int id)
    {
        await using var conn = DatabaseService.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM orders WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static void AddOrderParams(NpgsqlCommand cmd, Order order)
    {
        cmd.Parameters.AddWithValue("num", order.OrderNumber);
        cmd.Parameters.AddWithValue("od", order.OrderDate.Date);
        cmd.Parameters.AddWithValue("dd", order.DeliveryDate.Date);
        cmd.Parameters.AddWithValue("pp", (object?)order.PickupPointId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("client", order.ClientFullName);
        cmd.Parameters.AddWithValue("code", order.PickupCode);
        cmd.Parameters.AddWithValue("status", order.Status);
    }

    private static async Task<List<OrderItem>> GetItemsAsync(int orderId)
    {
        const string sql = """
            SELECT oi.id, oi.product_id, p.article, p.name, oi.quantity
            FROM order_items oi
            JOIN products p ON p.id = oi.product_id
            WHERE oi.order_id = @id
            """;

        var items = new List<OrderItem>();
        await using var conn = DatabaseService.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", orderId);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new OrderItem
            {
                Id = reader.GetInt32(0),
                OrderId = orderId,
                ProductId = reader.GetInt32(1),
                Article = reader.GetString(2),
                ProductName = reader.GetString(3),
                Quantity = reader.GetInt32(4)
            });
        }
        return items;
    }
}
