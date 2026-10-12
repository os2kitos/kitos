using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PubSub.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.CreateTable(
                    name: "Subscriptions",
                    columns: table => new
                    {
                        Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                        Callback = table.Column<string>(type: "text", nullable: false),
                        Topic = table.Column<string>(type: "text", nullable: false),
                        OwnerId = table.Column<string>(type: "text", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Subscriptions", x => x.Uuid);
                    });

                return;
            }

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(nullable: false),
                    Callback = table.Column<string>(nullable: false),
                    Topic = table.Column<string>(nullable: false),
                    OwnerId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Uuid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscriptions");
        }
    }
}
