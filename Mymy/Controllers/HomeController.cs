﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Migrations;
using Mymy.DAL;
using Mymy.Logic;
using Mymy.Models;

namespace Mymy.Controllers
{
    public class HomeController : Controller
    {
        private MymyContext db = new MymyContext();

        public ActionResult Index()
        {
            var projects = db.Projects.OrderBy(x => x.OrderIndex).ToList();
            var settings = db.Settings.ToList();

            //Trac・DBからチケット取得
            var homeView = new HomeView();
            var newTickets = new List<Ticket>();
            new TicketLogic().GetTickets(projects, homeView, newTickets);

            //未登録チケットがある場合は登録
            if (newTickets.Any())
            {
                foreach (var newTicket in newTickets)
                {
                    CreateFromIndex(newTicket);
                }
            }
            
            homeView.Settings = settings;

            return View(homeView);
        }

        // POST: Tickets/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        //[HttpPost]
        public ActionResult InVisible(int? id)
        {
            //チケットの取得
            var ticket = db.Tickets.Where(x => x.TicketId == id).FirstOrDefault();
            ticket.Visible = false;

            //非表示登録
            db.Set<Ticket>().AddOrUpdate(ticket);
            db.SaveChanges();
            
            return RedirectToAction("Index", "Home");
        }

        private void CreateFromIndex(Ticket ticket)
        {
            //Trac取得時にDBにないものは登録
            db.Tickets.Add(ticket);
            db.SaveChanges();
        }
    }
}