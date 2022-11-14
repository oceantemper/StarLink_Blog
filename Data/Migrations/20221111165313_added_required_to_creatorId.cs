using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarLink_Blog.Data.Migrations
{
    public partial class added_required_to_creatorId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_AspNetUsers_CreatorId",
                table: "BlogPosts");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "BlogPosts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_AspNetUsers_CreatorId",
                table: "BlogPosts",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_AspNetUsers_CreatorId",
                table: "BlogPosts");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "BlogPosts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_AspNetUsers_CreatorId",
                table: "BlogPosts",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
