using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mymy.Models;
using Mymy.DAL;
using Mymy.Data;
using System.Net;
using System.Xml;
using System.Data;
using System.IO;

namespace Mymy.Logic
{
    public class TicketLogic
    {
        /// <summary>
        /// 戻り値用
        /// </summary>
        public class Tickets
        {
            public List<TracTicket> TracTickets = new List<TracTicket>();
            public List<Ticket> NewTickets = new List<Ticket>();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TicketLogic()
        {
            return;
        }

        /// <summary>
        /// チケット取得
        /// </summary>
        /// <param name="dbProjects"></param>
        public Tickets GetTickets(List<Project> dbProjects)
        {
            //最終的に返すチケット
            var returnTickets = new Tickets();

            //Projectごとに取りに行く
            foreach (var project in dbProjects)
            {
                //DB登録済みのチケット
                var dbProjectTickets = project.Tickets.Where(x => x.Project.ProjectId == project.ProjectId).ToList();
                //プロジェクトのカスタムフィールド
                var dbProjectCustomFields = project.ProjectCustomFields.Where(x => x.Visible).ToList();

                //Tracクエリから初期表示チケットを取得する -> returnTicketsに追加
                if (project.Condition != null)
                {
                    GetTracTicketsOnLoad(project, dbProjectTickets, dbProjectCustomFields, returnTickets);
                }
                
                //Tracで取得したチケットID
                List<int> ticketIds = returnTickets.TracTickets.Where(x => x.ProjectId == project.ProjectId).Select(x => x.TracId).ToList();

                //DBに登録されているチケットのうち、Tracで取得できなかったチケット取得
                foreach (var dbOnlyTicket in dbProjectTickets.FindAll(x => x.Visible && !ticketIds.Contains(x.TracId)))
                {
                    //Tracから最新情報取得
                    var ticket = GetTracTicketById(project, dbOnlyTicket.TracId, dbProjectCustomFields);
                    returnTickets.TracTickets.Add(ticket);
                }
            }

            return returnTickets;
        }

        /// <summary>
        /// Tracからチケットの情報取得(初期表示)
        /// </summary>
        /// <param name="project"></param>
        /// <param name="tracId"></param>
        /// <param name="dbProjectCustomFields"></param>
        /// <returns></returns>
        public void GetTracTicketsOnLoad(Project project, List<Ticket> dbProjectTickets, List<ProjectCustomField> dbProjectCustomFields, Tickets returnTickets)
        {
            var data = new DataTable();
            switch (Common.DebugMode)
            {
                case Common.DebugModeEnum.Trac:
                        data = GetTicketsDataTableFromTrac((CreateFirstTracQueryUrl(project)));
                    break;
                case Common.DebugModeEnum.LocalCsv:
                    data = GetTicketsFromLocalCsv(project, returnTickets);
                    break;
            }
            
            ConvertDataTableToTickets(project, data, dbProjectTickets, dbProjectCustomFields, returnTickets);            
        }

        /// <summary>
        /// Tracからチケットの最新情報取得(ID指定)
        /// </summary>
        /// <param name="project"></param>
        /// <param name="tracId"></param>
        /// <param name="dbProjectCustomFields"></param>
        /// <returns></returns>
        public TracTicket GetTracTicketById(Project project, int tracId, List<ProjectCustomField> dbProjectCustomFields)
        {
            var dt = new DataTable();
            //最新情報取得
            switch (Common.DebugMode)
            {
                case Common.DebugModeEnum.Trac:
                    dt = GetTicketsDataTableFromTrac(CreateTracTicketQueryUrl(project, tracId));
                    break;
                case Common.DebugModeEnum.LocalCsv:
                    dt = GetTicketDataTableFromLocalCsv(project, tracId);
                    break;
            }

            return ConvertRowToTracTicket(dt.Rows[0], project.ProjectId, dbProjectCustomFields);
        }

        #region Trac接続

