﻿using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using StoreFront.Data;

namespace Week_3_Intern_Assignment.Controllers
{
    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration POST Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(User_table user)
        {
            bool status = false;
            string message = "";

            //Model validation
            if(ModelState.IsValid)  
            {
                var isExist = IsUserNameExist(user.UserName); 
                if (isExist)//if user name exists already
                {
                    ModelState.AddModelError("UserNameExist", "User name already exists.");
                    return View(user);
                }

                SqlSecurityManager.RegisterUser(user);
                message = "Registration successful!";
                status = true;
            }
            else
            {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(user);

            
        }

        //Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(SqlSecurityManager login, string ReturnUrl="")
        {
            string message = "";
            using (StoreFrontEntities dc = new StoreFrontEntities())
            {
                var v = dc.User_table.Where(a => a.UserName == login.UserName).FirstOrDefault();
                if(v != null)
                {
                    if (SqlSecurityManager.AuthenticateUser(login.UserName, login.Password))
                    {
                        int timeout = login.RememberMe ? 525600 : 1;  //525600 min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.UserName, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if(Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("HomePage", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid credential provided.";
                    }
                }
                else
                {
                    message = "Invalid credential provided.";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        public bool IsUserNameExist(string userName)
        {
            using (StoreFrontEntities dc = new StoreFrontEntities())
            {
                var v = dc.User_table.Where(a => a.UserName == userName).FirstOrDefault();
                return v != null;
            }
        }
    }
}