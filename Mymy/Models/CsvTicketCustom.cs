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
    public class CsvTicketCustom
    {
        [DisplayName("プロジェクトID")]
        public int ProjectId { get; set; }
        [DisplayName("チケットID")]
        public int TicketId { get; set; }

        [DisplayName("ID")]
        public int ProjectCustomFieldId { get; set; }
        [DisplayName("項目名")]
        public string Field { get; set; }
        [DisplayName("項目日本語名")]
        public string FieldJapaneseName { get; set; }
        [DisplayName("項目値")]
        public string FieldValue { get; set; }

    }

}