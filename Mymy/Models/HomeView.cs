using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mymy.Models
{
    [NotMapped]
    public class HomeView
    {
        public List<Setting> Settings { get; set; }
        public List<Project> Projects { get; set; }
    }
}