using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Chatting.Server.Data;

#nullable disable

namespace Chatting.Server.Migrations;

[DbContext(typeof(ChatDbContext))]
[Migration("20261017000000_AddConversationMemberNickname")]
public partial class AddConversationMemberNickname : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Nickname",
            table: "ConversationMembers",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Nickname", table: "ConversationMembers");
    }
}
