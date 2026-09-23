namespace Turnero.Domain.Constants;

public static class ClaimsStore
{
    public static readonly List<Claim> AllClaims =
    [
        new Claim("Create Role", "Create Role"),
        new Claim("Edit Role", "Edit Role"),
        new Claim("Delete Role", "Delete Role"),
    ];
}
