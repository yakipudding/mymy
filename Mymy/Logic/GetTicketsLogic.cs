using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mymy.Models;
using Mymy.DAL;
using System.Net;
using System.Xml;

namespace Mymy.Logic
{
    public class GetTicketsLogic
    {
        public List<Ticket> Tickets = new List<Ticket>();
        public List<Ticket> NewTickets = new List<Ticket>();
        
        private List<Project> DbProjects;

        private bool IsUseRSS = false;
        
        public GetTicketsLogic(List<Project> projects)
        {
            DbProjects = projects;            

            GetTicketList();
        }

        private void GetTicketList()
        {
            foreach (var project in DbProjects)
            {
                var dbProjectTickets = project.Tickets.ToList();

                //RSSから取得
                if (project.RssUrl != null)
                {
                    var xml = GetXmlFromRSS(project.RssUrl);
                    ConvertXmlToTickets(project, xml, dbProjectTickets); //ここでListに追加
                }

                //DBから取得（個人設定　プロジェクト登録分）
                foreach (var dbProjectTicket in dbProjectTickets.FindAll(x => x.Visible))
                {
                    var containRss = Tickets.FirstOrDefault(x => x.TracId == dbProjectTicket.TracId);
                    if (containRss == null)
                    {
                        //RSSから最新情報取得
                        //TODO RSSのURLをどう作るか
                        string rssURL = project.RssTicketUrl + dbProjectTicket.TracId;
                        var xml = GetXmlFromRSS(rssURL);
                        Tickets.Add(UpdateTicketFromXml(dbProjectTicket, xml));
                    }
                }
            }
            
            //ファイルから取得（テスト用）
            var xmlF = new XmlDocument();
            xmlF.Load(@"C:\tmp\test.xml");
            var testProject = DbProjects.FirstOrDefault(x => x.ProjectId == 1);
            ConvertXmlToTickets(testProject, xmlF, testProject.Tickets.ToList());

            return;
        }

        /// <summary>
        /// RSSから取得した情報をXMLに変換する
        /// </summary>
        /// <param name="rssUrl"></param>
        /// <returns></returns>
        private XmlDocument GetXmlFromRSS(string rssUrl)
        {
            var xml = new XmlDocument();

            if (!IsUseRSS) return xml;

            try
            {
                var request = HttpWebRequest.Create(rssUrl);
                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();
                var stream = response.GetResponseStream();
                xml.Load(stream);
            }
            catch (Exception)
            {
                throw;
            }

            return xml;
        }

        /// <summary>
        /// RSSから取得した情報をリストに追加する
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="xml"></param>
        private void ConvertXmlToTickets(Project project, XmlDocument xml, List<Ticket> dbProjectTicket)
        {
            var nodelist = xml.SelectNodes(@"rss/channel/item");

            foreach (XmlNode node in nodelist)
            {
                var ticket = new Ticket();

                var link = node.SelectSingleNode(@"link").InnerText;
                var title = node.SelectSingleNode(@"title").InnerText;
                var date_str = node.SelectSingleNode(@"pubDate").InnerText;
                var description = node.SelectSingleNode(@"description").InnerText;
                var creator = (string)null; //, node.SelectSingleNode(@"creator").InnerText
                
                ticket.Project = project;
                ticket.Link = link;
                ticket.Title = title;
                ticket.Date = DateTime.Parse(date_str);
                ticket.Description = description;
                ticket.Creator = creator;

                ticket.TracId = Ticket.GetTracId(link);

                if (dbProjectTicket != null)
                {
                    //データが存在するか確認
                    var dbTicket = dbProjectTicket.FirstOrDefault(x => x.TracId == ticket.TracId);
                    if (dbTicket != null)
                    {
                        ticket.Category = dbTicket.Category;
                        ticket.Status = dbTicket.Status;
                        ticket.Status2 = dbTicket.Status2;
                        ticket.Link2 = dbTicket.Link2;
                        ticket.Memo = dbTicket.Memo;
                        ticket.DetailMemo = dbTicket.DetailMemo;
                        ticket.Visible = dbTicket.Visible; //これはどっち優先するか考えたほうが。

                    }
                    else
                    {
                        ticket.Visible = true;

                        //データが存在しないため、新規登録を行う
                        NewTickets.Add(ticket);
                    }

                }

                Tickets.Add(ticket);
            }

            return;

        }

        /// <summary>
        /// RSSから取得した最新情報分をアップデートする
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        private Ticket UpdateTicketFromXml(Ticket ticket, XmlDocument xml)
        {
            if (!IsUseRSS) return ticket;

            var node = xml.SelectSingleNode(@"rss/channel/item");

            ticket.Link = node.SelectSingleNode(@"link").InnerText;
            ticket.Title = node.SelectSingleNode(@"title").InnerText;
            ticket.Description = node.SelectSingleNode(@"description").InnerText;
            ticket.TracId = Ticket.GetTracId(ticket.Link);

            return ticket;

        }
    }
}