        /// <summary>
        /// Tracクエリで取得した情報をDataTableに変換します
        /// </summary>
        /// <param name="queryUrl">クエリURL</param>
        /// <returns></returns>
        private DataTable GetTicketsDataTableFromTrac(string queryUrl)
        {
            var data = new DataTable();

            try
            {
                var request = HttpWebRequest.Create(queryUrl);
                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    data = CommonLogic.ConvertStreamToDataTable(sr);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return data;
        }

        /// <summary>
        /// Tracから取得したDataTable型のチケットを変換し、returnTicket.TracTicketsリストに追加します
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="xml"></param>
        private Tickets ConvertDataTableToTickets(Project project, DataTable data, List<Ticket> dbProjectTicket, List<ProjectCustomField> dbProjectCustomFields, Tickets returnTickets)
        {
            foreach (DataRow row in data.Rows)
            {
                //DataRowからTracチケットに変換
                var tracTicket = ConvertRowToTracTicket(row, project.ProjectId, dbProjectCustomFields);
                returnTickets.TracTickets.Add(tracTicket);
                
                //DBに登録済みかどうか確認
                var dbTicket = dbProjectTicket?.FirstOrDefault(x => x.TracId == tracTicket.TracId);
                if (dbTicket == null)
                {
                    var ticket = new Ticket();
                    ticket.Project = project;
                    ticket.TracId = tracTicket.TracId;
                    ticket.Link = project.ProjectUrl + "ticket/" + tracTicket.TracId;
                    ticket.Visible = true;
                    ticket.Summary = tracTicket.Summary;

                    //DB登録リストに追加
                    returnTickets.NewTickets.Add(ticket);
                }
            }

            return returnTickets;

        }

        /// <summary>
        /// DataRowのチケットをTracTicketに変換します
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="projectId">プロジェクトID</param>
        /// <param name="dbProjectCustomFields">プロジェクトカスタムフィールド</param>
        /// <returns></returns>
        private TracTicket ConvertRowToTracTicket(DataRow row, int projectId, List<ProjectCustomField> dbProjectCustomFields)
        {
            var tracTicket = new TracTicket();

            var tracId = int.Parse(row["id"].ToString());
            var summary = row["summary"].ToString();
            //var reporter = row["reporter"].ToString();        //ものによってはいらないのでカスタムフィールドで
            //var owner = row["owner"].ToString();              //ものによってはいらないのでカスタムフィールドで
            var createdate_str = row["time"].ToString();          //一覧にはあるけどチケットにない
            var updatedate_str = row["changetime"].ToString();    //一覧にはあるけどチケットにない
            var status = row["status"].ToString();
            var dueclosedate_str = row["due_close"].ToString(); //期日
                                                                //他の項目もあるけど今のところ使わない

            tracTicket.ProjectId = projectId;
            tracTicket.TracId = tracId;
            tracTicket.Summary = summary;            
            //tracTicket.Reporter = reporter;
            //tracTicket.Owner = owner;
            tracTicket.CreateDate = DateTime.Parse(createdate_str);
            tracTicket.UpdateDate = DateTime.Parse(updatedate_str);
            tracTicket.Status = status;
            tracTicket.DueClose = dueclosedate_str;

            //カスタムフィールド分
            tracTicket.TracTicketCustoms = new List<TracTicketCustom>();
            if (dbProjectCustomFields != null)
            {
                foreach (var item in dbProjectCustomFields)
                {
                    var tracTicketCustom = new TracTicketCustom();
                    tracTicketCustom.Field = item.Field;
                    tracTicketCustom.FieldJapaneseName = item.FieldJapaneseName;
                    tracTicketCustom.FieldValue = row[item.Field].ToString();
                    tracTicketCustom.DisplayOrder = item.DisplayOrder;

                    tracTicket.TracTicketCustoms.Add(tracTicketCustom);
                }
            }

            return tracTicket;
        }        

        /// <summary>
        /// 初期表示用URL作成
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private string CreateFirstTracQueryUrl(Project project)
        {
            return project.ProjectUrl + "query?" + project.Condition + "&format=csv&" + project.Column;
        }

        /// <summary>
        /// チケット指定URL作成
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private string CreateTracTicketQueryUrl(Project project, int tracId)
        {
            return project.ProjectUrl + "query?id=" + tracId.ToString() + "&format=csv&" + project.Column;
        }

        #endregion

        #region デバッグ用：ローカルのcsvファイルからチケットを読み込む

        /// <summary>
        /// デバッグ用：ローカルのcsvファイルからチケットを読み込みます[c:\tmp\[project_id]\test.csv
        /// </summary>
        /// <param name="dbProjects"></param>
        /// <param name="project"></param>
        /// <param name="returnTickets"></param>
        /// <returns></returns>
        private DataTable GetTicketsFromLocalCsv(Project project, Tickets returnTickets)
        {
            //ファイルから取得（テスト用）
            var testData = new DataTable();
            var sr = new StreamReader(@"C:\tmp\" + project.ProjectId + @"\test.csv", System.Text.Encoding.GetEncoding("shift_jis"));
            testData = CommonLogic.ConvertStreamToDataTable(sr);
            sr.Dispose();

            //return ConvertDataTableToTickets(project, testData, project.Tickets.ToList(), project.ProjectCustomFields.ToList(), returnTickets);
            return testData;

        }

        /// <summary>
        /// デバッグ用：ローカルのcsvファイルからチケットを読み込みます[c:\tmp\[project_id]\[ticket_id].csv
        /// </summary>
        /// <param name="tracId"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        private DataTable GetTicketDataTableFromLocalCsv(Project project, int tracId)
        {
            //ファイルから取得（テスト用）
            var testData = new DataTable();
            var sr = new StreamReader(@"C:\tmp\" + project.ProjectId + @"\" + tracId.ToString() + ".csv", System.Text.Encoding.GetEncoding("shift_jis"));
            testData = CommonLogic.ConvertStreamToDataTable(sr);
            sr.Dispose();

            return testData;
        }

        #endregion

    }
}