using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRISPR.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prop_DataSet_ModelId",
                table: "Prop");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_DataSet_DataSetId",
                table: "Tag");

            migrationBuilder.AlterColumn<int>(
                name: "DataSetId",
                table: "Tag",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ModelId",
                table: "Prop",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Prop_DataSet_ModelId",
                table: "Prop",
                column: "ModelId",
                principalTable: "DataSet",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_DataSet_DataSetId",
                table: "Tag",
                column: "DataSetId",
                principalTable: "DataSet",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prop_DataSet_ModelId",
                table: "Prop");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_DataSet_DataSetId",
                table: "Tag");

            migrationBuilder.AlterColumn<int>(
                name: "DataSetId",
                table: "Tag",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ModelId",
                table: "Prop",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Prop_DataSet_ModelId",
                table: "Prop",
                column: "ModelId",
                principalTable: "DataSet",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_DataSet_DataSetId",
                table: "Tag",
                column: "DataSetId",
                principalTable: "DataSet",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
