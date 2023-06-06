using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRISPR.Data.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSet",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tilte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubTilte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepositoryURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Licenses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSet", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Code",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tilte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubTilte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepositoryURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Licenses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false),
                    DataSetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Code", x => x.id);
                    table.ForeignKey(
                        name: "FK_Code_DataSet_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSet",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataSetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.id);
                    table.ForeignKey(
                        name: "FK_Comment_DataSet_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSet",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Prop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prop", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prop_DataSet_ModelId",
                        column: x => x.ModelId,
                        principalTable: "DataSet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataSetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tag_DataSet_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Code_DataSetId",
                table: "Code",
                column: "DataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_DataSetId",
                table: "Comment",
                column: "DataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Prop_ModelId",
                table: "Prop",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_DataSetId",
                table: "Tag",
                column: "DataSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Code");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "Prop");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "DataSet");
        }
    }
}
