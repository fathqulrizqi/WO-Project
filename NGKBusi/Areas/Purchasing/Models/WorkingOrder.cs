using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;


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

    public class WOConnection : DbContext
    {
        public DbSet<Purchasing_WorkingOrder_List> Purchasing_WorkingOrder_List { get; set; }
        public WOConnection()
        {
            this.Database.Connection.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
    }
}