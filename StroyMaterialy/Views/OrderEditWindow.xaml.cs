using System.Collections.ObjectModel;
using System.Windows;
using StroyMaterialy.Models;
using StroyMaterialy.Services;

namespace StroyMaterialy.Views;

public partial class OrderEditWindow : Window
{
    private readonly Order _order;
    private readonly ObservableCollection<OrderItem> _items = [];
    private List<Product> _products = [];

    public OrderEditWindow(Order order)
    {
        InitializeComponent();
        _order = order;
        Title = order.Id == 0 ? "Заказ — добавление" : "Заказ — редактирование";
        Loaded += async (_, _) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        var points = await OrderService.GetPickupPointsAsync();
        PickupCombo.ItemsSource = points;
        if (_order.PickupPointId.HasValue)
            PickupCombo.SelectedItem = points.FirstOrDefault(p => p.Id == _order.PickupPointId);

        _products = await ProductService.GetAllAsync();
        ProductCombo.ItemsSource = _products;

        NumberBox.Text = _order.OrderNumber == 0 ? "" : _order.OrderNumber.ToString();
        OrderDatePicker.SelectedDate = _order.OrderDate;
        DeliveryDatePicker.SelectedDate = _order.DeliveryDate;
        ClientBox.Text = _order.ClientFullName;
        CodeBox.Text = _order.PickupCode;
        StatusBox.Text = string.IsNullOrWhiteSpace(_order.Status) ? "Новый" : _order.Status;

        foreach (var item in _order.Items)
            _items.Add(new OrderItem
            {
                Id = item.Id,
                OrderId = item.OrderId,
                ProductId = item.ProductId,
                Article = item.Article,
                ProductName = item.ProductName,
                Quantity = item.Quantity
            });

        ItemsGrid.ItemsSource = _items;
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        if (ProductCombo.SelectedItem is not Product product)
            return;

        if (!int.TryParse(QtyBox.Text, out var qty) || qty <= 0)
        {
            MessageBox.Show("Укажите количество.", "Внимание");
            return;
        }

        var existing = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing != null)
            existing.Quantity += qty;
        else
            _items.Add(new OrderItem
            {
                ProductId = product.Id,
                Article = product.Article,
                ProductName = product.Name,
                Quantity = qty
            });
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsGrid.SelectedItem is OrderItem item)
            _items.Remove(item);
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(NumberBox.Text, out var number) || number <= 0)
        {
            MessageBox.Show("Укажите номер заказа.", "Ошибка");
            return;
        }

        if (OrderDatePicker.SelectedDate == null || DeliveryDatePicker.SelectedDate == null)
        {
            MessageBox.Show("Укажите даты.", "Ошибка");
            return;
        }

        if (_items.Count == 0)
        {
            MessageBox.Show("Добавьте позиции в заказ.", "Ошибка");
            return;
        }

        _order.OrderNumber = number;
        _order.OrderDate = OrderDatePicker.SelectedDate.Value;
        _order.DeliveryDate = DeliveryDatePicker.SelectedDate.Value;
        _order.PickupPointId = (PickupCombo.SelectedItem as PickupPoint)?.Id;
        _order.ClientFullName = ClientBox.Text.Trim();
        _order.PickupCode = CodeBox.Text.Trim();
        _order.Status = StatusBox.Text.Trim();
        _order.Items = _items.ToList();

        try
        {
            await OrderService.SaveAsync(_order);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка");
        }
    }
}
