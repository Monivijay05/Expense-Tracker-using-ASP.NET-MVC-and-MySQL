using ExpenseTrackerPro.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace ExpenseTrackerPro.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["dbcs"].ConnectionString;

        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            DashboardViewModel vm = new DashboardViewModel();
            vm.RecentExpenses = new List<Expense>();

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                con.Open();

                string todayQuery = "SELECT IFNULL(SUM(Amount),0) FROM Expenses WHERE UserId=@UserId AND ExpenseDate=CURDATE()";
                MySqlCommand todayCmd = new MySqlCommand(todayQuery, con);
                todayCmd.Parameters.AddWithValue("@UserId", userId);
                vm.TodayExpense = Convert.ToDecimal(todayCmd.ExecuteScalar());

                string monthQuery = @"SELECT IFNULL(SUM(Amount),0) FROM Expenses
                                      WHERE UserId=@UserId
                                      AND MONTH(ExpenseDate)=MONTH(CURDATE())
                                      AND YEAR(ExpenseDate)=YEAR(CURDATE())";
                MySqlCommand monthCmd = new MySqlCommand(monthQuery, con);
                monthCmd.Parameters.AddWithValue("@UserId", userId);
                vm.MonthlyExpense = Convert.ToDecimal(monthCmd.ExecuteScalar());

                string totalQuery = "SELECT IFNULL(SUM(Amount),0) FROM Expenses WHERE UserId=@UserId";
                MySqlCommand totalCmd = new MySqlCommand(totalQuery, con);
                totalCmd.Parameters.AddWithValue("@UserId", userId);
                vm.TotalExpense = Convert.ToDecimal(totalCmd.ExecuteScalar());

                string catQuery = "SELECT COUNT(*) FROM Categories WHERE UserId IS NULL OR UserId=@UserId";
                MySqlCommand catCmd = new MySqlCommand(catQuery, con);
                catCmd.Parameters.AddWithValue("@UserId", userId);
                vm.TotalCategories = Convert.ToInt32(catCmd.ExecuteScalar());

                string recentQuery = @"SELECT e.ExpenseId, e.Amount, e.ExpenseDate, e.Description, e.PaymentMode, c.CategoryName
                                       FROM Expenses e
                                       INNER JOIN Categories c ON e.CategoryId = c.CategoryId
                                       WHERE e.UserId=@UserId
                                       ORDER BY e.ExpenseDate DESC, e.ExpenseId DESC
                                       LIMIT 5";
                MySqlCommand recentCmd = new MySqlCommand(recentQuery, con);
                recentCmd.Parameters.AddWithValue("@UserId", userId);

                using (MySqlDataReader dr = recentCmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        vm.RecentExpenses.Add(new Expense
                        {
                            ExpenseId = Convert.ToInt32(dr["ExpenseId"]),
                            Amount = Convert.ToDecimal(dr["Amount"]),
                            ExpenseDate = Convert.ToDateTime(dr["ExpenseDate"]),
                            Description = dr["Description"].ToString(),
                            PaymentMode = dr["PaymentMode"].ToString(),
                            CategoryName = dr["CategoryName"].ToString()
                        });
                    }
                }
            }

            return View(vm);
        }
    }
}