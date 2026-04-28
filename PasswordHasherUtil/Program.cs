using Microsoft.AspNetCore.Identity;

var passwordHasher = new PasswordHasher<IdentityUser>();
var testUser = new IdentityUser();
var hashedPassword = passwordHasher.HashPassword(testUser, "password123");
Console.WriteLine(hashedPassword);
