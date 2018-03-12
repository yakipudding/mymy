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
        [DisplayName("チケットURL")]
        public string Link { get; set; }
        [DisplayName("タイトル")]
        public string Summary { get; set; }
        [DisplayName("作成者")]
        public string Repoter { get; set; }
        [DisplayName("担当者")]
        public string Owner { get; set; }
        [DisplayName("作成日")]
        [DisplayFormat(DataFormatString = "{0:MM/dd}")]
        public DateTime CreateDate { get; set; }
        [DisplayName("更新日")]
        [DisplayFormat(DataFormatString = "{0:MM/dd}")]
        public DateTime UpdateDate { get; set; }

        [DisplayName("期日")]
        [DisplayFormat(DataFormatString = "{0:MM/dd}")]
        public DateTime DueCloseDate { get; set; }
        [DisplayName("状態")]
        public string Status { get; set; }

        [DisplayName("TracID")]
        public int TracId { get; set; }
        [DisplayName("カテゴリ")]
        public string Category { get; set; }
        [DisplayName("同時並行")]
        public string Status2 { get; set; }
        [DisplayName("関連リンク")]
        public string Link2 { get; set; }
        [DisplayName("メモ")]
        public string Memo { get; set; }
        [DisplayName("詳細メモ")]
        [DataType(DataType.MultilineText)]
        public string DetailMemo { get; set; }
        [DisplayName("表示")]
        public bool Visible { get; set; }

        [DisplayName("完了フラグ")]
        public bool IsClose { get; set; }

        [DisplayName("プロジェクト")]
        public virtual Project Project { get; set; }
        
        public Ticket()
        {

        }
        
    }

}