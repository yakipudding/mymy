using System;
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
            var projects = db.Projects.ToList();
            var settings = db.Settings.ToList();

            //RSS・DBからチケット取得
            var getTickets = new GetTicketsLogic(projects);

            //未登録チケットがある場合は登録
            if (getTickets.NewTickets.Any())
            {
                foreach (var newTicket in getTickets.NewTickets)
                {
                    CreateFromIndex(newTicket);
                }
            }

            //DBから再取得してCsvTicketと結合
            var tickets = db.Tickets.Where(x => x.Visible).ToList();
            foreach (var ticket in tickets)
            {
                var csvTicket = getTickets.CsvTickets.FirstOrDefault(x => x.ProjectId == ticket.Project.ProjectId
                                                                       && x.TracId == ticket.TracId);
                ticket.Summary = csvTicket.Summary;
                ticket.CsvTicket = csvTicket;

                //カテゴリのスペース分割
                ticket.Categories = ticket.Category == null ? new string[0] : ticket.Category.Split(' ');
            }

            //グルーピングして渡す
            var groupTickets = tickets.GroupBy(x => x.Project.ProjectId);
            var homeView = new HomeView();
            var homeViewProjects = new List<Project>();
            homeView.Settings = settings;

            foreach (var groupTicket in groupTickets)
            {
                var project = new Project();
                project = projects.FirstOrDefault(x => x.ProjectId == groupTicket.Key);
                var projectTickets = new List<Ticket>();
                projectTickets = groupTicket.ToList();
                project.Tickets = projectTickets;

                homeViewProjects.Add(project);           
            }
            homeView.Projects = homeViewProjects;

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
            //RSSから取得時にDBにないものは登録
            db.Tickets.Add(ticket);
            db.SaveChanges();
        }
    }
}