using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

namespace Mymy.Models
{
    public class Memo
    {
        public int MemoId { get; set; }
        [DisplayName("プロジェクトID")]
        public int? ProjectId { get; set; }
        [DisplayName("カテゴリ")]
        public string Category { get; set; }
        [DisplayName("タイトル")]
        public string Title { get; set; }
        [DisplayName("メモ")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
    }
}