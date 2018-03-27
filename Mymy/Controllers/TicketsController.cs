﻿using System;
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
using System.Configuration;

namespace Mymy.Controllers
{
    public class TicketsController : Controller
    {
        private MymyContext db = new MymyContext();

        // GET: Tickets
        public ActionResult Index(int? projectId)
        {
            return View(db.Tickets.Where(x => x.Project.ProjectId == projectId).ToList());
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

            ticket.Link = project.ProjectUrl + tracId;

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

            bool isUseTrac = ConfigurationManager.AppSettings["IsUseTrac"] == "true";
            if (isUseTrac)
            {
                var csvTicket = GetTicketsLogic.GetCsvTicketFromTicket(ticket, project, project.ProjectCustomFields.ToList());
                ticket.CsvTicket = csvTicket;
                ticket.Summary = csvTicket.Summary;
            }
            ticket.FromIndex = fromIndex;

            if (!string.IsNullOrEmpty(ticket.Category))
            {
                var memos = db.Memos.Where(x => x.Category == ticket.Category).ToList();
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
        public ActionResult Edit([Bind(Include = "TicketId,TracId,Summary,Category,Status,Status2,Link2,Memo,DetailMemo,Visible,FromIndex")] Ticket ticket)
        {
            var project = db.Tickets.FirstOrDefault(x => x.TicketId == ticket.TicketId).Project;
            ticket.Project = project;
            ticket.Link = project.ProjectUrl + "ticket/" + ticket.TracId;

            if (ModelState.IsValid)
            {
                db.Set<Ticket>().AddOrUpdate(ticket);
                db.SaveChanges();
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
