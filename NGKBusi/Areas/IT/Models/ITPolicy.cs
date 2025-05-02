﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
using System.Security.Claims;
using System.Web.Mvc;

namespace NGKBusi.Areas.IT.Models
{
    public class ITPolicy
    {
    }

    public class IT_ITPolicy_Document
    {
        public int ID { get; set; }
        public string Year { get; set; }
        public string DocumentName { get; set; }
    }

    public class IT_ISMS_Document
    {
        public int ID { get; set; }
        public string Year { get; set; }
        public string DocumentName { get; set; }
    }

    public class ITPolicyConnection : DbContext
    {
        public DbSet<IT_ITPolicy_Document> IT_ITPolicy_Document { get; set; }
        public DbSet<IT_ISMS_Document> IT_ISMS_Document { get; set; }

        public ITPolicyConnection()
        {
            this.Database.Connection.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
    }
}