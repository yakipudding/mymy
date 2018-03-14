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
        [DisplayName("プロジェクトURL")]
        public string ProjectUrl { get; set; }  //http://*****/
        [DisplayName("一覧の条件部分")]
        public string Condition { get; set; }   //owner=****
        [DisplayName("項目部分")]
        public string Column { get; set; }      //col=id&col=...
        [DisplayName("メモ")]
        public string Memo { get; set; }
        [DisplayName("詳細メモ")]
        [DataType(DataType.MultilineText)]
        public string DetailMemo { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<ProjectCustomField> ProjectCustomFields { get; set; }
    }
}