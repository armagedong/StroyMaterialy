using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using StroyMaterialy.Helpers;
using StroyMaterialy.Models;
using StroyMaterialy.Services;

namespace StroyMaterialy.Views;

public partial class OrdersView : UserControl
{
    public OrdersView()
    {
        InitializeComponent();
        AdminPanel.Visibility = AppSession.CanManageOrders ? Visibility.Visible : Visibility.Collapsed;
    }

    public async void RefreshAsync() => await LoadOrdersAsync();

    private async Task LoadOrdersAsync()
    {
        try
        {
            var orders = await OrderService.GetAllAsync();
            OrdersGrid.ItemsSource = new ObservableCollection<Order>(orders);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка");
        }
    }

    private Order? SelectedOrder => OrdersGrid.SelectedItem as Order;

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OrderEditWindow(new Order { OrderDate = DateTime.Today, DeliveryDate = DateTime.Today.AddDays(3) });
        if (dlg.ShowDialog() == true)
            await LoadOrdersAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedOrder == null)
        {
            MessageBox.Show("Выберите заказ.", "Внимание");
            return;
        }

        var dlg = new OrderEditWindow(SelectedOrder);
        if (dlg.ShowDialog() == true)
            await LoadOrdersAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedOrder == null)
        {
            MessageBox.Show("Выберите заказ.", "Внимание");
            return;
        }

        if (MessageBox.Show($"Удалить заказ №{SelectedOrder.OrderNumber}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            await OrderService.DeleteAsync(SelectedOrder.Id);
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось удалить: {ex.Message}", "Ошибка");
        }
    }

    private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
}
