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

namespace NGKBusi.Areas.SCM.Models
{
    public class Sparepart
    {

    }

    public class SCM_Sparepart_User_Management
    {
        [Key]
        public int ID { get; set; }
        public string userNIK { get; set; }
        public string Role { get; set; }
        public byte IsActive { get; set; }
    }


    public class SCM_Sparepart_Master_List
    {
        [Key]
        public string ITEMID { get; set; }
        public string Image { get; set; }
        public int? Quantity { get; set; }
        public String RackId { get; set; }
        public string RackSequence { get; set; }
        public Int32 MinQty { get; set; }
        public Int32 MaxQty { get; set; }
        public Int32 Lifetime { get; set; }
        public byte IsActive { get; set; }
        public byte IsFastMoving { get; set; }
        public byte IsLocalPart { get; set; }
        public byte IsKanri { get; set; }
    }
    public class SCM_SparepartList
    {
        [Key]
        public string ITEMID { get; set; }
        public string ProductName { get; set; }
        public string ItemDescription { get; set; }
        public string ItemGroup { get; set; }
        public string ProductCategory { get; set; }
        public string ProCateName { get; set; }
        public string SectionType { get; set; }
        public string Section { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
        public Int32 Lifetime { get; set; }
    }

    public class SCM_Sparepart_Rack
    {
        [Key]
        public Int64 RackId { get; set; }
        public string RackName { get; set; }
        public string RackLocation { get; set; }
        public byte IsDelete { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
    }

    public class SCM_Sparepart_Cupboard_Rack
    {
        [Key]
        public string RackID { get; set; }
        public string RackName { get; set; }
        public string cupBoardID { get; set; }
    }


    public class SCM_Sparepart_Request_Temp
    {
        [Key]
        [Column(Order = 0)]
        public string ITEMID { get; set; }
        public int quantity { get; set; }
        [Key]
        [Column(Order = 1)]
        public string userRequest { get; set; }
    }

    public class SCM_Sparepart_Request_Header
    {
        [Key]
        //public int Id { get; set; }
        public string RequestNo { get; set; }
        public string UserRequest { get; set; }
        public string Remark { get; set; }
        public string MaintenanceType { get; set; }
        public string DivisionName { get; set; }
        public string DeptName { get; set; }
        public string SectionName { get; set; }
        public string SubSectionName { get; set; }
        public string CostName { get; set; }
        public byte Status { get; set; }
        public DateTime Create_Time { get; set; }
        public DateTime? PrepareTime { get; set; }
        public DateTime? ReadyTime { get; set; }
        public string ReadyBy { get; set; }
        public DateTime? CloseTime { get; set; }
        public DateTime? CloseTimeDueDate { get; set; }
        public byte? IsCancel { get; set; }
        public byte? IsReturn { get; set; }
        public DateTime? CancelTime { get; set; }
        public string CancelBy { get; set; }
    }

    public class SCM_Sparepart_Request_Detail
    {
        [Key]
        public int Id { get; set; }
        public string ITEMID { get; set; }
        public int Quantity { get; set; }
        public string RequestNo { get; set; }
        public int Qty_Realization { get; set; }
        public int Qty_Retur { get; set; }
        public string ReturNotes { get; set; }
        public byte IsReady { get; set; }
        public byte IsCancel { get; set; }

    }

