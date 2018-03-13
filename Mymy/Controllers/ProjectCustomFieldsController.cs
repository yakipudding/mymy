using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Mymy.DAL;
using Mymy.Models;

namespace Mymy.Controllers
{
    public class ProjectCustomFieldsController : Controller
    {
        private MymyContext db = new MymyContext();                

        // GET: ProjectCustomFields/Create
        public ActionResult Create(int projectid)
        {
            var project = db.Projects.FirstOrDefault(x => x.ProjectId == projectid);
            var projectCustomField = new ProjectCustomField();
            projectCustomField.Project = project;
            return View(projectCustomField);
        }

        // POST: ProjectCustomFields/Create
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int projectId, string Field, string FieldJapaneseName, bool Visible)
        {
            var projectCustomField = new ProjectCustomField();

            if (ModelState.IsValid)
            {
                var project = db.Projects.FirstOrDefault(x => x.ProjectId == projectId);
                projectCustomField.Project = project;
                db.ProjectCustomFields.Add(projectCustomField);
                db.SaveChanges();
                return RedirectToAction("Detail", new { id = projectId, Controller="Project" });
            }

            return View(projectCustomField);
        }

        // GET: ProjectCustomFields/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectCustomField projectCustomField = db.ProjectCustomFields.Find(id);
            if (projectCustomField == null)
            {
                return HttpNotFound();
            }
            return View(projectCustomField);
        }

        // POST: ProjectCustomFields/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProjectCustomFieldId,Field,FieldJapaneseName,Visible")] ProjectCustomField projectCustomField)
        {
            if (ModelState.IsValid)
            {
                db.Entry(projectCustomField).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(projectCustomField);
        }

        // GET: ProjectCustomFields/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectCustomField projectCustomField = db.ProjectCustomFields.Find(id);
            if (projectCustomField == null)
            {
                return HttpNotFound();
            }
            return View(projectCustomField);
        }

        // POST: ProjectCustomFields/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProjectCustomField projectCustomField = db.ProjectCustomFields.Find(id);
            db.ProjectCustomFields.Remove(projectCustomField);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
