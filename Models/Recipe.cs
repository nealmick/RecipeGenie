using System;
using Microsoft.AspNetCore.Identity;

namespace RecipeGenerator.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public string? Prompt { get; set; }
        public string? UserId { get; set; }  // Foreign key property

        public ApplicationUser? User { get; set; }
    }
}