    public class SCM_Sparepart_Report
    {
        [Key]
        public string ITEMID { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? dateFrom { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? dateTo { get; set; }
        public string ProductName { get; set; }
        public string ItemGroup { get; set; }
        public string RackBoxName { get; set; }
        public int StockAvailable { get; set; }
        public string ProCateName { get; set; }
        public string ProductCategory { get; set; }
        public string RackName { get; set; }
        public string RackId { get; set; }
        public int Quantity { get; set; }
        public string CloseTime { get; set; }
        public string COSTNAME { get; set; }
        public string Status { get; set; }
        public string NameUserRequest { get; set; }
        public string Create_Time { get; set; }
        public string Create_Date { get; set; }
        public string ReadyByName { get; set; }
        public string MaintenanceType { get; set; }
        public int TotalSeconds { get; set; }
        public string Duration { get; set; }
        public int QtyNegative { get; set; }
        public string Site { get; set; }
        public string PrepareDate { get; set; }
    }

    public class SCM_Sparepart_Stock_In_Temp
    {
        [Key]
        public string ITEMID { get; set; }
        public int QtyIn { get; set; }
    }
    public class V_SCM_Sparepart_Request_Detail
    {

        public int Id { get; set; }
        public string ITEMID { get; set; }
        public int Quantity { get; set; }
        public string RequestNo { get; set; }
        public string ProductName { get; set; }
        public string RackName { get; set; }
        public string RackSequence { get; set; }
        public string ItemGroup { get; set; }
        public int Qty_Realization { get; set; }
        public int Qty_Retur { get; set; }
        public string ReturNotes { get; set; }
        public string ProductCategory { get; set; }
        public byte IsReady { get; set; }
        public string RackBoxName { get; set; }
    }

    public class V_SCM_Sparepart_ItemList
    { 
        public int Id { get; set; }
        public string ITEMID { get; set; }
        public int Quantity { get; set; }
        public string RequestNo { get; set; }
        public string ProductName { get; set; }
        public string ItemGroup { get; set; }
        public string RackName { get; set; }
        public string RackSequence { get; set; }
        public int Qty_Realization { get; set; }
        public string ProductCategory { get; set; }
        public string Tools { get; set; }
        public string Qty_Retur { get; set; }
        public string ReturNotes { get; set; }
        public string RackBoxName { get; set; }
    }

    public class SCM_Sparepart_Request_Temp_Views
    {
        [Key]
        public string ITEMID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string UserRequest { get; set; }
        public int Stock { get; set; }
        public string Image { get; set; }
        public int Id { get; set; }
    }

    public class V_SCM_Sparepart_Request
    {
        public string userRequest { get; set; }
        [Key]
        public string RequestNo { get; set; }
        public string Remark { get; set; }
        public string DivisionName { get; set; }
        public string DeptName { get; set; }
        public string SectionName { get; set; }
        public string SubSectionName { get; set; }
        public string CostName { get; set; }
        public byte Status { get; set; }
        public byte? IsReturn { get; set; }
        public DateTime Create_Time { get; set; }
        public string Name { get; set; }
        public DateTime? CloseTimeDueDate { get; set; }
    }

    public class V_SCM_Sparepart_Master_List
    {
        [Key]
        public string ITEMID { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public string ItemDescription { get; set; }
        public string ProductName { get; set; }
        public string ItemGroup { get; set; }
        public string ProductCategory { get; set; }
        public string ProCateName { get; set; }
        public string CostName { get; set; }
        public int MinQty { get; set; }
        public int MaxQty { get; set; }
        public string RackBoxName { get; set; }
        public string RackName { get; set; }
        public string cupBoardName { get; set; }
        public string Section { get; set; }
        public string RackBoxID { get; set; }
        public Int32 Lifetime { get; set; }
        public byte IsActive { get; set; }
        public byte IsKanri { get; set; }
        public byte IsLocalPart { get; set; }
        public byte IsFastMoving { get; set; }
    }

    
    public class Tbl_SCM_Sparepart_Master_List
    {
        [Key]
        public string ITEMID { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public string ItemDescription { get; set; }
        public string ProductName { get; set; }
        public string RackBoxName { get; set; }
        public string RackBoxID { get; set; }
        public string ItemGroup { get; set; }
        public string ProductCategory { get; set; }
        public string ProCateName { get; set; }
        public string RackName { get; set; }
        public string cupBoardName { get; set; }
        public string RackSequence { get; set; }
        public string CostName { get; set; }
        public string Section { get; set; }
        public int MinQty { get; set; }
        public int MaxQty { get; set; }
        public string EditButton { get; set; }
        public string Lifetime { get; set; }
        public string IsActive { get; set; }
        public string IsKanri { get; set; }
        public string IsLocalPart { get; set; }
        public string IsFastMoving { get; set; }
    }
    public class Tbl_V_SCM_Sparepart_Request
    {
        public string RequestNo { get; set; }
        public string RequestBy { get; set; }
        public string Remark { get; set;  }
        public string Section { get; set; }
        public string RequestTime { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
    }
    public class SCM_Sparepart_Product_Received
    {
        [Key]
        public int ID { get; set; }
        public string ProductReceipt { get; set; }
        public string Site { get; set; }
        public string ItemGroup { get; set; }
        public string ITEMID { get; set; }
        public int QuantityReceived { get; set; }
        public DateTime? DateReceived { get; set; }
        public string ReceivedBy { get; set; }
    }
    public class SparepartConnection : DbContext
    {
        public DbSet<V_Users_Active> V_Users_Active { get; set; }
        public DbSet<SCM_Sparepart_User_Management> SCM_Sparepart_User_Management { get; set; }
        public DbSet<SCM_Sparepart_Master_List> SCM_Sparepart_Master_List { get; set; }
        public DbSet<SCM_SparepartList> SCM_SparepartList { get; set; }
        public DbSet<SCM_Sparepart_Rack> SCM_Sparepart_Rack { get; set; }
        public DbSet<SCM_Sparepart_Cupboard_Rack> SCM_Sparepart_Cupboard_Rack { get; set; }
        public DbSet<SCM_Sparepart_Request_Temp> SCM_Sparepart_Request_Temp { get; set; }
        public DbSet<SCM_Sparepart_Request_Temp_Views> SCM_Sparepart_Request_Temp_Views { get; set; }
        public DbSet<SCM_Sparepart_Request_Header> SCM_Sparepart_Request_Header { get; set; }
        //public DbSet<SCM_Sparepart_Report> SCM_Sparepart_Report { get; set; }
        public DbSet<SCM_Sparepart_Stock_In_Temp> SCM_Sparepart_Stock_In_Temp { get; set; }
        public DbSet<V_SCM_Sparepart_Request_Detail> V_SCM_Sparepart_Request_Detail { get; set; }
        public DbSet<V_SCM_Sparepart_ItemList> V_SCM_Sparepart_ItemList { get; set; }
        public DbSet<SCM_Sparepart_Request_Detail> SCM_Sparepart_Request_Detail { get; set; }
        public DbSet<V_SCM_Sparepart_Request> V_SCM_Sparepart_Request { get; set; }
        public DbSet<V_SCM_Sparepart_Master_List> V_SCM_Sparepart_Master_List { get; set; }
        public DbSet<SCM_Sparepart_Product_Received> SCM_Sparepart_Product_Received { get; set; }
        
        public SparepartConnection()
        {
            this.Database.Connection.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
    }
}