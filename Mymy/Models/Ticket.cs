using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mymy.Models
{
    public class Ticket
    {
        [DisplayName("チケットID")]
        public int TicketId { get; set; }

        [DisplayName("プロジェクト")]
        public virtual Project Project { get; set; }
        [DisplayName("TracID")]
        public int TracId { get; set; }
        [DisplayName("チケット名")]
        public string Summary { get; set; }
        [DisplayName("カテゴリ")]
        public string Category { get; set; }
        [DisplayName("状態")]
        public string Status { get; set; }
        [DisplayName("同時並行")]
        public string Status2 { get; set; }
        [DisplayName("関連")]
        public string Link2 { get; set; }
        [DisplayName("メモ")]
        public string Memo { get; set; }
        [DisplayName("詳細メモ")]
        [DataType(DataType.MultilineText)]
        public string DetailMemo { get; set; }
        [DisplayName("表示")]
        public bool Visible { get; set; }

        public string Link { get; set; }

        [NotMapped]
        public virtual CsvTicket CsvTicket { get; set; }
        [NotMapped]
        public virtual List<Memo> Memos { get; set; }
        [NotMapped]
        public bool? FromIndex { get; set; }

    }

}