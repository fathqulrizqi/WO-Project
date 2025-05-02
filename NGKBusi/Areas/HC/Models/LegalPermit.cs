using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NGKBusi.Areas.HC.Models
{
    public class LegalPermit
    {
    }

    public class HC_LegalPermit_Flow
    {
        [Key]
        public int ID { get; set; }
        public string FlowName { get; set; }
        public string IsActive { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
    }
    public class HC_LegalPermit_Flow_Detail
    {
        [Key]
        public int ID { get; set; }
        public string Flow_ID { get; set; }
        public string StepName { get; set; }
        public int StepNumber { get; set; }
        public string Description { get; set; }
        public string PIC { get; set; }
        public int Estimation_Time { get; set; }
        public string Requirement_Document { get; set; }
        public byte IsDelete { get; set; }
    }

    public class HC_LegalPermit_Request
    {
        public int ID { get; set; }
        public string RefQuotationNo { get; set; }
        public string SupplierName { get; set; }
        public string PresidentDirector { get; set; }
        public string Address { get; set; }
        public string TelpNo { get; set; }
        public string FaxNo { get; set; }
        public string ProjectName { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public string TermPayment { get; set; }
        public int LeadTime { get; set; }
        public int Penalty { get; set; }
        public string BankAccountDetail { get; set; }
        public string Warranty { get; set; }
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanEndDate { get; set; }
        public string LegalDocument { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class HC_LegalPermit_Request_BreakdownCost
    {
        [Key]
        public int ID { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int ReqID { get; set; }
    }
    public class HC_LegalPermit_Request_Progress
    {
        [Key]
        public int ID { get; set; }
        public int ReqID { get; set; }
        public int StepID { get; set;  }
        public int StepNumber { get; set; }
        public string StepName { get; set; }
        public string PIC { get; set; }
        public int Estimation_Time { get; set; }
        public byte Status { get; set; }
    }
    public class HC_LegalPermit_Recap_Agreement
    {
        [Key]
        public int ID { get; set; }
        public string AgreementName { get; set; }
        public string SecondParty { get; set; }
        public string AgreementNo { get; set; }
        public DateTime PeriodeStart { get; set; }
        public DateTime PeriodeEnd { get; set; }
        public string Attachment { get; set; }
        public string AgreementType { get; set; }
        public string Note { get; set; }
        public int ReminderID { get; set; }
        public int? PrevAgreementID { get; set; }
        public string PrevAgreementNo { get; set; }
        public byte IsRenewal { get; set; }
        public int? RenewalRefID { get; set; }
    }
    public class HC_LegalPermit_Recap_Permit
    {
        [Key]
        public int ID { get; set; }
        public string Permit { get; set; }
        public string Number { get; set; }
        public string SectionHandling { get; set; }
        public string Goverment { get; set; }
        public DateTime? Expired { get; set; }
        public string Attachment { get; set; }
        public string PIC { get; set; }
        public string Status { get; set; }   
        public int ReminderID { get; set; }
        public int? PrevPermitID { get; set; }
        public string PrevPermitNo { get; set; }
        public byte IsRenewal { get; set; }
        public int? RenewalRefID { get; set; }
    }
    public class HC_LegalPermit_Share_User
    {
        [Key]
        public int ID { get; set; }
        public string NIK { get; set; }
        public byte IsDelete { get; set; }
        public string CreateBy { get; set; }
        public int Legal_ID { get; set; }
        public string LegalType { get; set; }
        public DateTime CreateTime { get; set; }

    }
    public class Tbl_HC_LegalPermit_Request
    {
        public string RequestNo { get; set; }
        public string RequestDate { get; set; }
        public string RefQuotationNo { get; set; }
        public string SupplierName { get; set; }
        public string PresidentDirector { get; set; }
        public string Address { get; set; }
        public string TelpNo { get; set; }
        public string FaxNo { get; set; }
        public string ProjectName { get; set; }
        public decimal Price { get; set; }
        public string Discount { get; set; }
        public string FinalPrice { get; set; }
        public string TermPayment { get; set; }
        public string LeadTime { get; set; }
        public string Penalty { get; set; }
        public string BankAccountDetail { get; set; }
        public string Warranty { get; set; }
        public string PlanStartDate { get; set; }
        public string PlanEndDate { get; set; }
        public string BtnEdit { get; set; }
    }

    public class Tbl_HC_LegalPermit_Flow_Detail
    {
        public int No { get; set; }
        public string PIC { get; set; }
        public string StepName { get; set; }
        public string StepNumber { get; set; }
        public string Description { get; set; }
        public string EstimationTime { get; set; }
        public string Button { get; set; }
        public string Requirement_Document { get; set; }
    }
    public class Tbl_HC_LegalPermit_Recap_Agreement
    {
        public string ID { get; set; }
        public int No { get; set; }
        public string Name { get; set; }
        public string SecondParty { get; set; }
        public string AgreementNo { get; set; }
        public string PeriodeStart { get; set; }
        public string PeriodeEnd { get; set; }
        public string Attachment { get; set; }
        public string AgreementType { get; set; }
        public string Note { get; set; }
        public string btnAlert { get; set; }
    }
    public class Tbl_HC_LegalPermit_Recap_Permit
    {
        public int No { get; set; }
        public string ID { get; set; }
        public string Permit { get; set; }
        public string Number { get; set; }
        public string SectionHandling { get; set; }
        public string Goverment { get; set; }
        public string Expired { get; set; }
        public string PIC { get; set; }
        public string Attachment { get; set; }
        public string Status { get; set; }
        public string btnAlert { get; set; }
    }
    public class Tbl_HC_LegalPermit_Share_User
    {
        public int ID { get; set; }
        public int No { get; set; }
        public string NIK { get; set; }
        public string Name { get; set; }
        public string Section { get; set; }
        public byte IsDelete { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public string Action { get; set; }
    }
    public class LegalPermitConnection : DbContext
    {
        public DbSet<HC_LegalPermit_Flow> HC_LegalPermit_Flow { get; set; }
        public DbSet<HC_LegalPermit_Flow_Detail> HC_LegalPermit_Flow_Detail { get; set; }
        public DbSet<HC_LegalPermit_Request> HC_LegalPermit_Request { get; set; }
        public DbSet<HC_LegalPermit_Request_BreakdownCost> HC_LegalPermit_Request_BreakdownCost { get; set; }
        public DbSet<HC_LegalPermit_Request_Progress> HC_LegalPermit_Request_Progress { get; set; }
        public DbSet<HC_LegalPermit_Recap_Agreement> HC_LegalPermit_Recap_Agreement { get; set; }
        public DbSet<HC_LegalPermit_Recap_Permit> HC_LegalPermit_Recap_Permit { get; set; }
        public IDbSet<HC_LegalPermit_Share_User> HC_LegalPermit_Share_User { get; set; }
        public LegalPermitConnection()
        {
            this.Database.Connection.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
    }
}