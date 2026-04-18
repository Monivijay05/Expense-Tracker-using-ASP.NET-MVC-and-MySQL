using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerPro.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int? UserId { get; set; }

        [Required]
        public string CategoryName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}   