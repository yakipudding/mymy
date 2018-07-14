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
using Mymy.Data;
using System.Data.Entity.Migrations;
using System.Configuration;

namespace Mymy.Controllers
{
    public class TicketsController : Controller
    {
        private MymyContext db = new MymyContext();

        // GET: Tickets
        public ActionResult Index(int? projectId, string category, string summary, OldTicketsView.DisplayEnum? display)
        {
            var tickets = db.Tickets.Where(x => x.Project.ProjectId == projectId
                                             && (string.IsNullOrEmpty(category) || !string.IsNullOrEmpty(category) && x.Category.Contains(category))
                                             && (string.IsNullOrEmpty(summary) || !string.IsNullOrEmpty(summary) && x.Summary.Contains(summary))
                                             && (display == null
                                              || display == OldTicketsView.DisplayEnum.All
                                              || display == OldTicketsView.DisplayEnum.VisibleOnly && x.Visible
                                              || display == OldTicketsView.DisplayEnum.InVisibleOnly && !x.Visible)
                                           ).ToList();
            foreach (var ticket in tickets)
            {
                //カテゴリのスペース分割
                ticket.Categories = ticket.Category == null ? new string[0] : ticket.Category.Split(' ');
            }

            var oldTicketsView = new OldTicketsView();
            oldTicketsView.Tickets = tickets;
            oldTicketsView.Project = db.Projects.Where(x => x.ProjectId == projectId).FirstOrDefault();
            oldTicketsView.Category = category;
            oldTicketsView.Summary = summary;

            return View(oldTicketsView);
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
        public ActionResult Create(int projectId, int tracId, string summary, string category, string status, string status2, string link2, string memo, string detailMemo)
        {
            var ticket = new Ticket();
            var project = db.Projects.FirstOrDefault(x => x.ProjectId == projectId);

            ticket.Project = project;
            ticket.TracId = tracId;
            ticket.Summary = summary;
            ticket.Category = category;
            ticket.Status = status;
            ticket.Status2 = status2;
            ticket.Link2 = link2;
            ticket.Memo = memo;
            ticket.DetailMemo = detailMemo;
            ticket.Visible = true;

            ticket.Link = project.ProjectUrl + "ticket/" + tracId;

            if (ModelState.IsValid)
            {
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id, bool? fromIndex)
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
            
            if (Common.DebugMode == Common.DebugModeEnum.Trac)
            {
                var csvTicket = new TicketLogic().GetTracTicketById(project, ticket, project.ProjectCustomFields.ToList());
                //ticket.TracTicket = csvTicket;
                ticket.Summary = csvTicket.Summary;
            }
            ticket.FromIndex = fromIndex;

            if (!string.IsNullOrEmpty(ticket.Category))
            {
                //カテゴリのスペース分割
                ticket.Categories = ticket.Category == null ? new string[0] : ticket.Category.Split(' ');
                var memos = new List<Memo>();
                foreach (var category in ticket.Categories)
                {
                    var memo = db.Memos.Where(x => x.Category == category).ToList();
                    if (memo != null)
                    {
                        foreach (var item in memo)
                        {
                            memos.Add(item);
                        }
                    }
                }
                ticket.Memos = memos;
            }
            else
            {
                ticket.Memos = new List<Memo>();
            }

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TicketId,TracId,Summary,Category,Status,Status2,Link2,Memo,DetailMemo,Visible,FromIndex,SaveAndStay")] Ticket ticket)
        {
            var project = db.Tickets.FirstOrDefault(x => x.TicketId == ticket.TicketId).Project;
            ticket.Project = project;
            ticket.Link = project.ProjectUrl + "ticket/" + ticket.TracId;

            if (ModelState.IsValid)
            {
                db.Set<Ticket>().AddOrUpdate(ticket);
                db.SaveChanges();
                if (!string.IsNullOrEmpty(ticket.SaveAndStay))
                {
                    return RedirectToAction("Edit", "Tickets", new { id = ticket.TicketId, fromIndex = ticket.FromIndex });
                }
                if (ticket.FromIndex == true)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Tickets", new { projectId = project.ProjectId });
                }
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
