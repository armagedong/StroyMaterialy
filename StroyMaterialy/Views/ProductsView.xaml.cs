using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using StroyMaterialy.Helpers;
using StroyMaterialy.Models;
using StroyMaterialy.Services;

namespace StroyMaterialy.Views;

public partial class ProductsView : UserControl
{
    private List<Product> _allProducts = [];

    public ProductsView()
    {
        InitializeComponent();
        FilterPanel.Visibility = AppSession.CanFilterProducts ? Visibility.Visible : Visibility.Collapsed;
        AdminPanel.Visibility = AppSession.CanManageProducts ? Visibility.Visible : Visibility.Collapsed;

        if (AppSession.CanFilterProducts)
        {
            SortCombo.ItemsSource = new[]
            {
                "Наименование (А-Я)",
                "Наименование (Я-А)",
                "Цена (возр.)",
                "Цена (убыв.)",
                "Скидка (возр.)",
                "Скидка (убыв.)"
            };
            SortCombo.SelectedIndex = 0;
        }
    }

    public async void RefreshAsync() => await LoadProductsAsync();

    private async Task LoadProductsAsync()
    {
        try
        {
            _allProducts = await ProductService.GetAllAsync();
            if (AppSession.CanFilterProducts)
                PopulateFilters();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PopulateFilters()
    {
        var categories = _allProducts.Select(p => p.Category).Distinct().OrderBy(x => x).ToList();
        categories.Insert(0, "Все");
        CategoryFilter.ItemsSource = categories;
        CategoryFilter.SelectedIndex = 0;

        var suppliers = _allProducts.Select(p => p.Supplier).Distinct().OrderBy(x => x).ToList();
        suppliers.Insert(0, "Все");
        SupplierFilter.ItemsSource = suppliers;
        SupplierFilter.SelectedIndex = 0;
    }

    private void ApplyFilters()
    {
        IEnumerable<Product> query = _allProducts;

        if (AppSession.CanFilterProducts)
        {
            var search = SearchBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Article.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (CategoryFilter.SelectedItem is string cat && cat != "Все")
                query = query.Where(p => p.Category == cat);

            if (SupplierFilter.SelectedItem is string sup && sup != "Все")
                query = query.Where(p => p.Supplier == sup);

            query = SortCombo.SelectedIndex switch
            {
                1 => query.OrderByDescending(p => p.Name),
                2 => query.OrderBy(p => p.Price),
                3 => query.OrderByDescending(p => p.Price),
                4 => query.OrderBy(p => p.DiscountPercent),
                5 => query.OrderByDescending(p => p.DiscountPercent),
                _ => query.OrderBy(p => p.Name)
            };
        }

        ProductsGrid.ItemsSource = new ObservableCollection<Product>(query.ToList());
    }

    private void Filter_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        ApplyFilters();
    }

    private void ResetFilters_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = string.Empty;
        if (CategoryFilter.Items.Count > 0) CategoryFilter.SelectedIndex = 0;
        if (SupplierFilter.Items.Count > 0) SupplierFilter.SelectedIndex = 0;
        if (SortCombo.Items.Count > 0) SortCombo.SelectedIndex = 0;
        ApplyFilters();
    }

    private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private Product? SelectedProduct => ProductsGrid.SelectedItem as Product;

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ProductEditWindow(new Product());
        if (dlg.ShowDialog() == true)
            await LoadProductsAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Выберите товар.", "Внимание");
            return;
        }

        var copy = new Product
        {
            Id = SelectedProduct.Id,
            Article = SelectedProduct.Article,
            Name = SelectedProduct.Name,
            Unit = SelectedProduct.Unit,
            Price = SelectedProduct.Price,
            Supplier = SelectedProduct.Supplier,
            Manufacturer = SelectedProduct.Manufacturer,
            Category = SelectedProduct.Category,
            DiscountPercent = SelectedProduct.DiscountPercent,
            QuantityInStock = SelectedProduct.QuantityInStock,
            Description = SelectedProduct.Description,
            ImageFile = SelectedProduct.ImageFile
        };

        var dlg = new ProductEditWindow(copy);
        if (dlg.ShowDialog() == true)
            await LoadProductsAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Выберите товар.", "Внимание");
            return;
        }

        if (MessageBox.Show($"Удалить товар «{SelectedProduct.Name}»?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            await ProductService.DeleteAsync(SelectedProduct.Id);
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось удалить: {ex.Message}", "Ошибка");
        }
    }
}
