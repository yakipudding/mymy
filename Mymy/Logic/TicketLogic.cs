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
        public class TicketForHomeDto
        {
            public HomeView HomeView = new HomeView();
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
        public TicketForHomeDto GetTickets(List<Project> dbProjects)
        {
            //最終的に返すチケット
            var retDto = new TicketForHomeDto();
            retDto.HomeView.Projects = new List<Project>();
            var newTickets = retDto.NewTickets;

            //Projectごとに取りに行く
            foreach (var project in dbProjects)
            {
                var retTickets = new List<Ticket>();

                //DB登録済みのチケット
                var dbProjectTickets = project.Tickets.Where(x => x.Project.ProjectId == project.ProjectId).ToList();
                //プロジェクトのカスタムフィールド
                var dbProjectCustomFields = project.ProjectCustomFields.Where(x => x.Visible).ToList();

                //Tracクエリから初期表示チケットを取得する -> returnTicketsに追加
                if (project.Condition != null)
                {
                    GetTracTicketsOnLoad(project, dbProjectTickets, dbProjectCustomFields, retTickets, newTickets);
                }
                
                //Tracで取得したチケットID
                List<int> ticketIds = retTickets.Where(x => x.Project.ProjectId == project.ProjectId).Select(x => x.TracId).ToList();

                //DBに登録されているチケットのうち、Tracで取得できなかったチケット取得
                foreach (var dbOnlyTicket in dbProjectTickets.FindAll(x => x.Visible && !ticketIds.Contains(x.TracId)))
                {
                    //Tracから最新情報取得
                    GetTracTicketById(project, dbOnlyTicket, dbProjectCustomFields);
                    retTickets.Add(dbOnlyTicket);
                }

                if (retTickets.Any())
                {
                    //HomeView追加
                    var retProject = new Project();
                    retProject.ProjectId = project.ProjectId;
                    retProject.ProjectName = project.ProjectName;
                    retProject.ProjectCustomFields = project.ProjectCustomFields;
                    retProject.Tickets = retTickets.Where(x => x.Visible == true).ToList();
                    retDto.HomeView.Projects.Add(retProject);
                }

            }

            return retDto;
        }

        /// <summary>
        /// Tracからチケットの情報取得(初期表示)
        /// </summary>
        /// <param name="project"></param>
        /// <param name="tracId"></param>
        /// <param name="dbProjectCustomFields"></param>
        /// <returns></returns>
        public void GetTracTicketsOnLoad(Project project, List<Ticket> dbProjectTickets, List<ProjectCustomField> dbProjectCustomFields, List<Ticket> retTickets, List<Ticket> newTickets)
        {
            var data = new DataTable();
            switch (Common.DebugMode)
            {
                case Common.DebugModeEnum.Trac:
                        data = GetTicketsDataTableFromTrac((CreateFirstTracQueryUrl(project)));
                    break;
                case Common.DebugModeEnum.LocalCsv:
                    data = GetTicketsFromLocalCsv(project);
                    break;
            }
            
            ConvertDataTableToTickets(project, data, dbProjectTickets, dbProjectCustomFields, retTickets, newTickets);            
        }

        /// <summary>
        /// Tracからチケットの最新情報取得(ID指定)
        /// </summary>
        /// <param name="project"></param>
        /// <param name="tracId"></param>
        /// <param name="dbProjectCustomFields"></param>
        /// <returns></returns>
        public Ticket GetTracTicketById(Project project, Ticket ticket, List<ProjectCustomField> dbProjectCustomFields)
        {
            var dt = new DataTable();
            //最新情報取得
            switch (Common.DebugMode)
            {
                case Common.DebugModeEnum.Trac:
                    dt = GetTicketsDataTableFromTrac(CreateTracTicketQueryUrl(project, ticket.TracId));
                    break;
                case Common.DebugModeEnum.LocalCsv:
                    dt = GetTicketDataTableFromLocalCsv(project, ticket.TracId);
                    break;
            }

            return ConvertRowToTicket(dt.Rows[0], project,ticket, dbProjectCustomFields);
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
        private void ConvertDataTableToTickets(Project project, DataTable data, List<Ticket> dbProjectTicket, List<ProjectCustomField> dbProjectCustomFields, List<Ticket> retTickets, List<Ticket> newTickets)
        {
            foreach (DataRow row in data.Rows)
            {
                //チケット取得
                var tracTicket = new Ticket();
                tracTicket.Project = project;

                //DataRowからチケットに変換
                ConvertRowToTicket(row, project, tracTicket, dbProjectCustomFields);
                retTickets.Add(tracTicket);
                
                //DBに登録済みかどうか確認
                var dbTicket = dbProjectTicket?.FirstOrDefault(x => x.TracId == tracTicket.TracId);
                if (dbTicket == null)
                {
                    //なければ新規作成
                    //var newTicket = new Ticket();
                    //newTicket.Project = project;
                    //newTicket.TracId = tracTicket.TracId;
                    tracTicket.Link = project.ProjectUrl + "ticket/" + tracTicket.TracId;
                    tracTicket.Visible = true;
                    //newTicket.Summary = tracTicket.Summary;
                    //カテゴリのスペース分割
                    tracTicket.Categories = new string[0];

                    //DB登録リストに追加
                    newTickets.Add(tracTicket);
                }
                else
                {
                    //DBに登録済みであればDBチケットをマージ
                    var ticket = retTickets.Find(x => x.Project.ProjectId == project.ProjectId && x.TracId == tracTicket.TracId);
                    ticket.TicketId = dbTicket.TicketId;
                    ticket.Category = dbTicket.Category;
                    ticket.Status = dbTicket.Status;
                    ticket.Status2 = dbTicket.Status2;
                    ticket.Link2 = dbTicket.Link2;
                    ticket.Memo = dbTicket.Memo;
                    ticket.DetailMemo = dbTicket.DetailMemo;
                    ticket.Visible = dbTicket.Visible;
                    ticket.Link = dbTicket.Link;
                    //カテゴリのスペース分割
                    ticket.Categories = ticket.Category == null ? new string[0] : ticket.Category.Split(' ');

                }
            }

            //return returnTickets;

        }

        /// <summary>
        /// DataRowのチケットをTracTicketに変換します
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="projectId">プロジェクトID</param>
        /// <param name="dbProjectCustomFields">プロジェクトカスタムフィールド</param>
        /// <returns></returns>
        private Ticket ConvertRowToTicket(DataRow row, Project project, Ticket tracTicket,  List<ProjectCustomField> dbProjectCustomFields)
        {
            //var tracTicket = new Ticket();

            var tracId = int.Parse(row["id"].ToString());
            var summary = row["summary"].ToString();
            //var reporter = row["reporter"].ToString();        //ものによってはいらないのでカスタムフィールドで
            //var owner = row["owner"].ToString();              //ものによってはいらないのでカスタムフィールドで
            var createdate_str = row["time"].ToString();          //一覧にはあるけどチケットにない
            var updatedate_str = row["changetime"].ToString();    //一覧にはあるけどチケットにない
            var status = row["status"].ToString();
            var dueclosedate_str = row["due_close"].ToString(); //期日
                                                                //他の項目もあるけど今のところ使わない

            tracTicket.Project = project;
            tracTicket.TracId = tracId;
            tracTicket.Summary = summary;            
            //tracTicket.Reporter = reporter;
            //tracTicket.Owner = owner;
            tracTicket.CreateDate = DateTime.Parse(createdate_str);
            tracTicket.UpdateDate = DateTime.Parse(updatedate_str);
            tracTicket.Status = status;
            tracTicket.DueClose = dueclosedate_str;
            //カテゴリのスペース分割
            tracTicket.Categories = tracTicket.Category == null ? new string[0] : tracTicket.Category.Split(' ');

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
        private DataTable GetTicketsFromLocalCsv(Project project)
        {
            //ファイルから取得（テスト用）
            var testData = new DataTable();
            var sr = new StreamReader(@"C:\tmp\" + project.ProjectId + @"\test.csv", System.Text.Encoding.GetEncoding("shift_jis"));
            testData = CommonLogic.ConvertStreamToDataTable(sr);
            sr.Dispose();
            
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