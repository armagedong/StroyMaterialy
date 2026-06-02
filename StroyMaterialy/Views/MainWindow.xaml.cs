using System.Windows;
using Microsoft.Win32;
using StroyMaterialy.Helpers;
using StroyMaterialy.Models;
using StroyMaterialy.Services;

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

            if (AppSession.Role == UserRoleType.Administrator)
            {
                ExportExcelButton.Visibility = Visibility.Visible;
                ImportExcelButton.Visibility = Visibility.Visible;
            }
        }

        ShowProducts();
    }

    private async void ExportExcel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Экспорт всех таблиц в Excel",
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = $"StroyMaterialy_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            DefaultExt = ".xlsx"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            ExportExcelButton.IsEnabled = false;
            ImportExcelButton.IsEnabled = false;
            await ExcelExchangeService.ExportAllAsync(dialog.FileName);
            MessageBox.Show($"Данные экспортированы в файл:\n{dialog.FileName}", "Экспорт",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ExportExcelButton.IsEnabled = true;
            ImportExcelButton.IsEnabled = true;
        }
    }

    private async void ImportExcel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Импорт всех таблиц из Excel",
            Filter = "Excel (*.xlsx)|*.xlsx"
        };

        if (dialog.ShowDialog() != true) return;

        if (MessageBox.Show(
                "Импорт заменит все данные в таблицах (кроме ролей — они дополняются).\nПродолжить?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            ExportExcelButton.IsEnabled = false;
            ImportExcelButton.IsEnabled = false;
            await ExcelExchangeService.ImportAllAsync(dialog.FileName);
            MessageBox.Show("Импорт успешно завершён.", "Импорт",
                MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshCurrentView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ExportExcelButton.IsEnabled = true;
            ImportExcelButton.IsEnabled = true;
        }
    }

    private void RefreshCurrentView()
    {
        if (ContentHost.Content is ProductsView pv)
            pv.RefreshAsync();
        else if (ContentHost.Content is OrdersView ov)
            ov.RefreshAsync();
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
