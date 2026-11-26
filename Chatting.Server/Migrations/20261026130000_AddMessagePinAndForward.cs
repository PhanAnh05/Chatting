using System;
using Chatting.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chatting.Server.Migrations;

[DbContext(typeof(ChatDbContext))]
[Migration("20261026130000_AddMessagePinAndForward")]
public partial class AddMessagePinAndForward : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ForwardedFromUsername",
            table: "Messages",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsPinned",
            table: "Messages",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "PinnedAt",
            table: "Messages",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "PinnedByUserId",
            table: "Messages",
            type: "uniqueidentifier",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ForwardedFromUsername", table: "Messages");
        migrationBuilder.DropColumn(name: "IsPinned", table: "Messages");
        migrationBuilder.DropColumn(name: "PinnedAt", table: "Messages");
        migrationBuilder.DropColumn(name: "PinnedByUserId", table: "Messages");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        ChatDbContextModelSnapshot.BuildCurrentModel(modelBuilder);
}
