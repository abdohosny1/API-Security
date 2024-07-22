using API_Security.enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API_Security.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
           
            builder.HasData(
                new IdentityRole()
                {
                    Name= RolesModel.Team.ToString(),
                    NormalizedName="TEAM"
                },
                new IdentityRole()
                {
                    Name = RolesModel.Admin.ToString(),
                    NormalizedName = "ADMIN"
                }
             );
        }
    }
}
