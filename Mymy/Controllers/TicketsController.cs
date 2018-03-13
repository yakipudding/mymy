using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Mymy.DAL;
using Mymy.Logic;
using Mymy.Models;
using System.Data.Entity.Migrations;

namespace Mymy.Controllers
{
    public class TicketsController : Controller
    {
        private MymyContext db = new MymyContext();

        // GET: Tickets
        public ActionResult Index()
        {
            return View(db.Tickets.ToList());
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // GET: Tickets/Create
        public ActionResult Create(int? projectId)
        {
            var ticket = new Ticket();
            var project = db.Projects.FirstOrDefault(x => x.ProjectId == projectId);
            ticket.Project = project;

            return View(ticket);
        }

        // POST: Tickets/Create
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int projectId, int tracId, string category, string status, string status2, string link2, string memo, string detailMemo)
        {
            var ticket = new Ticket();
            var project = db.Projects.FirstOrDefault(x => x.ProjectId == projectId);

            ticket.Project = project;
            ticket.TracId = tracId;
            ticket.Category = category;
            ticket.Status = status;
            ticket.Status2 = status2;
            ticket.Link2 = link2;
            ticket.Memo = memo;
            ticket.DetailMemo = detailMemo;
            ticket.Visible = true;

            if (ModelState.IsValid)
            {
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            //CSV取得
            var project = db.Projects.FirstOrDefault(x => x.ProjectId == ticket.Project.ProjectId);
            //var csvTicket = GetTicketsLogic.GetCsvTicketFromTicket(ticket, project, project.ProjectCustomFields.ToList());
            //ticket.CsvTicket = csvTicket;
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TicketId,TracId,Category,Status,Status2,Link2,Memo,DetailMemo,Visible")] Ticket ticket)
        {
            var project = db.Tickets.FirstOrDefault(x => x.TicketId == ticket.TicketId).Project;
            ticket.Project = project;

            if (ModelState.IsValid)
            {
                //db.Entry(ticket).State = EntityState.Modified;
                db.Set<Ticket>().AddOrUpdate(ticket);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
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
