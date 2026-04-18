using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ExpenseTrackerPro.Models
{
    public class ExpenseFilterViewModel
    {
        public int? CategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<Expense> Expenses { get; set; }
        public SelectList Categories { get; set; }
    }
}