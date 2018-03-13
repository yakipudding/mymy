using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mymy.Models
{
    public class ProjectCustomField
    {
        [DisplayName("ID")]
        public int ProjectCustomFieldId { get; set; }
        [DisplayName("プロジェクト")]
        public virtual Project Project { get; set; }
        
        [DisplayName("項目")]
        public string Field { get; set; }
        [DisplayName("項目日本語名")]
        public string FieldJapaneseName { get; set; }
        [DisplayName("表示")]
        public bool Visible { get; set; }
    }

}