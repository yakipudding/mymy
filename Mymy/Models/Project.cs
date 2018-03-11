using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mymy.Models
{
    public class Project
    {
        [DisplayName("プロジェクトID")]
        public int ProjectId { get; set; }
        [DisplayName("プロジェクト名")]
        public string ProjectName { get; set; }
        [DisplayName("チケットURL")]
        public string TicketUrl { get; set; }
        [DisplayName("一覧のRSSURL")]
        public string RssUrl { get; set; }
        [DisplayName("特定チケットのRSSURL")]
        public string RssTicketUrl { get; set; }
        [DisplayName("メモ")]
        public string Memo { get; set; }
        [DisplayName("詳細メモ")]
        [DataType(DataType.MultilineText)]
        public string DetailMemo { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}