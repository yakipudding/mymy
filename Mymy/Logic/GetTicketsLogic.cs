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
        public List<CsvTicket> CsvTickets = new List<CsvTicket>();
        public List<Ticket> NewTickets = new List<Ticket>();
        
        private bool IsUseCSV = false;
        
        public GetTicketsLogic(List<Project> dbProjects)
        {
            foreach (var project in dbProjects)
            {
                var dbProjectTickets = project.Tickets.ToList();
                var dbProjectCustomField = project.ProjectCustomFields.Where(x => x.Visible).ToList();

                //CSVから取得
                if (project.CsvUrl != null)
                {
                    var data = GetDataFromCsv(project.CsvUrl);
                    ConvertDataTableToTickets(project, data, dbProjectTickets, dbProjectCustomField); //ここでListに追加
                }

                //DBから取得（個人設定　プロジェクト登録分）
                foreach (var dbProjectTicket in dbProjectTickets.FindAll(x => x.Visible))
                {
                    var containCsv = CsvTickets.FirstOrDefault(x => x.TracId == dbProjectTicket.TracId);
                    if (containCsv == null)
                    {
                        //CSVから最新情報取得
                        if (IsUseCSV)
                        {
                            string CsvUrl = project.TicketUrl + dbProjectTicket.TracId + "?type=csv";
                            var data = GetDataFromCsv(CsvUrl);
                            CsvTickets.Add(UpdateTicketFromData(dbProjectTicket, data, dbProjectCustomField));
                        }
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
            var testProject = dbProjects.FirstOrDefault(x => x.ProjectId == 1);
            ConvertDataTableToTickets(testProject, testData, testProject.Tickets.ToList(), testProject.ProjectCustomFields.ToList());

            return;
        }

        /// <summary>
        /// CSVから取得した情報をDataTableに変換する
        /// </summary>
        /// <param name="csvUrl"></param>
        /// <returns></returns>
        private static DataTable GetDataFromCsv(string csvUrl)
        {
            var data = new DataTable();

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
        /// CSVから取得した情報をリストに追加する
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="xml"></param>
        private void ConvertDataTableToTickets(Project project, DataTable data, List<Ticket> dbProjectTicket, List<ProjectCustomField> dbProjectCustomFields)
        {
            foreach (DataRow row in data.Rows)
            {
                var csvTicket = ConvertRowToCstTicket(row, project.ProjectId, dbProjectCustomFields);                

                CsvTickets.Add(csvTicket);

                //DBに存在しない場合、足す
                if (dbProjectTicket != null)
                {
                    //データが存在するか確認
                    var dbTicket = dbProjectTicket.FirstOrDefault(x => x.TracId == csvTicket.TracId);
                    if (dbTicket == null)
                    {
                        var ticket = new Ticket();
                        ticket.Project = project;
                        ticket.TracId = csvTicket.TracId;
                        ticket.Visible = true;

                        //データが存在しないため、新規登録を行う
                        NewTickets.Add(ticket);
                    }

                }

            }

            return;

        }       

        /// <summary>
        /// CSSから取得した最新情報分をアップデートする
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        private CsvTicket UpdateTicketFromData(Ticket ticket, DataTable data, List<ProjectCustomField> dbProjectCustomFields)
        {
            var csvTicket = new CsvTicket();

            if (!IsUseCSV) return csvTicket;

            var row = data.Rows[0];
            
            csvTicket = ConvertRowToCstTicket(row, ticket.Project.ProjectId, dbProjectCustomFields);            

            return csvTicket;

        }

        private static CsvTicket ConvertRowToCstTicket(DataRow row, int projectId, List<ProjectCustomField> dbProjectCustomFields)
        {
            var csvTicket = new CsvTicket();

            var tracId = int.Parse(row["id"].ToString());
            var summary = row["summary"].ToString();
            var reporter = row["reporter"].ToString();
            var owner = row["owner"].ToString();
            //var createdate_str = row["time"].ToString();          //一覧にはあるけどチケットにない
            //var updatedate_str = row["changetime"].ToString();    //一覧にはあるけどチケットにない
            var status = row["status"].ToString();
            var dueclosedate_str = row["due_close"].ToString(); //期日
                                                                //他の項目もあるけど今のところ使わない

            csvTicket.ProjectId = projectId;
            csvTicket.TracId = tracId;
            csvTicket.Summary = summary;
            csvTicket.Reporter = reporter;
            csvTicket.Owner = owner;
            //ticket.CreateDate = DateTime.Parse(createdate_str);
            //ticket.UpdateDate = DateTime.Parse(updatedate_str);
            csvTicket.Status = status;
            csvTicket.DueClose = dueclosedate_str;

            //カスタムフィールド分
            csvTicket.CsvTicketCustoms = new List<CsvTicketCustom>();
            if (dbProjectCustomFields != null)
            {
                foreach (var item in dbProjectCustomFields)
                {
                    var csvTicketCustom = new CsvTicketCustom();
                    csvTicketCustom.Field = item.Field;
                    csvTicketCustom.FieldJapaneseName = item.FieldJapaneseName;
                    csvTicketCustom.FieldValue = row[item.Field].ToString();

                    csvTicket.CsvTicketCustoms.Add(csvTicketCustom);
                }
            }

            return csvTicket;
        }

        public static CsvTicket GetCsvTicketFromTicket(Ticket ticket, Project project, List<ProjectCustomField> dbProjectCustomFields)
        {
            //CSVから最新情報取得
            string CsvUrl = project.TicketUrl + ticket.TracId + "?type=csv";
            var data = GetDataFromCsv(CsvUrl);
            return ConvertRowToCstTicket(data.Rows[0], project.ProjectId, dbProjectCustomFields);
        }
    }
}