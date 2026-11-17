using Microsoft.AspNetCore.Identity;

namespace JobTracker.Models;

// IdentityUser: Id, Email, UserName, PasswordHash gibi alanları hazır getirir.
// Boş bırakıyorum ama ileride buraya DisplayName gibi alanlar eklenecek
public class AppUser : IdentityUser
{
}
