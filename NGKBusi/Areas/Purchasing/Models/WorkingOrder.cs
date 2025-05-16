using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using NGKBusi.Areas.WebService.Models;


namespace NGKBusi.Areas.Purchasing.Models
{
    public class WorkingOrder
    {
    }

    public class Purchasing_WorkingOrder_List
    {
        [Key]
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public string Vendor { get; set; }
        public string VendorName { get; set; }
        public string Subject { get; set; }
        public string NIK { get; set; }
        public string NIKName { get; set; }
        public DateTime Timestamps { get; set; }
    }

    public class Purchasing_WorkingOrder_Letter
    {
        [Key]
        public int ID { get; set; }
        public int IDList { get; set; }
        public string Number { get; set; }
        public string Vendor { get; set; }
        public string Attn { get; set; } = string.Empty;
        public string Ref { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal? Total { get; set; } 
        public string PaymentTerm { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string BudgetNo { get; set; }
        public string BudgetDesc { get; set; }
        public string Remark { get; set; } = string.Empty;
        public DateTime Timestamps { get; set; }
    }

    public class WOConnection : DbContext
    {
        public DbSet<Purchasing_WorkingOrder_List> Purchasing_WorkingOrder_List { get; set; }
        public DbSet<Purchasing_WorkingOrder_Letter> Purchasing_WorkingOrder_Letter { get; set; }
        public WOConnection()
        {
            this.Database.Connection.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
    }
}