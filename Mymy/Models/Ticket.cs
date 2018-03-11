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
        public string Title { get; set; }
        [DisplayName("作成日")]
        [DisplayFormat(DataFormatString = "{0:yy/MM/dd}")]
        public DateTime Date { get; set; }
        [DisplayName("詳細")]
        public string Description { get; set; }
        [DisplayName("作成者")]
        public string Creator { get; set; }
        
        [DisplayName("TracID")]
        public int TracId { get; set; }
        [DisplayName("カテゴリ")]
        public string Category { get; set; }
        [DisplayName("状態")]
        public string Status { get; set; }
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

        [DisplayName("プロジェクト")]
        public virtual Project Project { get; set; }

        /// <summary>
        /// コンストラクタ　DB用
        /// </summary>
        public Ticket()
        {

        }

        public static int GetTracId(string link)
        {
            return int.Parse(link.Substring(link.LastIndexOf('/') + 1));
        }
    }

}