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
        [DisplayName("リンク")]
        public string Link { get; set; }

        //Trac取得
        [NotMapped]
        [DisplayName("作成者")]
        public string Reporter { get; set; }
        [NotMapped]
        [DisplayName("担当者")]
        public string Owner { get; set; }
        [NotMapped]
        [DisplayName("詳細")]
        public string Description { get; set; }
        [NotMapped]
        [DisplayName("分類")]
        public string Type { get; set; }
        [NotMapped]
        [DisplayName("重要度")]
        public string Proprity { get; set; }
        [NotMapped]
        [DisplayName("製品")]
        public string Component { get; set; }
        [NotMapped]
        [DisplayName("キーワード")]
        public string Keywords { get; set; }
        [NotMapped]
        [DisplayName("cc")]
        public string Cc { get; set; }
        [NotMapped]
        [DisplayName("作成日")]
        [DisplayFormat(DataFormatString = "{0:MM/dd}")]
        public DateTime? CreateDate { get; set; }
        [NotMapped]
        [DisplayName("更新日")]
        [DisplayFormat(DataFormatString = "{0:MM/dd hh:mm}")]
        public DateTime? UpdateDate { get; set; }
        [NotMapped]
        [DisplayName("期日")]
        public string DueClose { get; set; }
        [NotMapped]
        public List<TracTicketCustom> TracTicketCustoms { get; set; }

        //View用
        [NotMapped]
        public virtual TracTicket TracTicket { get; set; }
        [NotMapped]
        public string[] Categories { get; set; }
        [NotMapped]
        public virtual List<Memo> Memos { get; set; }
        [NotMapped]
        public bool? FromIndex { get; set; }
        [NotMapped]
        public string SaveAndStay { get; set; }

    }

}