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
    public class OldTicketsView
    {
        public List<Ticket> Tickets { get; set; }
        public Project Project { get; set; }

        [DisplayName("カテゴリ")]
        public string Category {get; set; }
        [DisplayName("タイトル")]
        public string Summary { get; set; }
        [DisplayName("表示条件")]
        public DisplayEnum Display { get; set; }

        public enum DisplayEnum
        {
            [Display(Name = "すべて")]
            All,
            [Display(Name = "表示のみ")]
            VisibleOnly,
            [Display(Name = "非表示のみ")]
            InVisibleOnly
        }
    }
}