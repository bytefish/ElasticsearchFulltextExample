using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ElasticsearchFulltextExample.Web.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(nullable: false),
                    filename = table.Column<string>(nullable: false),
                    data = table.Column<byte[]>(nullable: false),
                    keywords = table.Column<string>(nullable: false),
                    suggestions = table.Column<string>(nullable: false),
                    is_ocr_requested = table.Column<bool>(nullable: false),
                    uploaded_at = table.Column<DateTime>(nullable: false),
                    indexed_at = table.Column<DateTime>(nullable: true),
                    status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documents");
        }
    }
}
