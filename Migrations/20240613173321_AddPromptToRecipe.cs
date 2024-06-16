using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                table: "Recipes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prompt",
                table: "Recipes");
        }
    }
}
