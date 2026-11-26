using Chatting.Server.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatting.Server.Migrations;

[DbContext(typeof(ChatDbContext))]
[Migration("20261026000000_AddGroupMemberPermissionAndUserBio")]
public partial class AddGroupMemberPermissionAndUserBio : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "AllowMembersToAdd",
            table: "ChatGroups",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "Bio",
            table: "Users",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "AllowMembersToAdd", table: "ChatGroups");
        migrationBuilder.DropColumn(name: "Bio", table: "Users");
    }
}
