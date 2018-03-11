using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

            //グルーピングして渡す
            var groupTickets = getTickets.Tickets.GroupBy(x => x.Project.ProjectId);
            var homeView = new HomeView();
            var homeViewProjects = new List<Project>();
            homeView.Settings = settings;

            foreach (var groupTicket in groupTickets)
            {
                var project = new Project();
                project = projects.FirstOrDefault(x => x.ProjectId == groupTicket.Key);
                var tickets = new List<Ticket>();
                tickets = groupTicket.ToList();
                project.Tickets = tickets;

                homeViewProjects.Add(project);           
            }
            homeView.Projects = homeViewProjects;

            return View(homeView);
        }

        private void CreateFromIndex(Ticket ticket)
        {
            //RSSから取得時にDBにないものは登録
            db.Tickets.Add(ticket);
            db.SaveChanges();
        }
    }
}