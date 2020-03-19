﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MVCAttendanceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MVCAttendanceSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        ApplicationDbContext Context;
        UserManager<ApplicationUser> userManager;
        RoleManager<IdentityRole> roleManger;
        public AdminController()
        {
            Context = new ApplicationDbContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            roleManger = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
        }
        // GET: Admin
        public ActionResult Index()
        {
            var users = userManager.Users.Include(a => a.department).ToList();
            return View(users);
        }

        // GET: Admin/Details/5
        public ActionResult Details(string id)
        {
            ApplicationUser UserDetails = userManager.Users.Include(a => a.department).FirstOrDefault(s => s.Id == id);
            if (UserDetails == null)
            {
                return RedirectToAction("Index");
            }
            return View(UserDetails);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            var DepartmentId = Context.departments.ToList();
            ViewBag.DepartmentId = new SelectList(DepartmentId, "DepartmentId", "DepartmentName");
            var Roles = roleManger.Roles.ToList();
            ViewBag.Roles = new SelectList(Roles, "Name", "Name");
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel model, int DepartmentId, string Roles)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, DepartmentId = DepartmentId };
                await userManager.CreateAsync(user, model.Password);
                await userManager.AddToRoleAsync(user.Id, Roles);
                return RedirectToAction("Index");
            }

            return View("Create");

        }

        // GET: Admin/Edit/5
        public ActionResult Edit(string id)
        {
            ApplicationUser EditedUser = userManager.Users.Include(a => a.department).FirstOrDefault(s => s.Id == id);
            if (EditedUser == null)
            {
                return HttpNotFound();
            }
            return View(EditedUser);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUser user)
        {
            ApplicationUser EditedUser = userManager.FindById(user.Id);
            EditedUser.UserName = user.UserName;
            EditedUser.Email = user.Email;
            EditedUser.PhoneNumber = user.PhoneNumber;
            userManager.Update(EditedUser);
            return RedirectToAction("Index");
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser DeletedUser = userManager.Users.FirstOrDefault(s => s.Id == id);
            if (DeletedUser == null)
            {
                return HttpNotFound();
            }
            return View(DeletedUser);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ApplicationUser DeletedUser = userManager.Users.FirstOrDefault(s => s.Id == id);
            userManager.Delete(DeletedUser);
            return RedirectToAction("Index");
        }
        
        public ActionResult PreviewPermission()
        {
            var permissions = Context.permissions.Include(a => a.applicationUser).ToList();
            return View(permissions);
        }
        [HttpPost]
        public ActionResult PreviewPermission(List<Permission> permissions)
        {
            foreach(var item in permissions)
            {
                var StudentPermission = Context.permissions.Find(item.PermissionId);
                
                if (item.IsChecked)
                {
                    item.Status = "Accept";
                }
                else
                {
                    item.Status = "Refused";
                }

                 StudentPermission.Status = item.Status;
                 StudentPermission.IsChecked = item.IsChecked;

            }

            Context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
    }
}
