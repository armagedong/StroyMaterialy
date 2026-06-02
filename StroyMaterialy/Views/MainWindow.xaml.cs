using System.Windows;
using StroyMaterialy.Helpers;
using StroyMaterialy.Models;

namespace StroyMaterialy.Views;

public partial class MainWindow : Window
{
    private readonly bool _guestMode;
    private ProductsView? _productsView;
    private OrdersView? _ordersView;

    public MainWindow(bool showOnlyProducts)
    {
        InitializeComponent();
        _guestMode = showOnlyProducts;

        if (_guestMode)
        {
            Title = "ООО «СтройМатериалы» — Товары (гость)";
            UserInfoText.Text = "Роль: Гость";
            OrdersNavButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            var user = AppSession.CurrentUser!;
            UserInfoText.Text = $"{user.FullName} | Роль: {user.RoleName}";
            OrdersNavButton.Visibility = AppSession.CanViewOrders
                ? Visibility.Visible
                : Visibility.Collapsed;
            Title = $"ООО «СтройМатериалы» — {user.RoleName}";
        }

        ShowProducts();
    }

    private void ShowProducts()
    {
        _productsView ??= new ProductsView();
        ContentHost.Content = _productsView;
        _productsView.RefreshAsync();
    }

    private void ShowOrders()
    {
        if (!AppSession.CanViewOrders) return;
        _ordersView ??= new OrdersView();
        ContentHost.Content = _ordersView;
        _ordersView.RefreshAsync();
    }

    private void ProductsNav_Click(object sender, RoutedEventArgs e) => ShowProducts();

    private void OrdersNav_Click(object sender, RoutedEventArgs e) => ShowOrders();

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        AppSession.CurrentUser = null;
        var login = new LoginWindow();
        login.Show();
        Close();
    }
}
