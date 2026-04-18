using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerPro.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }
        public int UserId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        public string PaymentMode { get; set; }

        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}