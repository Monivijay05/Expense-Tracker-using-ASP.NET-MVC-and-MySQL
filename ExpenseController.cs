using ExpenseTrackerPro.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace ExpenseTrackerPro.Controllers
{
    public class ExpenseController : BaseController
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["dbcs"].ConnectionString;

        private List<Category> GetCategories()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            List<Category> list = new List<Category>();

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                string query = @"SELECT CategoryId, CategoryName
                                 FROM Categories
                                 WHERE UserId IS NULL OR UserId=@UserId
                                 ORDER BY CategoryName";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new Category
                        {
                            CategoryId = Convert.ToInt32(dr["CategoryId"]),
                            CategoryName = dr["CategoryName"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public ActionResult Index(int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            ExpenseFilterViewModel vm = new ExpenseFilterViewModel();
            vm.Expenses = new List<Expense>();

            string query = @"SELECT e.ExpenseId, e.CategoryId, e.Amount, e.ExpenseDate, e.Description, e.PaymentMode, c.CategoryName
                             FROM Expenses e
                             INNER JOIN Categories c ON e.CategoryId = c.CategoryId
                             WHERE e.UserId=@UserId";

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                if (categoryId.HasValue && categoryId.Value > 0)
                    query += " AND e.CategoryId=@CategoryId";

                if (fromDate.HasValue)
                    query += " AND e.ExpenseDate >= @FromDate";

                if (toDate.HasValue)
                    query += " AND e.ExpenseDate <= @ToDate";

                query += " ORDER BY e.ExpenseDate DESC, e.ExpenseId DESC";

                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                if (categoryId.HasValue && categoryId.Value > 0)
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);

                if (fromDate.HasValue)
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Value.Date);

                if (toDate.HasValue)
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Value.Date);

                con.Open();
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        vm.Expenses.Add(new Expense
                        {
                            ExpenseId = Convert.ToInt32(dr["ExpenseId"]),
                            CategoryId = Convert.ToInt32(dr["CategoryId"]),
                            Amount = Convert.ToDecimal(dr["Amount"]),
                            ExpenseDate = Convert.ToDateTime(dr["ExpenseDate"]),
                            Description = dr["Description"].ToString(),
                            PaymentMode = dr["PaymentMode"].ToString(),
                            CategoryName = dr["CategoryName"].ToString()
                        });
                    }
                }
            }

            vm.CategoryId = categoryId;
            vm.FromDate = fromDate;
            vm.ToDate = toDate;
            vm.Categories = new SelectList(GetCategories(), "CategoryId", "CategoryName");

            return View(vm);
        }

        public ActionResult Create()
        {
            ViewBag.Categories = new SelectList(GetCategories(), "CategoryId", "CategoryName");
            ViewBag.PaymentModes = new SelectList(new List<string> { "Cash", "UPI", "Card", "Bank" });
            return View();
        }

        [HttpPost]
        public ActionResult Create(Expense model)
        {
            ViewBag.Categories = new SelectList(GetCategories(), "CategoryId", "CategoryName", model.CategoryId);
            ViewBag.PaymentModes = new SelectList(new List<string> { "Cash", "UPI", "Card", "Bank" }, model.PaymentMode);

            if (!ModelState.IsValid)
                return View(model);

            int userId = Convert.ToInt32(Session["UserId"]);

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                string query = @"INSERT INTO Expenses(UserId, CategoryId, Amount, ExpenseDate, Description, PaymentMode)
                                 VALUES(@UserId,@CategoryId,@Amount,@ExpenseDate,@Description,@PaymentMode)";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);
                cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate.Date);
                cmd.Parameters.AddWithValue("@Description", model.Description ?? "");
                cmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Expense added successfully";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            Expense model = null;

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                string query = @"SELECT * FROM Expenses
                                 WHERE ExpenseId=@ExpenseId AND UserId=@UserId";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ExpenseId", id);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        model = new Expense
                        {
                            ExpenseId = Convert.ToInt32(dr["ExpenseId"]),
                            CategoryId = Convert.ToInt32(dr["CategoryId"]),
                            Amount = Convert.ToDecimal(dr["Amount"]),
                            ExpenseDate = Convert.ToDateTime(dr["ExpenseDate"]),
                            Description = dr["Description"].ToString(),
                            PaymentMode = dr["PaymentMode"].ToString()
                        };
                    }
                }
            }

            if (model == null)
                return RedirectToAction("Index");

            ViewBag.Categories = new SelectList(GetCategories(), "CategoryId", "CategoryName", model.CategoryId);
            ViewBag.PaymentModes = new SelectList(new List<string> { "Cash", "UPI", "Card", "Bank" }, model.PaymentMode);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Expense model)
        {
            ViewBag.Categories = new SelectList(GetCategories(), "CategoryId", "CategoryName", model.CategoryId);
            ViewBag.PaymentModes = new SelectList(new List<string> { "Cash", "UPI", "Card", "Bank" }, model.PaymentMode);

            if (!ModelState.IsValid)
                return View(model);

            int userId = Convert.ToInt32(Session["UserId"]);

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                string query = @"UPDATE Expenses
                                 SET CategoryId=@CategoryId,
                                     Amount=@Amount,
                                     ExpenseDate=@ExpenseDate,
                                     Description=@Description,
                                     PaymentMode=@PaymentMode,
                                     UpdatedAt=NOW()
                                 WHERE ExpenseId=@ExpenseId AND UserId=@UserId";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);
                cmd.Parameters.AddWithValue("@ExpenseDate", model.ExpenseDate.Date);
                cmd.Parameters.AddWithValue("@Description", model.Description ?? "");
                cmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
                cmd.Parameters.AddWithValue("@ExpenseId", model.ExpenseId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Expense updated successfully";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                string query = "DELETE FROM Expenses WHERE ExpenseId=@ExpenseId AND UserId=@UserId";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ExpenseId", id);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Expense deleted successfully";
            return RedirectToAction("Index");
        }

        public ActionResult MonthlyReport()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            List<MonthlyReportViewModel> list = new List<MonthlyReportViewModel>();

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                string query = @"SELECT c.CategoryName, IFNULL(SUM(e.Amount),0) AS TotalAmount
                                 FROM Expenses e
                                 INNER JOIN Categories c ON e.CategoryId = c.CategoryId
                                 WHERE e.UserId=@UserId
                                 AND MONTH(e.ExpenseDate)=MONTH(CURDATE())
                                 AND YEAR(e.ExpenseDate)=YEAR(CURDATE())
                                 GROUP BY c.CategoryName
                                 ORDER BY TotalAmount DESC";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new MonthlyReportViewModel
                        {
                            CategoryName = dr["CategoryName"].ToString(),
                            TotalAmount = Convert.ToDecimal(dr["TotalAmount"])
                        });
                    }
                }
            }

            return View(list);
        }
    }
}