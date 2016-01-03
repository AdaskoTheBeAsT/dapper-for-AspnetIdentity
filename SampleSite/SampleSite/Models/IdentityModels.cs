using Lu.AspnetIdentity.Dapper;
using Microsoft.AspNet.Identity;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentitySample.Models
{
    using MySql.Data.MySqlClient;

    [Lu.Dapper.Extensions.DataAnnotations.Table("IdentityUsers")]
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    [Lu.Dapper.Extensions.DataAnnotations.Table("IdentityRoles")]
    public class ApplicationRole : IdentityRole
    {
    }

    [Lu.Dapper.Extensions.DataAnnotations.Table("IdentityUserRoles")]
    public class ApplicationUserRole : IdentityUserRole<string, string, string>
    {
    }

    [Lu.Dapper.Extensions.DataAnnotations.Table("IdentityUserLogins")]
    public class ApplicationUserLogin : IdentityUserLogin<string, string>
    {
    }

    [Lu.Dapper.Extensions.DataAnnotations.Table("IdentityUserClaims")]
    public class ApplicationUserClaim : IdentityUserClaim<string, string>
    {
    }

    public class ApplicationDbContext : DapperIdentityDbContext<ApplicationUser, ApplicationRole, ApplicationUserRole, ApplicationUserLogin, ApplicationUserClaim>
    {
        public ApplicationDbContext(IDbConnection connection)
            : base(connection)
        {
        }

        public static ApplicationDbContext Create()
        {
            var connString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultIdentityConnection"].ConnectionString;
            var conn = new MySqlConnection(connString);
            return new ApplicationDbContext(conn);
        }
    }
}