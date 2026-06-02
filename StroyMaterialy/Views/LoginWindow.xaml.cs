using System.Windows;
using StroyMaterialy.Helpers;
using StroyMaterialy.Models;
using StroyMaterialy.Services;

namespace StroyMaterialy.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        var login = LoginTextBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Введите логин и пароль.");
            return;
        }

        try
        {
            var user = await AuthService.AuthenticateAsync(login, password);
            if (user == null)
            {
                ShowError("Неверный логин или пароль.");
                return;
            }

            AppSession.CurrentUser = user;
            OpenMain();
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка подключения к БД: {ex.Message}");
        }
    }

    private void Guest_Click(object sender, RoutedEventArgs e)
    {
        AppSession.CurrentUser = null;
        var main = new MainWindow(showOnlyProducts: true);
        main.Show();
        Close();
    }

    private void OpenMain()
    {
        var main = new MainWindow(showOnlyProducts: false);
        main.Show();
        Close();
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
