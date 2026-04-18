using ExpenseTrackerPro.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace ExpenseTrackerPro.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["dbcs"].ConnectionString;

        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            List<Category> list = new List<Category>();

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                string query = @"SELECT * FROM Categories
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
                            UserId = dr["UserId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["UserId"]),
                            CategoryName = dr["CategoryName"].ToString(),
                            CreatedAt = Convert.ToDateTime(dr["CreatedAt"])
                        });
                    }
                }
            }

            return View(list);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int userId = Convert.ToInt32(Session["UserId"]);

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                con.Open();

                string query = "INSERT INTO Categories(UserId, CategoryName) VALUES(@UserId, @CategoryName)";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CategoryName", model.CategoryName);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            Category model = null;

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                string query = @"SELECT * FROM Categories
                                 WHERE CategoryId=@CategoryId
                                 AND UserId=@UserId";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CategoryId", id);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        model = new Category
                        {
                            CategoryId = Convert.ToInt32(dr["CategoryId"]),
                            UserId = Convert.ToInt32(dr["UserId"]),
                            CategoryName = dr["CategoryName"].ToString()
                        };
                    }
                }
            }

            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int userId = Convert.ToInt32(Session["UserId"]);

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                string query = @"UPDATE Categories
                                 SET CategoryName=@CategoryName
                                 WHERE CategoryId=@CategoryId AND UserId=@UserId";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CategoryName", model.CategoryName);
                cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                con.Open();

                string checkExpenseQuery = "SELECT COUNT(*) FROM Expenses WHERE CategoryId=@CategoryId AND UserId=@UserId";
                MySqlCommand checkCmd = new MySqlCommand(checkExpenseQuery, con);
                checkCmd.Parameters.AddWithValue("@CategoryId", id);
                checkCmd.Parameters.AddWithValue("@UserId", userId);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    TempData["Error"] = "Cannot delete category. Expenses exist for this category.";
                    return RedirectToAction("Index");
                }

                string deleteQuery = "DELETE FROM Categories WHERE CategoryId=@CategoryId AND UserId=@UserId";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, con);
                deleteCmd.Parameters.AddWithValue("@CategoryId", id);
                deleteCmd.Parameters.AddWithValue("@UserId", userId);
                deleteCmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}