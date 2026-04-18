using ExpenseTrackerPro.Helpers;
using ExpenseTrackerPro.Models;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace ExpenseTrackerPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["dbcs"].ConnectionString;

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@Email", model.Email);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    ViewBag.Message = "Email already exists";
                    return View(model);
                }

                string query = "INSERT INTO Users(Name, Email, PasswordHash) VALUES(@Name,@Email,@PasswordHash)";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(model.Password));
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Registration successful. Please login.";
            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            if (Session["UserId"] != null)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (MySqlConnection con = new MySqlConnection(cs))
            {
                con.Open();

                string query = "SELECT UserId, Name FROM Users WHERE Email=@Email AND PasswordHash=@PasswordHash";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(model.Password));

                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    Session["UserId"] = dr["UserId"].ToString();
                    Session["UserName"] = dr["Name"].ToString();
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            ViewBag.Message = "Invalid email or password";
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}