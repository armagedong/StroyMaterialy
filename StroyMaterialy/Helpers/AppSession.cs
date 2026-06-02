using StroyMaterialy.Models;

namespace StroyMaterialy.Helpers;

public static class AppSession
{
    public static UserAccount? CurrentUser { get; set; }

    public static UserRoleType Role => CurrentUser?.RoleType ?? UserRoleType.Guest;

    public static bool IsGuest => Role == UserRoleType.Guest;
    public static bool IsClient => Role == UserRoleType.Client;
    public static bool CanFilterProducts => Role is UserRoleType.Manager or UserRoleType.Administrator;
    public static bool CanManageProducts => Role == UserRoleType.Administrator;
    public static bool CanViewOrders => Role is UserRoleType.Manager or UserRoleType.Administrator;
    public static bool CanManageOrders => Role == UserRoleType.Administrator;
}
