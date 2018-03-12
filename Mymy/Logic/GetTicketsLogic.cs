using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mymy.Models;
using Mymy.DAL;
using System.Net;
using System.Xml;
using System.Data;
using System.IO;

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

                //CSVから取得
                if (project.CsvUrl != null)
                {
                    var data = GetDataFromCsv(project.CsvUrl);
                    ConvertDataTableToTickets(project, data, dbProjectTickets); //ここでListに追加
                }

                //DBから取得（個人設定　プロジェクト登録分）
                foreach (var dbProjectTicket in dbProjectTickets.FindAll(x => x.Visible))
                {
                    var containCsv = Tickets.FirstOrDefault(x => x.TracId == dbProjectTicket.TracId);
                    if (containCsv == null)
                    {
                        //CSVから最新情報取得
                        string CsvUrl = project.TicketUrl + dbProjectTicket.TracId + "?type=csv";
                        var data = GetDataFromCsv(CsvUrl);
                        Tickets.Add(UpdateTicketFromData(dbProjectTicket, data));
                    }
                }
            }

            //ファイルから取得（テスト用）
            var testData = new DataTable();
            var sr = new StreamReader(@"C:\tmp\test.csv", System.Text.Encoding.GetEncoding("shift_jis"));
            var line = sr.ReadLine();
            var headers = line.Split(',');

            foreach (var head in headers)
            {
                testData.Columns.Add(head);
            }
            // ストリームの末尾まで繰り返す
            while (!sr.EndOfStream)
            {
                string[] rows = sr.ReadLine().Split(',');
                DataRow dr = testData.NewRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    dr[i] = rows[i];
                }
                testData.Rows.Add(dr);
            }
            var testProject = DbProjects.FirstOrDefault(x => x.ProjectId == 1);
            ConvertDataTableToTickets(testProject, testData, testProject.Tickets.ToList());

            return;
        }

        /// <summary>
        /// CSVから取得した情報をDataTableに変換する
        /// </summary>
        /// <param name="csvUrl"></param>
        /// <returns></returns>
        private DataTable GetDataFromCsv(string csvUrl)
        {
            var data = new DataTable();

            if (!IsUseRSS) return data;

            try
            {
                var request = HttpWebRequest.Create(csvUrl);
                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    // ファイルから一行読み込む
                    var line = sr.ReadLine();
                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    var headers = line.Split(',');

                    foreach (var head in headers)
                    {
                        data.Columns.Add(head);
                    }

                    // ストリームの末尾まで繰り返す
                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        DataRow dr = data.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i];
                        }
                        data.Rows.Add(dr);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return data;
        }

        /// <summary>
        /// RSSから取得した情報をリストに追加する
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="xml"></param>
        private void ConvertDataTableToTickets(Project project, DataTable data, List<Ticket> dbProjectTicket)
        {
            foreach (DataRow row in data.Rows)
            {
                var ticket = new Ticket();

                var link = project.TicketUrl + row["id"];
                var summary = row["summary"].ToString();
                var repoter = row["repoter"].ToString();
                var owner = row["owner"].ToString();
                var createdate_str = row["time"].ToString();
                var updatedate_str = row["changetime"].ToString();

                var dueclosedate_str = row["due_close"].ToString(); //期日
                var status = row["status2"].ToString();  //

                ticket.Project = project;

                ticket.Link = link;
                ticket.Summary = summary;
                ticket.Repoter = repoter;
                ticket.Owner = owner;
                ticket.CreateDate = DateTime.Parse(createdate_str);
                ticket.UpdateDate = DateTime.Parse(updatedate_str);

                ticket.DueCloseDate = DateTime.Parse(dueclosedate_str);
                ticket.Status = status;

                ticket.TracId = int.Parse(row["id"].ToString());

                if (dbProjectTicket != null)
                {
                    //データが存在するか確認
                    var dbTicket = dbProjectTicket.FirstOrDefault(x => x.TracId == ticket.TracId);
                    if (dbTicket != null)
                    {
                        ticket.Category = dbTicket.Category;
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
        private Ticket UpdateTicketFromData(Ticket ticket, DataTable data)
        {
            if (!IsUseRSS) return ticket;

            var row = data.Rows[0];
            
            var summary = row["summary"].ToString();
            var owner = row["owner"].ToString();
            var updatedate_str = row["changetime"].ToString();

            var dueclosedate_str = row["due_close"].ToString(); //期日
            var status = row["status2"].ToString();  //
            
            ticket.Summary = summary;
            ticket.Owner = owner;
            ticket.UpdateDate = DateTime.Parse(updatedate_str);

            ticket.DueCloseDate = DateTime.Parse(dueclosedate_str);
            ticket.Status = status;
            ticket.TracId = int.Parse(row["id"].ToString());

            return ticket;

        }
    }
}