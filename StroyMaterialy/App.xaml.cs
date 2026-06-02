using System.Windows;
using StroyMaterialy.Helpers;
using StroyMaterialy.Services;
using StroyMaterialy.Views;

namespace StroyMaterialy;

public partial class App : Application
{
    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        AppConfig.Initialize();

        try
        {
            await DatabaseService.EnsureSchemaAsync();

            if (!await DatabaseService.HasDataAsync())
            {
                var result = MessageBox.Show(
                    "База данных пуста. Выполнить импорт данных из папки «импорт»?",
                    "Импорт данных",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    await ImportService.ImportAllAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось подключиться к PostgreSQL.\n\n{ex.Message}\n\n" +
                "Проверьте appsettings.json и выполните скрипт database/init.sql.",
                "Ошибка БД",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        var login = new LoginWindow();
        login.Show();
    }
}
