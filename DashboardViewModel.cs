using System.Collections.Generic;

namespace ExpenseTrackerPro.Models
{
    public class DashboardViewModel
    {
        public decimal TodayExpense { get; set; }
        public decimal MonthlyExpense { get; set; }
        public decimal TotalExpense { get; set; }
        public int TotalCategories { get; set; }
        public List<Expense> RecentExpenses { get; set; }
    }
}