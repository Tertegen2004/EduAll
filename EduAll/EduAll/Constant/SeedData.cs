using EduAll.Domain;
using Microsoft.AspNetCore.Identity;

namespace EduAll.Constant
{
    public class SeedData
    {
        public static async Task Seeding(IServiceProvider service)
        {
            var usermanager = service.GetRequiredService<UserManager<AppUser>>();
            var rolemanager = service.GetRequiredService<RoleManager<IdentityRole>>();


            // Add Admin Role
            if (!await rolemanager.RoleExistsAsync(Roles.Admin.ToString()))
            {
                await rolemanager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            }

            // Add Student Role
            if (!await rolemanager.RoleExistsAsync(Roles.Student.ToString()))
            {
                await rolemanager.CreateAsync(new IdentityRole(Roles.Student.ToString()));
            }

            // Add Instructor Role
            if (!await rolemanager.RoleExistsAsync(Roles.Instructor.ToString()))
            {
                await rolemanager.CreateAsync(new IdentityRole(Roles.Instructor.ToString()));
            }

            // Create Admin

            var admin = new AppUser
            {
                FirstName ="Admin",
                LastName ="Admin",
                Country ="Global",
                Email = "admin@admin.com",
                UserName = "admin@admin.com"
            };

            // Check If User Is Found
            var exist = await usermanager.FindByEmailAsync(admin.Email);

            // Create User If Not Found
            if (exist == null)
            {
                var createResult = await usermanager.CreateAsync(admin, "admin@123");

                // Add Role Admin To User
                if (createResult.Succeeded)
                {
                    await usermanager.AddToRoleAsync(admin, Roles.Admin.ToString());
                }
            }


            // Create Student

            //var student = new AppUser
            //{
            //    FirstName = "Ahmed",
            //    LastName = "Gamal",
            //    Country = "Egypt",
            //    Email = "Student123@gmail.com",
            //    UserName = "Student123@gmail.com"
            //};

            //// Check If User Is Found
            //var existstudent = await usermanager.FindByEmailAsync(student.Email);

            //// Create User If Not Found
            //if (existstudent == null)
            //{
            //    var createResult = await usermanager.CreateAsync(student, "student@123");

            //    // Add Role Student To User
            //    if (createResult.Succeeded)
            //    {
            //        await usermanager.AddToRoleAsync(student, Roles.Student.ToString());
            //    }
            //}

            // Create Instructor

            //var instructor = new AppUser
            //{
            //    FirstName = "Mr_Abdo",
            //    LastName = "Safah",
            //    Country = "Egypt",
            //    Email = "inst123@gmail.com",
            //    UserName = "inst123@gmail.com"
            //};

            //// Check If User Is Found
            //var existinst = await usermanager.FindByEmailAsync(instructor.Email);

            //// Create User If Not Found
            //if (existinst == null)
            //{
            //    var createResult = await usermanager.CreateAsync(instructor, "student@123");

            //    // Add Role Student To User
            //    if (createResult.Succeeded)
            //    {
            //        await usermanager.AddToRoleAsync(instructor, Roles.Instructor.ToString());
            //    }
            //}
        }
    }
}
