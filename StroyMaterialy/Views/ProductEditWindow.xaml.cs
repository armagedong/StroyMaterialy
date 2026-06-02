using System.Globalization;
using System.Windows;
using StroyMaterialy.Models;
using StroyMaterialy.Services;

namespace StroyMaterialy.Views;

public partial class ProductEditWindow : Window
{
    private readonly Product _product;

    public ProductEditWindow(Product product)
    {
        InitializeComponent();
        _product = product;
        Title = product.Id == 0 ? "Товар — добавление" : "Товар — редактирование";

        ArticleBox.Text = product.Article;
        NameBox.Text = product.Name;
        UnitBox.Text = product.Unit;
        PriceBox.Text = product.Price.ToString(CultureInfo.InvariantCulture);
        SupplierBox.Text = product.Supplier;
        ManufacturerBox.Text = product.Manufacturer;
        CategoryBox.Text = product.Category;
        DiscountBox.Text = product.DiscountPercent.ToString();
        QuantityBox.Text = product.QuantityInStock.ToString();
        DescriptionBox.Text = product.Description;
        ImageBox.Text = product.ImageFile;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(PriceBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ||
            !int.TryParse(DiscountBox.Text, out var discount) ||
            !int.TryParse(QuantityBox.Text, out var qty))
        {
            MessageBox.Show("Проверьте числовые поля.", "Ошибка");
            return;
        }

        _product.Article = ArticleBox.Text.Trim();
        _product.Name = NameBox.Text.Trim();
        _product.Unit = UnitBox.Text.Trim();
        _product.Price = price;
        _product.Supplier = SupplierBox.Text.Trim();
        _product.Manufacturer = ManufacturerBox.Text.Trim();
        _product.Category = CategoryBox.Text.Trim();
        _product.DiscountPercent = discount;
        _product.QuantityInStock = qty;
        _product.Description = DescriptionBox.Text.Trim();
        _product.ImageFile = ImageBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(_product.Article) || string.IsNullOrWhiteSpace(_product.Name))
        {
            MessageBox.Show("Артикул и наименование обязательны.", "Ошибка");
            return;
        }

        try
        {
            await ProductService.SaveAsync(_product);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка");
        }
    }
}
