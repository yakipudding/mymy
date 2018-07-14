using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mymy.Models
{
    [NotMapped]
    public class TracTicket
    {
        [DisplayName("プロジェクトID")]
        public int ProjectId { get; set; }
        [DisplayName("チケットID")]
        public int TicketId { get; set; }

        [DisplayName("TracID")]
        public int TracId { get; set; }
        [DisplayName("タイトル")]
        public string Summary { get; set; }
        [DisplayName("作成者")]
        public string Reporter { get; set; }
        [DisplayName("担当者")]
        public string Owner { get; set; }
        [DisplayName("詳細")]
        public string Description { get; set; }
        [DisplayName("分類")]
        public string Type { get; set; }
        [DisplayName("状態")]
        public string Status { get; set; }
        [DisplayName("重要度")]
        public string Proprity { get; set; }
        [DisplayName("製品")]
        public string Component { get; set; }
        [DisplayName("キーワード")]
        public string Keywords { get; set; }
        [DisplayName("cc")]
        public string Cc { get; set; }
        [DisplayName("作成日")]
        [DisplayFormat(DataFormatString = "{0:MM/dd}")]
        public DateTime? CreateDate { get; set; }
        [DisplayName("更新日")]
        [DisplayFormat(DataFormatString = "{0:MM/dd hh:mm}")]
        public DateTime? UpdateDate { get; set; }
        [DisplayName("期日")]
        public string DueClose { get; set; }        

        public List<TracTicketCustom> TracTicketCustoms { get; set; }
    }

}