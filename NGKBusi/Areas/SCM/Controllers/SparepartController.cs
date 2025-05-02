using Microsoft.AspNet.Identity;
using NGKBusi.Models;
using NGK_AX.Models;
using NGKBusi.Areas.SCM.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using NGKBusi.Helpers;
using SCM_D365ImporForm_ProductReceipt = NGKBusi.Areas.SCM.Models.SCM_D365ImporForm_ProductReceipt;
using ClosedXML.Excel;
using NGKBusi.SignalR;
using NGKBusi.Controllers;
using System.Diagnostics;

namespace NGKBusi.Areas.SCM.Controllers
{
    [Authorize(Roles = "AdminSparepart, UserSparepart, Administrator, WarehouseSparepart, GroupLeader, Maintenance")]
    
    //[Authorize(Roles = "863.09.22")]
    public class SparepartController : Controller
    {
        DefaultConnection db = new DefaultConnection();
        SparepartConnection dbsp = new SparepartConnection();
        SparepartReturnConnection dbsr = new SparepartReturnConnection();
        NGK_AXConnection dbax = new NGK_AXConnection();
        NotificationController notif = new NotificationController();

        public string breakDecimal(decimal number)
        {
            string NominalValue = number.ToString("N0");

            return NominalValue;
        }

        private int MakeNegative(int value)
        {
            return value > 0 ? -value : value;
        }
        
        // GET: SCM/Sparepart
        public ActionResult Index()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //string q = "select ITEMID,  ProductName, ItemDescription, ItemGroup, ProductCategory, ProCateName, SectionType, Section from[dbo].[V_AXItemMaster] where ItemGroup like '%Tooling%' OR ItemGroup LIke '%MachineP%'";
            //SqlConnection conn = new SqlConnection(connectionString);
            //SqlCommand cmd = new SqlCommand(q, conn);
            //var Sparepart = new List<SCM_SparepartList>();
            //using (conn)
            //{
            //    conn.Open();
            //    SqlDataReader rdr = cmd.ExecuteReader();
            //    while (rdr.Read())
            //    {
            //        var sp = new SCM_SparepartList();
            //        sp.ITEMID = rdr["ITEMID"].ToString();
            //        sp.ProductName = rdr["ProductName"].ToString();
            //        sp.ItemDescription = rdr["ItemDescription"].ToString();
            //        sp.ItemGroup = rdr["ItemGroup"].ToString();
            //        sp.ProductCategory = rdr["ProductCategory"].ToString();
            //        sp.ProductCategory = rdr["ProductCategory"].ToString();

            //        Sparepart.Add(sp);
            //    }
            //}
            string[] itemGroup = { "Tooling,MachineP" };

            //var spl = db.V_SCM_Sparepart_Master_List.Where(w =>  w.CostName == CurrUser.CostName && w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling").ToList();
            var spl = from sp in dbsp.V_SCM_Sparepart_Master_List select sp;
            //if (CurrUser.RoleName == "UserSparepart")
            //{
            //    string[] costNameSection = { "SP PD - FINAL INSPECTION", "SP PD - ASSEMBLY", "SP PD - AUTO ASSEMBLY", "SP PD - BENDING" };
            //    if (CurrUser.SectionName == "ASSEMBLY SPARKPLUG & BENDING" || CurrUser.SectionName == "PACKAGING & OEM")
            //    {
            //        spl = spl.Where(w => w.IsActive == 1 && costNameSection.Contains(w.CostName));
            //    }
            //    else
            //    {
            //        spl = spl.Where(w => w.IsActive == 1 && w.CostName == CurrUser.CostName);
            //    }
                
            //} else
            //{
                spl = spl.Where(w => w.IsActive == 1);
            //}
            ViewBag.SparepartList = spl.ToList();
            var itemList =  dbsp.V_SCM_Sparepart_Master_List.ToList();
            var sectionList = dbsp.V_SCM_Sparepart_Master_List.OrderBy(o=>o.CostName).Select(p => p.CostName).Distinct();
            var CupboardList = dbsp.SCM_Sparepart_Cupboard_Rack.ToList();
            var alertQtyMin = dbsp.V_SCM_Sparepart_Master_List.Where(w => w.Stock <= w.MinQty && itemGroup.Contains(w.ItemGroup)).ToList();
            var countQtyMin = alertQtyMin.Count();
            ViewBag.alertQtyMin = alertQtyMin;
            ViewBag.countQtyMin = countQtyMin;
            ViewBag.ItemList = spl.ToList();
            ViewBag.SectionList = sectionList;
            ViewBag.CupboardList = CupboardList;
            ViewBag.NavHide = true;
            ViewBag.SessionUser = CurrUser;
            return View();
        }
        [Authorize(Roles = "AdminSparepart, Administrator, UserSparepart, WarehouseSparepart, GroupLeader")]
        [HttpPost]
        public JsonResult GetItemMasterList(string[] SelITEMID, string ItemGroup, string[] COSTNAME, string[] ProductCategory, string[] CupBoardRack, byte Status)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            var sql = from p in dbsp.V_SCM_Sparepart_Master_List select p ;
            if (SelITEMID != null)
            {
                sql = from p in sql
                      where SelITEMID.Contains(p.ITEMID)
                      select p;
            }
            if (ItemGroup != "all")
            {
                sql = from p in sql
                      where ItemGroup.Contains(p.ItemGroup)
                      select p;
            }

            if (COSTNAME != null)
            {
                sql = from p in sql
                      where COSTNAME.Contains(p.CostName)
                      select p;
            }
            else if (COSTNAME == null)
            {

                if (CurrUser.RoleName == "UserSparepart")
                {
                    string[] costNameSection = { "SP PD - FINAL INSPECTION", "SP PD - ASSEMBLY", "SP PD - AUTO ASSEMBLY", "SP PD - BENDING" };
                    if (CurrUser.SectionName == "ASSEMBLY SPARKPLUG & BENDING" || CurrUser.SectionName == "PACKAGING & OEM")
                    {
                        sql = from p in sql
                              where costNameSection.Contains(p.CostName)
                              select p;
                    }
                    else
                    {
                        sql = from p in sql
                              where p.CostName == CurrUser.CostName
                              select p;
                    }

                }
                else
                {

                }
            }
            if (ProductCategory != null)
            {
                sql = from p in sql
                      where ProductCategory.Contains(p.ProCateName)
                      select p;
            }
            byte IsKanri = 2;
            byte IsLocalPart = 2;
            byte IsFastMoving = 2;
            if (Status != 2)
            {
                sql = from p in sql
                      where p.IsActive == Status
                      select p;
            }
            if (IsLocalPart != 2)
            {
                sql = from p in sql where p.IsLocalPart == IsLocalPart select p;
            }
            if (IsFastMoving != 2)
            {
                sql = from p in sql where p.IsFastMoving == IsFastMoving select p;
            }
            if (IsKanri != 2)
            {
                sql = from p in sql where p.IsKanri == IsKanri select p;
            }

            var CountRow = sql.Count();

            List<Tbl_SCM_Sparepart_Master_List> itemList = new List<Tbl_SCM_Sparepart_Master_List>();

            foreach (var Item in sql)
            {
                var Tools = "";
                var urlContent = Url.Content("~/Files/SCM/Sparepart/Images/" + Item.Image);
                var UrlAction = Url.Action("EditMasterItem", "Sparepart", new { area = "SCM", ITEMID = Item.ITEMID });

                Tools = "<a href=\"#\" class=\"imageItem\"><img id=\"imageresource\" src=\"" + urlContent + "\" + @tbl.Image)\" height=\"70px\" data-toggle=\"modal\" data-target=\"#exampleModal\" /></a>";

                var editButton = "";

                if (User.IsInRole("AdminSparepart") || User.IsInRole("Administrator") || User.IsInRole("WarehouseSparepart"))
                {
                    editButton = "<a href=\"" + UrlAction + "\" title=\"edit item\" class=\"btn btn-warning\"><i class=\"fa fa-edit\"></i></a>";
                }
                else
                {
                    editButton = "";
                }

                var activation = "";
                if (Item.IsActive == 1)
                {
                    activation = "<span class=\"badge badge-success\">Active</span>";
                }
                else
                {
                    activation = "<span class=\"badge badge-danger\">Not Active</span>";
                }

                // jika ada filter untuk rack name
                if(CupBoardRack != null)
                {
                    // cari yang sesuai filter dari form CupBoardRack
                    if(CupBoardRack.Contains(Item.RackName))
                    {
                        itemList.Add(
                        new Tbl_SCM_Sparepart_Master_List
                        {
                            ITEMID = Item.ITEMID,
                            ProductName = Item.ProductName,
                            ItemGroup = Item.ItemGroup,
                            ProCateName = Item.ProCateName,
                            CostName = Item.CostName,
                            RackName = Item.RackName,
                            cupBoardName = Item.cupBoardName,
                            Stock = Item.Stock,
                            Section = Item.Section,
                            Image = Tools,
                            Lifetime = breakDecimal(Item.Lifetime),
                            IsActive = activation,
                            IsKanri = Item.IsKanri == 1 ? "Kanri" : "Non Kanri",
                            IsLocalPart = Item.IsLocalPart == 1 ? "Local" : "Import",
                            IsFastMoving = Item.IsFastMoving == 1 ? "Fast Moving" : "Slow Moving",
                            EditButton = editButton
                        });
                    }
                } 
                else // jika tidak ada filter cupBoardRack
                {
                    itemList.Add(
                    new Tbl_SCM_Sparepart_Master_List
                    {
                        ITEMID = Item.ITEMID,
                        ProductName = Item.ProductName,
                        ItemGroup = Item.ItemGroup,
                        ProCateName = Item.ProCateName,
                        CostName = Item.CostName,
                        RackName = Item.RackName,
                        cupBoardName = Item.cupBoardName,
                        Stock = Item.Stock,
                        Section = Item.Section,
                        Image = Tools,
                        Lifetime = breakDecimal(Item.Lifetime),
                        IsActive = activation,
                        EditButton = editButton,
                        IsKanri = Item.IsKanri == 1 ? "Kanri" : "Non Kanri",
                        IsLocalPart = Item.IsLocalPart == 1 ? "Local" : "Import",
                        IsFastMoving = Item.IsFastMoving == 1 ? "Fast Moving" : "Slow Moving",
                    });
                }
                
            }
            var jsonResult = Json(new { rows = itemList, totalNotFiltered = CountRow, total = CountRow }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

            //var qITEMID = "";

            //// definisikan value dari form filter
            ////var ITEMID = "all";
            ////var ProductCategory = "all";
            ////var COSTNAME = "all";
            ////var CupBoardRack = "all";
            ////ITEMID = Request.Form.Get("ITEMID");
            ////var ItemGroup = Request.Form.Get("ItemGroup");
            ////var Status = Request.Form.Get("Status");
            ////ProductCategory = Request.Form.Get("ProductCategory");
            ////COSTNAME = Request.Form.Get("COSTNAME");
            ////CupBoardRack = Request.Form.Get("CupBoardRack");

            ////if (ITEMID == null)
            ////{
            ////    qITEMID = "all";
            ////}
            ////else
            ////{
            ////    qITEMID = string.Join(",", ITEMID);
            ////}

            ////if (ProductCategory == null)
            ////{
            ////    ProductCategory = "all";
            ////}
            ////if (COSTNAME == null)
            ////{
            ////    COSTNAME = "all";
            ////}
            ////if (CupBoardRack == null)
            ////{
            ////    CupBoardRack = "all";
            ////}
            ////string[] itemGroup = { "MachineP", "Tooling" };

            ////string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            ////SqlConnection con = new SqlConnection(cnnString);

            ////List<Tbl_SCM_Sparepart_Master_List> actions = new List<Tbl_SCM_Sparepart_Master_List>();

            ////SqlCommand cmd = new SqlCommand("sp_SCM_Sparepart_GetItemMaster", con);
            ////cmd.CommandType = CommandType.StoredProcedure;

            ////cmd.Parameters.AddWithValue("@qITEMID", ITEMID);
            //////cmd.Parameters.AddWithValue("@ItemGroup", ItemGroup);
            //////cmd.Parameters.AddWithValue("@Status", Status);
            //////cmd.Parameters.AddWithValue("@ProductCategory", ProductCategory);
            //////cmd.Parameters.AddWithValue("@COSTNAME", COSTNAME);
            //////cmd.Parameters.AddWithValue("@CupBoardRack", CupBoardRack);

            ////SqlDataAdapter sd = new SqlDataAdapter(cmd);
            ////DataTable dt = new DataTable();

            ////con.Open();
            ////sd.Fill(dt);
            ////con.Close();

            ////int countRow = 0;
            ////foreach (DataRow dr in dt.Rows)
            ////{
            ////    countRow++;
            ////    actions.Add(
            ////        new Tbl_SCM_Sparepart_Master_List
            ////        {
            ////            ITEMID = Convert.ToString(dr["ITEMID"]),

            ////        });
            ////}


            //// end query report 

            //string[] itemGroup = { "Tooling", "MachineP" };
            ////var spl = db.V_SCM_Sparepart_Master_List.Where(w =>  w.CostName == CurrUser.CostName && w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling").ToList();
            //var spl = from sp in dbsp.V_SCM_Sparepart_Master_List select sp;

            //if (SelITEMID != null)
            //{
            //    spl.Where(w => w.ITEMID == "QIDMP-0030259");
            //}
            //spl.ToList();

            //var CountRow = spl.Count();

            //List<Tbl_SCM_Sparepart_Master_List> actions = new List<Tbl_SCM_Sparepart_Master_List>();

            //foreach (var Item in spl)
            //{
            //    var Tools = "";
            //    var urlContent = Url.Content("~/Files/SCM/Sparepart/Images/" + Item.Image);
            //    var UrlAction = Url.Action("EditMasterItem", "Sparepart", new { area = "SCM", ITEMID = Item.ITEMID });

            //    Tools = "<a href=\"#\" class=\"imageItem\"><img id=\"imageresource\" src=\"" + urlContent + "\" + @tbl.Image)\" height=\"70px\" data-toggle=\"modal\" data-target=\"#exampleModal\" /></a>";

            //    var editButton = "";

            //    if (User.IsInRole("AdminSparepart") || User.IsInRole("Administrator") || User.IsInRole("WarehouseSparepart"))
            //    {
            //        editButton = "<a href=\"" + UrlAction + "\" title=\"edit item\" class=\"btn btn-warning\"><i class=\"fa fa-edit\"></i></a>";
            //    }
            //    else
            //    {
            //        editButton = "";
            //    }

            //    var activation = "";
            //    if (Item.IsActive == 1)
            //    {
            //        activation = "<span class=\"badge badge-success\">Active</span>";
            //    }
            //    else
            //    {
            //        activation = "<span class=\"badge badge-danger\">Not Active</span>";
            //    }

            //    actions.Add(
            //        new Tbl_SCM_Sparepart_Master_List
            //        {
            //            ITEMID = Item.ITEMID,
            //            ProductName = Item.ProductName,
            //            ItemGroup = Item.ItemGroup,
            //            ProCateName = Item.ProCateName,
            //            CostName = Item.CostName,
            //            RackName = Item.RackName,
            //            cupBoardName = Item.cupBoardName,
            //            Stock = Item.Stock,
            //            Section = Item.Section,
            //            Image = Tools,
            //            Lifetime = breakDecimal(Item.Lifetime),
            //            IsActive = activation,
            //            EditButton = editButton
            //        });
            //}

            //var jsonResult = Json(new { rows = actions, totalNotFiltered = CountRow, total = CountRow }, JsonRequestBehavior.AllowGet);
            ////var jsonResult = Json(new { rows = qITEMID }, JsonRequestBehavior.AllowGet);
            //jsonResult.MaxJsonLength = int.MaxValue;
            //return jsonResult;
        }

        [HttpGet]
        [Authorize(Roles = "AdminSparepart, Administrator, WarehouseSparepart")]
        public ActionResult EditMasterItem(string ITEMID)
        {
            var spl = dbsp.V_SCM_Sparepart_Master_List.Where(w => w.ITEMID == ITEMID).FirstOrDefault();
            ViewBag.SparepartDetail = spl;

            var rack = dbsp.SCM_Sparepart_Rack.Where(w => w.IsDelete == 0).ToList();
            ViewBag.Rack = rack;

            return View();
        }

        [Authorize(Roles = "AdminSparepart, Administrator, WarehouseSparepart")]
        [HttpPost]
        public ActionResult EditMasterItem(HttpPostedFileBase ImageFile, String RackId, string RackSequence, string ITEMID, Int32 MinQty, Int32 MaxQty, Int32 Lifetime, byte IsKanri, byte IsFastMoving, byte IsLocalPart)
        {
            HttpPostedFileBase file = Request.Files["ImageFile"];

            Guid guid = Guid.NewGuid();
            string newfileName = guid.ToString();

            string fileextention = Path.GetExtension(file.FileName);

            string fileName = ITEMID + fileextention;

            //string uploadpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

            //var stream = new FileStream(uploadpath, FileMode.Create);

            //file.CopyToAsync(stream);

            //return Json(new { status = ImageFile });
            if (ImageFile != null)
            {

                var query = dbsp.SCM_Sparepart_Master_List.Where(w => w.ITEMID == ITEMID);
                var spl = query.FirstOrDefault();
                var queryCount = dbsp.SCM_Sparepart_Master_List.Where(w => w.ITEMID == ITEMID);
                int count = queryCount.Count();

                string oldpath = Request.MapPath("~/Files/SCM/Sparepart/Images/" + spl.Image);

                if (System.IO.File.Exists(oldpath))
                {
                    System.IO.File.Delete(oldpath);
                }

                string pic = System.IO.Path.GetFileName(file.FileName);

                string path = Path.Combine(Server.MapPath("~/Files/SCM/Sparepart/Images"), fileName);
                file.SaveAs(path);

                spl.RackId = RackId;
                spl.RackSequence = RackSequence;
                spl.Image = fileName;
                spl.MinQty = MinQty;
                spl.MaxQty = MaxQty;
                spl.Lifetime = Lifetime;
                spl.IsFastMoving = IsFastMoving;
                spl.IsKanri = IsKanri;
                spl.IsLocalPart = IsLocalPart;

                if (count == 0)
                {
                    spl.ITEMID = ITEMID;
                    dbsp.SCM_Sparepart_Master_List.Add(spl);
                }

                var update = dbsp.SaveChanges();
                if (update > 0)
                {
                    return Json(new { status = "1", path = path, filename = pic, msg = "Update Success" });
                }
                else
                {
                    return Json(new { status = "0", path = path, filename = pic, msg = "update failed" });
                }
            }
            else
            {
                var query = dbsp.SCM_Sparepart_Master_List.Where(w => w.ITEMID == ITEMID);
                var spl = query.FirstOrDefault();
                var queryCount = dbsp.SCM_Sparepart_Master_List.Where(w => w.ITEMID == ITEMID);
                int count = queryCount.Count();

                spl.RackId = RackId;
                spl.RackSequence = RackSequence;
                spl.MinQty = MinQty;
                spl.MaxQty = MaxQty;
                spl.Lifetime = Lifetime;
                spl.IsFastMoving = IsFastMoving;
                spl.IsKanri = IsKanri;
                spl.IsLocalPart = IsLocalPart;

                if (count == 0)
                {
                    spl.ITEMID = ITEMID;
                    dbsp.SCM_Sparepart_Master_List.Add(spl);
                }

                var update = dbsp.SaveChanges();
                if (update == 1)
                {
                    return Json(new { status = "1", msg = "Update Success" });
                }
                else
                {
                    return Json(new { status = "0", msg = "update Failed" });
                }
            }
        }

        [HttpPost]
        public ActionResult PrintQR(string ITEMIDArr)
        {

            String[] result = ITEMIDArr.Split(',');
            var query = dbsp.V_SCM_Sparepart_Master_List.Where(w => result.Contains(w.ITEMID)).ToList();
            ViewBag.ITEMID = result;
            ViewBag.ItemList = query;
            return View();

        }

        [HttpPost]
        public ActionResult UpdateActivation(string ITEMID, string action)
        {
            var item = dbsp.SCM_Sparepart_Master_List.Where(w => w.ITEMID == ITEMID).FirstOrDefault();

            if (action == "Deactivate")
            {
                item.IsActive = 0;
            } else
            {
                int CountItem = dbsp.SCM_Sparepart_Master_List.Where(w => w.ITEMID == ITEMID).Count();
                if (CountItem > 0 )
                {
                    item.IsActive = 1;
                } else
                {
                    SCM_Sparepart_Master_List sparepart = new SCM_Sparepart_Master_List();

                    sparepart.ITEMID = ITEMID;
                    sparepart.Lifetime = 0;
                    sparepart.MaxQty = 0;
                    sparepart.MinQty = 0;
                    sparepart.Image = "default.png";
                    sparepart.IsActive = 1;

                    dbsp.SCM_Sparepart_Master_List.Add(sparepart);
                }
                
            }
            int i = dbsp.SaveChanges();
            if (i > 0)
            {
                return Json(new
                {
                    status = 1,
                    msg = "Update Success"
                });
            } else
            {
                return Json(new
                {
                    status = 0,
                    msg = "Update Failed"
                });
            }
           

        }

        [HttpGet]
        public ActionResult PrintQR()
        {
            return View();

        }
        public class ItemDetailDTO
        {
            public string RequestNo { get; set; }
            public DateTime Create_Time { get; set; }
            public byte Status { get; set; }
            public string Remark { get; set; }
            public string MaintenanceType { get; set; }
            public string CostName { get; set; }
            public string UserRequest { get; set; }
            public DateTime? CloseTimeDueDate { get; set; }

            // Other field you may need from the Product entity
        }
        public ActionResult SendNotification()
        {
            // Ambil user ID dari Claims
            var userId = ((ClaimsIdentity)User.Identity).GetUserId();

            // Kirimkan notifikasi
            List<string> userList = new List<string>();
            var userData = dbsp.SCM_Sparepart_User_Management.Where(w => w.Role == "Admin" || w.Role == "Developer").ToList();
            foreach (var item in userData)
            {
                userList.Add(item.userNIK);
            }
            //List<string> users = new List<string> { "863.09.22", "" };
            notif.PushNotification("test Notification", userList);

            return Content("Notifikasi dikirim ke user dengan ID: " + userId);
        }
        [HttpPost]
        public ActionResult ItemListRequest(string RequestNo, string Status)
        {
            var spl = dbsp.V_SCM_Sparepart_Request_Detail.Where(w => w.RequestNo == RequestNo).ToList();
            var CountRow = dbsp.V_SCM_Sparepart_Request_Detail.Count(w => w.RequestNo == RequestNo);

            List<V_SCM_Sparepart_ItemList> actions = new List<V_SCM_Sparepart_ItemList>();

            var countNotReady = dbsp.V_SCM_Sparepart_Request_Detail.Count(w => w.RequestNo == RequestNo && w.IsReady == 0);

            foreach (var Item in spl)
            {
                var Tools = "";
                if (Status == "1")
                {
                    Tools = "<a href=\"#EditQuantityModal\" data-toggle=\"modal\" title=\"Edit\" id=\"EditQtyItem\" data-itemid=\"" + Item.ITEMID + "\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-warning EditQtyItem\"><i class=\"fa fa-pencil\"></i></a> <a href=\"#\" title=\"Delete\" id=\"DeleteItem\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-danger DeleteItem\"><i class=\"fa fa-trash\"></i></a>";
                }
                else
                {
                    if (Item.IsReady == 0 && (User.IsInRole("Administrator") || User.IsInRole("WarehouseSparepart")))
                    {
                        Tools = "<a href=\"#\" title=\"ready\" id=\"procItem\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-primary procItem\"><i class=\"fa fa-check\"></i></a> <a href=\"#\" title=\"Delete\" id=\"DeleteItem\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-danger DeleteItem\"><i class=\"fa fa-trash\"></i></a>";
                    }
                    else
                    {
                        if (User.IsInRole("Administrator") || User.IsInRole("WarehouseSparepart"))
                        {
                            Tools = "<a href=\"#EditQuantityRealizationModal\" data-toggle=\"modal\" title=\"Decrease Qty\" id=\"editQty\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-warning procItem\"><i class=\"fa fa-edit btnEdit\"></i></a>";
                        }
                    }
                }

                actions.Add(
                    new V_SCM_Sparepart_ItemList
                    {
                        ITEMID = Item.ITEMID,
                        ProductName = Item.ProductName,
                        Quantity = Item.Quantity,
                        Qty_Realization = Item.Qty_Realization,
                        RackName = Item.RackName,
                        RackSequence = Item.RackSequence,
                        RackBoxName = Item.RackBoxName,
                        Tools = Tools
                    });
            }

            return Json(new
            {
                rows = actions,
                totalNotFiltered = CountRow,
                total = CountRow,
                notReady = countNotReady,
                status = 1
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        // GET: SCM/Sparepart/Details/5
        public ActionResult DetailRequest(string RequestNo)
        {
            //var data = db.SCM_Sparepart_Request_Header.FirstOrDefault(x => x.RequestNo == RequestNo);
            //ViewBag.header = data;

            //List<SCM_Sparepart_Request_Header> sCM_Sparepart_Request_Headers = db.SCM_Sparepart_Request_Header.ToList();
            //List<V_AXItemMaster> v_AXItemMasters = db.V_AXItemMaster.ToList();

            var data = (from c in dbsp.SCM_Sparepart_Request_Header
                        join cn in dbsp.V_Users_Active on c.UserRequest equals cn.NIK
                        where (c.RequestNo == RequestNo)
                        select new ItemDetailDTO { RequestNo = c.RequestNo, Create_Time = c.Create_Time, Status = c.Status, Remark = c.Remark, MaintenanceType = c.MaintenanceType, UserRequest = cn.Name, CloseTimeDueDate = c.CloseTimeDueDate, CostName = c.CostName }).FirstOrDefault();
            ViewBag.header = data;
            ViewBag.RequestNo = RequestNo;
            var countNotReady = dbsp.V_SCM_Sparepart_Request_Detail.Count(w => w.RequestNo == RequestNo && w.IsReady == 0);
            ViewBag.NotReady = countNotReady;
            ViewBag.NavHide = true;
            //var header = new List<SCM_Sparepart_Request_Header>();
            //foreach ( var tdata in results)
            //{
            //    var sp = new SCM_Sparepart_Request_Header();
            //    sp.RequestNo = tdata.RequestNo;

            //    header.Add(sp);
            //}


            //return Json(new
            //{
            //    status = RequestNo,
            //    msg = results,
            //    spl =  spl
            //}, JsonRequestBehavior.AllowGet);

            return View();
        }

        // GET: SCM/Sparepart/Create
        [Authorize(Roles = "AdminSparepart, Administrator, UserSparepart, GroupLeader, Maintenance")]
        public ActionResult CreateRequest()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            var section = db.Users_Section_AX.ToList();

            ViewBag.CurrUser = dbsp.V_Users_Active.Where(w => w.NIK == currUser).First();
            ViewBag.UserList = dbsp.V_Users_Active.Where(w => w.Status == "Permanent").ToList();
            ViewBag.sectionCreator = CurrUser.CostName;
            ViewBag.section = section;
            ViewBag.NavHide = true;
            ViewBag.Role = CurrUser.RoleName;

            string[] itemGroup = { "" };
            string[] costNameSection = { "SP PD - FINAL INSPECTION", "SP PD - ASSEMBLY", "SP PD - AUTO ASSEMBLY", "SP PD - BENDING" };
            var spl = from sp in dbsp.V_SCM_Sparepart_Master_List select sp;
            //if (CurrUser.RoleName == "UserSparepart" && (CurrUser.SectionName != "ASSEMBLY SPARKPLUG & BENDING" || CurrUser.SectionName != "PACKAGING & OEM"))
            //{
            //    if (CurrUser.SectionName == "ASSEMBLY SPARKPLUG & BENDING" || CurrUser.SectionName == "PACKAGING & OEM")
            //    {
            //        spl = spl.Where(w => costNameSection.Contains(w.CostName) && w.IsActive == 1);
            //    } else
            //    {
            //        spl = spl.Where(w => w.CostName == CurrUser.CostName && w.IsActive == 1);
            //    }
                
            //}
            //else
            //{
                spl = spl.Where(w => w.IsActive == 1);
            //}

            ViewBag.SparepartList = spl.ToList() ;

            return View();
        }

        // POST: SCM/Sparepart/Create
        [Authorize(Roles = "AdminSparepart, Administrator,UserSparepart,GroupLeader")]
        [HttpPost]
        public ActionResult CreateRequest(SCM_Sparepart_Request_Header smodel)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = dbsp.V_Users_Active.Where(w => w.NIK == currUser).First();

            string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            SqlConnection con = new SqlConnection(cnnString);

            var spl = dbsp.SCM_Sparepart_Request_Temp.Where(w => w.userRequest == CurrUser.NIK).ToList();
            var stockMin = new ArrayList();
            int s = 0;
            foreach (var dt in spl)
            {
                var itemid = dt.ITEMID;
                var QStock = dbsp.V_SCM_Sparepart_Master_List.Where(w => w.ITEMID == itemid).FirstOrDefault();
                int Stock = QStock.Stock;

                if (Stock < dt.quantity)
                {
                    s++;
                    stockMin.Add(dt.ITEMID);
                }
            }

            if (s == 0)
            {
                SqlCommand cmd = new SqlCommand("sp_SCM_Sparepart_Request_Create", con);
                cmd.CommandType = CommandType.StoredProcedure;

                //cek item temp
                var count = dbsp.SCM_Sparepart_Request_Temp.Count(me => me.userRequest == CurrUser.NIK);
                var remark = "";
                var CostName = "";
                var MaintenanceType = "";

                if (count > 0)
                {
                    if (smodel.Remark != null)
                    {
                        remark = smodel.Remark;
                    }
                    else
                    {
                        remark = "";
                    }
                    if (smodel.CostName != null)
                    {
                        CostName = smodel.CostName;
                    } else
                    {
                        CostName = "default";
                    }
                    if (smodel.MaintenanceType != null)
                    {
                        MaintenanceType = smodel.MaintenanceType;
                    }
                    else
                    {
                        MaintenanceType = "";
                    }

                    cmd.Parameters.AddWithValue("@userRequest", CurrUser.NIK);
                    cmd.Parameters.AddWithValue("@Remark", remark);
                    cmd.Parameters.AddWithValue("@CostName", CostName);
                    cmd.Parameters.AddWithValue("@MaintenanceType", MaintenanceType);

                    SqlParameter outputParam = new SqlParameter("@reqNo", SqlDbType.VarChar, 50)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(outputParam);

                    con.Open();
                    //int i = cmd.ExecuteNonQuery();
                    cmd.ExecuteNonQuery();
                    //con.Close();

                    // Mengambil nilai output dari parameter @OrderId
                    var reqNo = outputParam.Value.ToString().Trim();

                    if (!string.IsNullOrEmpty(reqNo.ToString()))
                    {
                        List<string> userList = new List<string>();
                        var userData = dbsp.SCM_Sparepart_User_Management.Where(w => w.Role == "Admin" || w.Role == "Developer").ToList();
                        foreach(var item in userData)
                        {
                            userList.Add(item.userNIK);
                        }
                        //List<string> users = new List<string> { "863.09.22", "" };
                        notif.PushNotification(reqNo, userList);

                        return Json(new
                        {
                            status = 1,
                            msg = "Request Send",
                            minstock = spl,
                            itemidminus = stockMin,
                            reqNo = reqNo
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            status = 0,
                            msg = "failed",
                            minstock = spl,
                            itemidminus = stockMin
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        status = 0,
                        msg = "Empty Item Request" + stockMin,
                        minstock = s,
                        itemidminus = stockMin
                    });
                }
            }
            else
            {
                return Json(new
                {
                    status = 2,
                    msg = "Plese Check Stock for this Item " + stockMin,
                    minstock = s,
                    itemidminus = stockMin
                });
            }
        }

        [HttpPost]
        public ActionResult AddRequestList(SCM_Sparepart_Request_Temp smodel)
        {
            try
            {
                var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
                var CurrUser = dbsp.V_Users_Active.Where(w => w.NIK == currUser).First();

                var data = dbsp.SCM_Sparepart_Request_Temp.FirstOrDefault(x => x.ITEMID == smodel.ITEMID && x.userRequest == CurrUser.NIK);
                if (data != null)
                {
                    return Json(new
                    {
                        status = "2",
                        msg = "Item Exist, please check your item list"
                    });
                }
                else
                {
                    dbsp.SCM_Sparepart_Request_Temp.Add(smodel);
                    var ins = dbsp.SaveChanges();
                    if (ins == 1)
                    {
                        return Json(new
                        {
                            status = "1",
                            msg = "Item Inserted"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            status = "0",
                            msg = "failed"
                        });
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult RequestList()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            List<SCM_Sparepart_Request_Temp> sCM_Sparepart_Request_Temps = dbsp.SCM_Sparepart_Request_Temp.ToList();
            List<V_SCM_Sparepart_Master_List> V_SCM_Sparepart_Master_List = dbsp.V_SCM_Sparepart_Master_List.ToList();

            var item = (
                from ai in sCM_Sparepart_Request_Temps
                join al in V_SCM_Sparepart_Master_List on ai.ITEMID equals al.ITEMID
                where (ai.userRequest == CurrUser.NIK)
                select new SCM_Sparepart_Request_Temp_Views
                {
                    ITEMID = ai.ITEMID,
                    ProductName = al.ProductName,
                    Quantity = ai.quantity,
                    UserRequest = ai.userRequest
                }).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp_Views.SqlQuery("SELECT a.ITEMID, a.userRequest , a.quantity, b.ProductName from SCM_Sparepart_Request_Temp a LEFT JOIN V_AXItemMaster b ON b.ITEMID = a.ITEMID WHERE a.userRequest = {0}", CurrUser.NIK).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp.Where(w => w.userRequest == CurrUser.NIK).ToList();

            return Json(item, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditQuantityItemOpen(int DetailId)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            List<SCM_Sparepart_Request_Detail> sCM_Sparepart_Request_Detail = dbsp.SCM_Sparepart_Request_Detail.ToList();
            List<V_SCM_Sparepart_Master_List> v_SCM_Sparepart_Master_List = dbsp.V_SCM_Sparepart_Master_List.ToList();

            var item = (
                from ai in sCM_Sparepart_Request_Detail
                join al in v_SCM_Sparepart_Master_List on ai.ITEMID equals al.ITEMID
                where (ai.Id == DetailId)
                select new SCM_Sparepart_Request_Temp_Views
                {
                    ITEMID = ai.ITEMID,
                    ProductName = al.ProductName,
                    Quantity = ai.Quantity,
                    Stock = al.Stock,
                    Id = ai.Id
                }).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp_Views.SqlQuery("SELECT a.ITEMID, a.userRequest , a.quantity, b.ProductName from SCM_Sparepart_Request_Temp a LEFT JOIN V_AXItemMaster b ON b.ITEMID = a.ITEMID WHERE a.userRequest = {0}", CurrUser.NIK).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp.Where(w => w.userRequest == CurrUser.NIK).ToList();

            return Json(item, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditQuantityItemOpen(SCM_Sparepart_Request_Detail smodel)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();


            var data = dbsp.SCM_Sparepart_Request_Detail.FirstOrDefault(x => x.Id == smodel.Id);
            data.Quantity = smodel.Quantity;

            var update = dbsp.SaveChanges();
            if (update == 1)
            {
                return Json(new
                {
                    status = "1",
                    msg = "Update Quntity Success"
                });
            }
            else
            {
                return Json(new
                {
                    status = "0",
                    msg = "failed"
                });
            }

        }
        [HttpGet]
        public ActionResult EditQuantityItem(string itemid)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            List<SCM_Sparepart_Request_Temp> sCM_Sparepart_Request_Temps = dbsp.SCM_Sparepart_Request_Temp.ToList();
            List<V_SCM_Sparepart_Master_List> v_SCM_Sparepart_Master_List = dbsp.V_SCM_Sparepart_Master_List.ToList();

            var item = (
                from ai in sCM_Sparepart_Request_Temps
                join al in v_SCM_Sparepart_Master_List on ai.ITEMID equals al.ITEMID
                where (ai.userRequest == CurrUser.NIK && ai.ITEMID == itemid)
                select new SCM_Sparepart_Request_Temp_Views
                {
                    ITEMID = ai.ITEMID,
                    ProductName = al.ProductName,
                    Quantity = ai.quantity,
                    UserRequest = ai.userRequest,
                    Stock = al.Stock
                }).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp_Views.SqlQuery("SELECT a.ITEMID, a.userRequest , a.quantity, b.ProductName from SCM_Sparepart_Request_Temp a LEFT JOIN V_AXItemMaster b ON b.ITEMID = a.ITEMID WHERE a.userRequest = {0}", CurrUser.NIK).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp.Where(w => w.userRequest == CurrUser.NIK).ToList();

            return Json(item, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditQuantityItem(SCM_Sparepart_Request_Temp smodel)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();


            var data = dbsp.SCM_Sparepart_Request_Temp.FirstOrDefault(x => x.ITEMID == smodel.ITEMID && x.userRequest == CurrUser.NIK);
            data.quantity = smodel.quantity;

            var update = dbsp.SaveChanges();
            if (update == 1)
            {
                return Json(new
                {
                    status = "1",
                    msg = "Update Quntity Success"
                });
            }
            else
            {
                return Json(new
                {
                    status = "0",
                    msg = "failed"
                });
            }

        }

        [HttpGet]
        public ActionResult EditQuantityRealization(byte DetailId)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            List<SCM_Sparepart_Request_Detail> sCM_Sparepart_Request_Details = dbsp.SCM_Sparepart_Request_Detail.ToList();
            List<V_SCM_Sparepart_Master_List> v_SCM_Sparepart_Master_List = dbsp.V_SCM_Sparepart_Master_List.ToList();

            var item = (
                from ai in sCM_Sparepart_Request_Details
                join al in v_SCM_Sparepart_Master_List on ai.ITEMID equals al.ITEMID
                where (ai.Id == DetailId)
                select new V_SCM_Sparepart_ItemList
                {
                    Id = ai.Id,
                    ITEMID = ai.ITEMID,
                    ProductName = al.ProductName,
                    Quantity = ai.Quantity,
                    Qty_Realization = ai.Qty_Realization

                }).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp_Views.SqlQuery("SELECT a.ITEMID, a.userRequest , a.quantity, b.ProductName from SCM_Sparepart_Request_Temp a LEFT JOIN V_AXItemMaster b ON b.ITEMID = a.ITEMID WHERE a.userRequest = {0}", CurrUser.NIK).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp.Where(w => w.userRequest == CurrUser.NIK).ToList();

            return Json(item, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditQuantityRealization(SCM_Sparepart_Request_Detail item)
        {
            var data = dbsp.SCM_Sparepart_Request_Detail.FirstOrDefault(x => x.Id == item.Id);
            data.Qty_Realization = item.Qty_Realization;

            var update = dbsp.SaveChanges();
            if (update == 1)
            {
                return Json(new
                {
                    status = "1",
                    msg = "Update Success"
                });
            }
            else
            {
                return Json(new
                {
                    status = "0",
                    msg = "Item Not Update"
                });
            }
        }
        [HttpPost]
        public ActionResult RemoveList(string itemid)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();

            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            try
            {
                SCM_Sparepart_Request_Temp sCM_Sparepart_Request_Temp = dbsp.SCM_Sparepart_Request_Temp.Find(itemid, CurrUser.NIK);
                dbsp.SCM_Sparepart_Request_Temp.Remove(sCM_Sparepart_Request_Temp);
                var del = dbsp.SaveChanges();
                if (del == 1)
                {
                    return Json(new
                    {
                        status = "1",
                        msg = "Item Deleted"
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = "0",
                        msg = "failed"
                    });
                }
            }
            catch
            {
                return Json(new
                {
                    status = "2",
                    msg = "failed",
                    itemid = itemid,
                    userNIK = CurrUser.NIK
                });
            }

        }

        // GET: SCM/Sparepart/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: SCM/Sparepart/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult RequestData(DateTime dateFrom, DateTime dateTo, Byte status)
        {

            //DateTime parseStartDate = DateTime.Parse(result[0]);
            //

            //DateTime parseEndDate = DateTime.Parse(result[0]);
            //

            var fromDate = Convert.ToDateTime(dateFrom);
            var startDate = fromDate.ToString("yyyy-MM-dd");

            var toDate = Convert.ToDateTime(dateTo);
            var endDate = toDate.ToString("yyyy-MM-dd");

            //var spl = db.V_AXItemMaster.Where(w => w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling").ToList();
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            var spl = from sp in dbsp.V_SCM_Sparepart_Request select sp;
            if (status == 0)
            {
                if (CurrUser.RoleName == "AdminSparepart" || CurrUser.RoleName == "Administrator" || CurrUser.RoleName == "WarehouseSparepart")
                {
                    spl = spl.Where(w => (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate));
                }
                else
                {
                    spl = spl.Where(w => (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate) && w.userRequest == currUser);
                }

            }
            else
            {
                if (CurrUser.RoleName == "AdminSparepart" || CurrUser.RoleName == "Administrator" || CurrUser.RoleName == "WarehouseSparepart")
                {
                    spl = spl.Where(w => w.Status == status && (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate));
                }
                else
                {
                    spl = spl.Where(w => w.Status == status && (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate) && w.userRequest == currUser);
                }
            }
            var result = spl.ToList();


            var CheckedAction = "";

            //ViewBag.dater = spl;
            //return Json(new
            //{
            //    status = status,
            //    from = startDate,
            //    to = endDate,
            //    result = spl
            //});

            Session["startDate"] = startDate;
            Session["endDate"] = endDate;
            Session["status"] = Convert.ToString(status);

            ViewBag.RequestList = result;
            ViewBag.status = status;
            ViewBag.dateStart = startDate;
            ViewBag.dateEnd = endDate;
            ViewBag.autopick = "true";
            ViewBag.CheckedAction = CheckedAction;
            return View();
        }
        public ActionResult RequestData()
        {
            string dateFrom = Session["startDate"] as string;
            string dateTo = Session["endDate"] as string;
            string statusFilter = Session["status"] as string;

            var spl = from sp in dbsp.V_SCM_Sparepart_Request select sp;
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            Console.WriteLine(Session["startDate"]);

            if (!string.IsNullOrEmpty(dateFrom))
            {
               
                var fromDate = Convert.ToDateTime(dateFrom);
                var startDate = fromDate.ToString("yyyy-MM-dd");

                var toDate = Convert.ToDateTime(dateTo);
                var endDate = toDate.ToString("yyyy-MM-dd");

                int status = Convert.ToInt32(statusFilter);
                if (status == 0)
                {
                    if (CurrUser.RoleName == "AdminSparepart" || CurrUser.RoleName == "Administrator" || CurrUser.RoleName == "WarehouseSparepart")
                    {
                        spl = spl.Where(w => (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate));
                    }
                    else
                    {
                        spl = spl.Where(w => (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate) && w.userRequest == currUser);
                    }

                }
                else
                {
                    if (CurrUser.RoleName == "AdminSparepart" || CurrUser.RoleName == "Administrator" || CurrUser.RoleName == "WarehouseSparepart")
                    {
                        spl = spl.Where(w => w.Status == status && (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate));
                    }
                    else
                    {
                        spl = spl.Where(w => w.Status == status && (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate) && w.userRequest == currUser);
                    }
                }


            }

            else
            {
                if (CurrUser.RoleName == "AdminSparepart" || CurrUser.RoleName == "Administrator" || CurrUser.RoleName == "WarehouseSparepart")
                {
                    Console.WriteLine('1');
                    spl = spl.Where(w => w.Status == 1).OrderByDescending(o => o.Create_Time);
                }
                else
                {
                    Console.WriteLine('2');
                    spl = spl.Where(w => w.Status == 1 && w.userRequest == currUser).OrderByDescending(o => o.Create_Time);
                }

            }

            var result = spl.ToList();

            ViewBag.RequestList = result;
            ViewBag.status = 1;
            ViewBag.dateStart = null;
            ViewBag.dateEnd = null;
            ViewBag.autopick = "false";
            ViewBag.StartDate = dateFrom;
            ViewBag.endDate = dateTo;
            ViewBag.statusFilter = statusFilter;

            return View();
        }

        [HttpPost]
        public ActionResult GetRequestData(string dateFrom, string dateTo, int status)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            TempData["statusFilter"] = status;

            //string dateFrom = "2024-11-01 12:00:00";
            //string dateTo = "2024-11-14 12:00:00";
            //string FilterStatus = "0";

            //string dateFrom = Session["dateFrom"] as string;
            //string dateTo = Session["dateTo"] as string;
            //string FilterStatus = Session["status"] as string;
            Debug.WriteLine(dateFrom);
            var sql = from sp in dbsp.V_SCM_Sparepart_Request select sp;
            int sessionStatus;
            if (!string.IsNullOrEmpty(dateFrom))
            {
                TempData["statusFilter"] = status;
                DateTime fromDate = Convert.ToDateTime(dateFrom);
                var StartDate = fromDate.ToString("yyyy-mm-dd");
                DateTime toDate = Convert.ToDateTime(dateTo);
                var EndDate = toDate.ToString("yyyy-mm-dd");
                //int status = Convert.ToInt32(FilterStatus);
                Debug.WriteLine("a");
                if (status == 0)
                {
                    Debug.WriteLine("aa");
                    if (CurrUser.RoleName == "AdminSparepart" || CurrUser.RoleName == "Administrator" || CurrUser.RoleName == "WarehouseSparepart")
                    {
                        Debug.WriteLine("aaa");
                        sql = sql.Where(w => (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate));
                    }
                    else
                    {
                        Debug.WriteLine("aab");
                        sql = sql.Where(w => (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate) && w.userRequest == currUser);
                    }
                    
                } else
                {
                    Debug.WriteLine("ab");
                    if (CurrUser.RoleName == "AdminSparepart" || CurrUser.RoleName == "Administrator" || CurrUser.RoleName == "WarehouseSparepart")
                    {
                        Debug.WriteLine("aba");
                        sql = sql.Where(w => w.Status == status && (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate));
                    }
                    else
                    {
                        Debug.WriteLine("abb");
                        sql = sql.Where(w => w.Status == status && (DbFunctions.TruncateTime(w.Create_Time) >= fromDate && DbFunctions.TruncateTime(w.Create_Time) <= toDate) && w.userRequest == currUser);
                    }
                }
                sessionStatus = status;
            }
            else
            {
                TempData["statusFilter"] = 0;
                Debug.WriteLine("b");
                if (CurrUser.RoleName == "AdminSparepart" || CurrUser.RoleName == "Administrator" || CurrUser.RoleName == "WarehouseSparepart")
                {
                    Debug.WriteLine("ba");
                    sql = sql.Where(w => w.Status == 1).OrderByDescending(o => o.Create_Time);
                    sessionStatus = 1;
                }
                else
                {

                    Debug.WriteLine("bb");
                    sql = sql.Where(w => w.Status == 1 && w.userRequest == currUser).OrderByDescending(o => o.Create_Time);
                    sessionStatus = 1;
                }
            }
            var result = sql.OrderByDescending(o=>o.Create_Time).ToList();

            List<Tbl_V_SCM_Sparepart_Request> reqList = new List<Tbl_V_SCM_Sparepart_Request>();
            foreach (var item in result)
            {
                var actDetail = Url.Action("DetailRequest", "Sparepart", new { area = "SCM", RequestNo = item.RequestNo });
                var urlDetail = "<a href="+ actDetail+" class=\"btn btn-info btn-sm\" data-toggle=\"tooltip\" data-placement=\"right\" title=\"view Detail\"><i class=\"fa fa-eye\"></i> </a>";

                var action = Url.Action("ReturRequest", "Sparepart", new { area = "SCM", RequestNo = item.RequestNo });
                var urlAction = "<a href=" + action + " class=\"btn btn-warning btn-sm\" data-toggle=\"tooltip\" data-placement=\"right\" title=\"Retur Item\"><i class=\"fa fa-undo\"></i> </a>";

                string btnAction;
                if (item.Status == 4 && item.IsReturn == 0)
                {
                    btnAction = urlDetail + " " + urlAction;
                } else
                {
                    btnAction = urlDetail;
                }

                string sta = "";
                string badge = "";
                //string countdown = "";
                if (item.Status == 1)
                {
                    sta = "open";
                    badge = "primary";
                    //countdown = "";
                }
                else if (item.Status == 2)
                {
                    sta = "Preparing";
                    badge = "warning";
                    //countdown = "";
                }
                else if (item.Status == 3)
                {
                    sta = "Ready";
                    badge = "info";
                    //countdown = "";
                    //countdown = Html.Raw("Please Close your request before <strong>") + tbl.CloseTimeDueDate.ToString("dd - MMMM - yyyy") + Html.Raw("</strong>");
                }
                else if (item.Status == 10)
                {
                    sta = "Cancelled";
                    badge = "dark";
                    //countdown = "";
                }
                else
                {
                    sta = "Close";
                    badge = "secondary";
                    //countdown = "";
                }

                reqList.Add(
                    new Tbl_V_SCM_Sparepart_Request
                    {
                        RequestNo = item.RequestNo,
                        RequestBy = item.Name,
                        Remark = item.Remark,
                        Section = item.CostName,
                        RequestTime = item.Create_Time.ToString("dd MMM yyyy"),
                        Status = "<h5 ><span class=\"badge badge-"+ badge +"\"> " + sta + "</span></h5>",
                        Action = btnAction,

                    });
            }
            //List<SCM_Sparepart_Request_Temp> sCM_Sparepart_Request_Temps = dbsp.SCM_Sparepart_Request_Temp.ToList();
            //List<V_SCM_Sparepart_Master_List> V_SCM_Sparepart_Master_List = dbsp.V_SCM_Sparepart_Master_List.ToList();

            //var item = (
            //    from ai in sCM_Sparepart_Request_Temps
            //    join al in V_SCM_Sparepart_Master_List on ai.ITEMID equals al.ITEMID
            //    where (ai.userRequest == CurrUser.NIK)
            //    select new SCM_Sparepart_Request_Temp_Views
            //    {
            //        ITEMID = ai.ITEMID,
            //        ProductName = al.ProductName,
            //        Quantity = ai.quantity,
            //        UserRequest = ai.userRequest
            //    }).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp_Views.SqlQuery("SELECT a.ITEMID, a.userRequest , a.quantity, b.ProductName from SCM_Sparepart_Request_Temp a LEFT JOIN V_AXItemMaster b ON b.ITEMID = a.ITEMID WHERE a.userRequest = {0}", CurrUser.NIK).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp.Where(w => w.userRequest == CurrUser.NIK).ToList();
            Debug.WriteLine(reqList);
            return Json(new { reqList = reqList, status = sessionStatus, role = CurrUser.RoleName }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteItemRequest(int Id)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            try
            {
                var data = dbsp.SCM_Sparepart_Request_Detail.Where(w => w.Id == Id).FirstOrDefault();
                dbsp.SCM_Sparepart_Request_Detail.Remove(data);
                var del = dbsp.SaveChanges();
                if (del == 1)
                {
                    return Json(new
                    {
                        status = "1",
                        msg = "Item Deleted"
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = "0",
                        msg = "failed"
                    });
                }
            }
            catch
            {
                return Json(new
                {
                    status = "2",
                    msg = "failed"
                });
            }

        }
        [HttpPost]
        public ActionResult PrepareRequest(string[] requestNo)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            foreach (string ITEMID in requestNo)
            {
                SCM_Sparepart_Request_Header item = new SCM_Sparepart_Request_Header();
                item.RequestNo = ITEMID;
                dbsp.SCM_Sparepart_Request_Header.Attach(item);
                item.Status = 2; //Change a filed's value
                item.PrepareTime = DateTime.Now;
            }

            //return Json(requestNo, JsonRequestBehavior.AllowGet);
            var save = dbsp.SaveChanges();

            if (save > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Item prepared",
                    displayData = "prepared"
                });
            }
            else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Failed Prepare Item",
                    displayData = "Opened"
                });
            }
        }

        public ActionResult ReadyItem(int Id)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            var DetailItem = dbsp.SCM_Sparepart_Request_Detail.Where(w => w.Id == Id).FirstOrDefault();

            DetailItem.IsReady = 1;
            DetailItem.Qty_Realization = DetailItem.Quantity;

            var save = dbsp.SaveChanges();

            if (save > 0)
            {
                return Json(new
                {
                    status = 1,
                    msg = "Item Ready",
                    displayData = "Ready"
                });
            }
            else
            {
                return Json(new
                {
                    status = 0,
                    msg = "Failed Ready Item",
                    displayData = "prepared"
                });
            }
            //try
            //{
            //    SCM_Sparepart_Request_Temp sCM_Sparepart_Request_Temp = db.SCM_Sparepart_Request_Temp.Find(itemid, CurrUser.NIK);
            //    db.SCM_Sparepart_Request_Temp.Remove(sCM_Sparepart_Request_Temp);
            //    var del = db.SaveChanges();
            //    if (del == 1)
            //    {
            //        return Json(new
            //        {
            //            status = "1",
            //            msg = "Item Deleted"
            //        });
            //    }
            //    else
            //    {
            //        return Json(new
            //        {
            //            status = "0",
            //            msg = "failed"
            //        });
            //    }
            //}
            //catch
            //{
            //    return Json(new
            //    {
            //        status = "2",
            //        msg = "failed",
            //        itemid = itemid,
            //        userNIK = CurrUser.NIK
            //    });
            //}

        }

        public ActionResult PostScanItem(string ITEMID, string RequestNo)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            var QDetailItem = dbsp.SCM_Sparepart_Request_Detail.Where(w => w.ITEMID == ITEMID && w.RequestNo == RequestNo);
            int countItem = QDetailItem.Count();

            if (countItem > 0)
            {
                var DetailItem = QDetailItem.FirstOrDefault();
                DetailItem.IsReady = 1;
                DetailItem.Qty_Realization = DetailItem.Quantity;
                var save = dbsp.SaveChanges();
                if (save > 0)
                {
                    return Json(new
                    {
                        status = 1,
                        msg = "Item Ready"
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = 0,
                        msg = "<div class='alert alert-warning mt-1 p-1' role='alert'> Item <strong>" + ITEMID + "</strong> Already Scan</div>"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    status = 2,
                    msg = "<div class='alert alert-danger mt-1 p-1' role='alert'> Item <strong>" + ITEMID + "</strong> Not Found</div>"
                });
            }




            //try
            //{
            //    SCM_Sparepart_Request_Temp sCM_Sparepart_Request_Temp = db.SCM_Sparepart_Request_Temp.Find(itemid, CurrUser.NIK);
            //    db.SCM_Sparepart_Request_Temp.Remove(sCM_Sparepart_Request_Temp);
            //    var del = db.SaveChanges();
            //    if (del == 1)
            //    {
            //        return Json(new
            //        {
            //            status = "1",
            //            msg = "Item Deleted"
            //        });
            //    }
            //    else
            //    {
            //        return Json(new
            //        {
            //            status = "0",
            //            msg = "failed"
            //        });
            //    }
            //}
            //catch
            //{
            //    return Json(new
            //    {
            //        status = "2",
            //        msg = "failed",
            //        itemid = itemid,
            //        userNIK = CurrUser.NIK
            //    });
            //}

        }
        [HttpPost]
        public ActionResult ConfirmReadyRequest(string requestNo)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            var EmailAddress = CurrUser.Email;

            // get stock for each item //
            int noStock = 0;
            var StockMinus = new ArrayList();
            var RequestDtl = dbsp.SCM_Sparepart_Request_Detail.Where(w => w.RequestNo == requestNo).ToList();
            foreach (var dtl in RequestDtl)
            {
                var DtlItemID = dtl.ITEMID;
                var Qstock = dbsp.V_SCM_Sparepart_Master_List.Where(w => w.ITEMID == DtlItemID).FirstOrDefault();
                if (Qstock.Stock < dtl.Qty_Realization)
                {
                    noStock++;
                    StockMinus.Add(dtl.ITEMID);
                }
            }
            //-- get stock for each item --//

            //if (noStock == 0)
            //{
                var Request = dbsp.SCM_Sparepart_Request_Header.Where(w => w.RequestNo == requestNo).FirstOrDefault();
                DateTime today = DateTime.Now;
                Request.Status = 3;
                Request.ReadyTime = today;
                Request.ReadyBy = CurrUser.NIK;
                Request.CloseTimeDueDate = today.AddDays(3);

                var save = dbsp.SaveChanges();

                if (save > 0)
                {
                    if (EmailAddress != null)
                    {
                        string FilePath = Path.Combine(Server.MapPath("~/Emails/SCM/Sparepart/"), "Test.html");
                        StreamReader str = new StreamReader(FilePath);
                        string MailText = str.ReadToEnd();
                        str.Close();

                        var act = Url.Action("DetailRequest", "Sparepart", new { area = "SCM", RequestNo = requestNo });
                        //Repalce [newusername] = signup user name   
                        MailText = MailText.Replace("##UserName##", requestNo);
                        MailText = MailText.Replace("##Dept##", "Information Technology");
                        MailText = MailText.Replace("##Url##", act);

                        var senderEmail = new MailAddress("ngkportal-notification@ngkbusi.com", "Sparepart Apps");
                        var receiverEmail = new MailAddress(EmailAddress, "Receiver");
                        var password = "100%NGKbusi!";
                        var sub = "[no-reply] Sparepart Apps";
                        var body = MailText;
                        var smtp = new SmtpClient
                        {
                            Host = "ngkbusi.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,

                            Credentials = new NetworkCredential(senderEmail.Address, password)
                        };
                        using (var mess = new MailMessage(senderEmail, receiverEmail)
                        {
                            Subject = sub,
                            Body = body,
                            IsBodyHtml = true
                        })
                        {
                            smtp.Send(mess);
                        }
                    }
                    return Json(new
                    {
                        status = '1',
                        msg = "Request ready to pickup"
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = '0',
                        msg = "Failed to Ready pickup request"
                    });
                }
            //}
            //else
            //{
            //    return Json(new
            //    {
            //        status = '0',
            //        msg = "Not Enough Stock For ITEM  " + StockMinus
            //    });
            //}
        }

        [HttpPost]
        public ActionResult CloseRequest(string[] requestNo)
        {
            //string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //SqlConnection con = new SqlConnection(cnnString);

            //SqlCommand cmd = new SqlCommand("sp_SCM_Sparepart_Request_Close", con);
            //cmd.CommandType = CommandType.StoredProcedure;


            //cmd.Parameters.AddWithValue("@requestNo", requestNo);

            //con.Open();
            ////SqlDataReader i = cmd.ExecuteReader();
            //con.Close();
            //var join = string.Join(",", requestNo);

            //return Json(new
            //{
            //    status = requestNo,
            //    msg = join
            //});


            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            var updateDataHeader = dbsp.SCM_Sparepart_Request_Header.Where(w => requestNo.Contains(w.RequestNo)).ToList();
            foreach (var data in updateDataHeader)
            {
                data.Status = 4;
                data.CloseTime = DateTime.UtcNow.Date;
            }

            var updateHeader = dbsp.SaveChanges();

            var getDataTotal = dbsp.SCM_Sparepart_Request_Detail.Where(w => requestNo.Contains(w.RequestNo)).GroupBy(g => g.ITEMID).Select(s => new { QtyRealization = s.Sum(i => i.Qty_Realization), ITEMID = s.Key }).ToList();

            //db.SaveChanges();
            foreach (var ReqId in getDataTotal)
            {
                var item = dbsp.SCM_Sparepart_Master_List.Where(w => w.ITEMID == ReqId.ITEMID).FirstOrDefault();
                dbsp.SCM_Sparepart_Master_List.Attach(item);
                item.Quantity = (item.Quantity - ReqId.QtyRealization);
                //Change a filed's value
            }

            var updateStock = dbsp.SaveChanges();

            if (updateHeader > 0 && updateStock > 0)
            {
                return Json(new { status = 1, msg = "Request Closed", displayData = "Closed" });
            }
            else if (updateHeader > 0)
            {
                return Json(new { status = 0, msg = "Request Close, But Stock Failed to Update", displayData = "Ready" });
            }
            else
            {
                return Json(new { status = 0, msg = "Failed Close Request", displayData = "Ready" });
            }

            //if (save > 0)
            //{

            //    return Json(new
            //    {
            //        status = '1',
            //        msg = "Request Closed"
            //    });
            //}
            //else
            //{
            //    return Json(new
            //    {
            //        status = '0',
            //        msg = "Failed Closed Request"
            //    });
            //}
        }

        public ActionResult CancelRequest(string requestNo)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            var updateDataHeader = dbsp.SCM_Sparepart_Request_Header.Where(w => w.RequestNo == requestNo).ToList();
            foreach (var data in updateDataHeader)
            {
                data.Status = 10;
                data.CancelTime = DateTime.UtcNow.Date;
                data.IsCancel = 1;
                data.CancelBy = CurrUser.NIK;
            }

            var updateHeader = dbsp.SaveChanges();

            var getDetail = dbsp.SCM_Sparepart_Request_Detail.Where(w => w.RequestNo == requestNo).ToList();

            foreach (var item in getDetail)
            {

                item.IsCancel = 1;
                //Change a filed's value
            }

            var updateDetail = dbsp.SaveChanges();

            if (updateHeader > 0 && updateDetail > 0)
            {
                return Json(new { status = 1, msg = "Request Cancel" });
            }
            else
            {
                return Json(new { status = 0, msg = "Failed Cancel Request" });
            }
        }
        [HttpGet]
        public ActionResult ReturRequest(string RequestNo)
        {
            var data = (from c in dbsp.SCM_Sparepart_Request_Header
                        join cn in dbsp.V_Users_Active on c.UserRequest equals cn.NIK
                        where (c.RequestNo == RequestNo)
                        select new ItemDetailDTO { RequestNo = c.RequestNo, Create_Time = c.Create_Time, Status = c.Status, Remark = c.Remark, UserRequest = cn.Name, CloseTimeDueDate = c.CloseTimeDueDate, CostName = c.CostName }).FirstOrDefault();
            ViewBag.header = data;
            ViewBag.RequestNo = RequestNo;
            var countNotReady = dbsp.V_SCM_Sparepart_Request_Detail.Count(w => w.RequestNo == RequestNo && w.IsReady == 0);
            ViewBag.NotReady = countNotReady;
            //var header = new List<SCM_Sparepart_Request_Header>();
            //foreach ( var tdata in results)
            //{
            //    var sp = new SCM_Sparepart_Request_Header();
            //    sp.RequestNo = tdata.RequestNo;

            //    header.Add(sp);
            //}


            //return Json(new
            //{
            //    status = RequestNo,
            //    msg = results,
            //    spl =  spl
            //}, JsonRequestBehavior.AllowGet);

            return View();
        }
        public ActionResult ItemListRetur(string RequestNo)
        {
            var spl = dbsp.V_SCM_Sparepart_Request_Detail.Where(w => w.RequestNo == RequestNo).ToList();
            var CountRow = dbsp.V_SCM_Sparepart_Request_Detail.Count(w => w.RequestNo == RequestNo);

            List<V_SCM_Sparepart_ItemList> actions = new List<V_SCM_Sparepart_ItemList>();

            var countNotReady = dbsp.V_SCM_Sparepart_Request_Detail.Count(w => w.RequestNo == RequestNo && w.IsReady == 0);

            foreach (var Item in spl)
            {
                var Tools = "";

                Tools = "<a href=\"#EditQuantityModal\" data-toggle=\"modal\" title=\"Edit\" id=\"EditQtyItem\" data-itemid=\"" + Item.ITEMID + "\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-warning EditQtyItem\"><i class=\"fa fa-pencil\"></i></a> <a href=\"#\" title=\"Delete\" id=\"DeleteItem\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-danger DeleteItem\"><i class=\"fa fa-trash\"></i></a>";

                if (Item.IsReady == 0)
                {
                    Tools = "<a href=\"#\" title=\"ready\" id=\"procItem\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-primary procItem\"><i class=\"fa fa-check\"></i></a>";
                }
                else
                {
                    Tools = "<a href=\"#EditQuantityRealizationModal\" data-toggle=\"modal\" title=\"Insert Qty Retur\" id=\"editQty\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-warning procItem\"><i class=\"fa fa-edit btnEdit\"></i></a>";
                }

                actions.Add(
                    new V_SCM_Sparepart_ItemList
                    {
                        ITEMID = Item.ITEMID,
                        ProductName = Item.ProductName,
                        Quantity = Item.Quantity,
                        Qty_Realization = Item.Qty_Realization,
                        Tools = Tools,
                        Qty_Retur = "<input type=\"hidden\" class=\"form-control w-50 p-3\" name=\"ITEMID[]\" value=\"" + Item.ITEMID + "\"> <input type=\"number\" onkeyup=\"if (value > " + Item.Qty_Realization + ") value=0;\" min =\"0\" max=\"" + Item.Qty_Realization + "\" class=\"form-control w-50 p-3\" name=\"QtyRetur[" + Item.ITEMID + "]\" value=\"" + Item.Qty_Retur + "\">",
                        ReturNotes = "<input type =\"text\"  class=\"form-control\" name=\"ReturnNotes[" + Item.ITEMID + "]\" value=\"" + Item.ReturNotes + "\">"
                    });
            }

            return Json(new
            {
                rows = actions,
                totalNotFiltered = CountRow,
                total = CountRow,
                notReady = countNotReady
            }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult SaveReturRequest(string RequestNo)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            var spl = dbsp.SCM_Sparepart_Request_Detail.Where(w => w.RequestNo == RequestNo).ToList();

            List<SCM_Sparepart_Return_Detail> ReturList = new List<SCM_Sparepart_Return_Detail>();
            var returQty = new ArrayList();
            int insert = 0;
            foreach (var item in spl)
            {
                //var sp = new SCM_Sparepart_Request_Header();
                //sp.RequestNo = tdata.RequestNo;

                ////    header.Add(sp);

                int QtyRetur = int.Parse(Request["QTYRetur[" + item.ITEMID + "]"]);
                if (QtyRetur > 0)
                {
                    dbsr.SCM_Sparepart_Return_Detail.Add(new SCM_Sparepart_Return_Detail
                    {
                        RequestNo = RequestNo,
                        Quantity = int.Parse(Request["QTYRetur[" + item.ITEMID + "]"]),
                        ReturnNotes = Request["ReturnNotes[" + item.ITEMID + "]"],
                        ITEMID = item.ITEMID,
                        Create_By = CurrUser.NIK,
                        Create_Time = DateTime.Now,
                        Status = "open"
                    }); 

                    int i = dbsr.SaveChanges();
                    if (i > 0)
                    {
                        insert++;
                    }
                }

            }


            //string ReturnNo =  DateTime.Now.ToString("yyyyMMddHHmmss");

            //SCM_Sparepart_Return_Header header = new SCM_Sparepart_Return_Header();
            //header.ReturnNo = ReturnNo;
            //header.RequestNo = RequestNo;
            //header.CostName = CurrUser.CostName;
            //header.DivisionName = CurrUser.DivisionName;
            //header.SectionName = CurrUser.SectionName;
            //header.SubSectionName = CurrUser.SubSectionName;
            //header.Status = 1;
            //header.UserReturn = CurrUser.NIK;
            //header.Create_Time = DateTime.Now;

            //var headerList = dbsr.SCM_Sparepart_Return_Header.Add(header);
            //var saveHeader = dbsr.SaveChanges();

            //if (saveHeader >= 0)
            //{
            //    var detailRequest = dbsp.V_SCM_Sparepart_Request_Detail.Where(w => w.RequestNo == RequestNo).ToList();

            //    SCM_Sparepart_Return_Detail detail = new SCM_Sparepart_Return_Detail();
            //    foreach (var item in detailRequest)
            //    {
            //        detail.ITEMID = item.ITEMID;
            //        detail.Quantity = Qty_Retur[0];
            //        detail.Qty_Realization = 
            //    }
            //}
            if (insert > 0)
            {
                //update request header
                var query = dbsp.SCM_Sparepart_Request_Header.Where(w => w.RequestNo == RequestNo).FirstOrDefault();
                DateTime today = DateTime.Now;
                query.IsReturn = 1;

                var save = dbsp.SaveChanges();

                return Json(new
                {
                    status = 1,
                    rows = returQty,
                    //totalNotFiltered = Qty_Retur,
                    //total = ReturNotes
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    status = 0,
                    rows = returQty,
                    //totalNotFiltered = Qty_Retur,
                    //total = ReturNotes
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CreateRetur()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();

            ViewBag.CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            ViewBag.UserList = db.V_Users_Active.Where(w => w.Status == "Permanent").ToList();


            var spl = dbsp.V_SCM_Sparepart_Master_List.Where(w => w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling").ToList();
            ViewBag.SparepartList = spl;

            return View();
        }
        [Authorize(Roles = "AdminSparepart, Administrator, WarehouseSparepart,GroupLeader")]
        public ActionResult Report()
        {
            //var spl = db.V_SCM_Sparepart_Master_List.Where(w =>  w.CostName == CurrUser.CostName && w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling").ToList();
            var spl = dbsp.V_SCM_Sparepart_Master_List.Where(w => w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling");
            var query = spl.GroupBy(row => new { row.ProCateName, row.ProductCategory }).Select(group => new { ProCateName = group.Key.ProCateName, ProductCategory = group.Key.ProductCategory }).ToList();
            var section = db.Users_Section_AX.ToList();

            List<V_SCM_Sparepart_Master_List> par = new List<V_SCM_Sparepart_Master_List>();

            foreach (var data in query)
            {
                par.Add(new V_SCM_Sparepart_Master_List
                {
                    ProCateName = data.ProCateName,
                    ProductCategory = data.ProductCategory
                });
            }

            ViewBag.Section = section;
            ViewBag.ItemGroup = par;
            ViewBag.RackList = dbsp.SCM_Sparepart_Rack.Where(w => w.IsDelete == 0).ToList();
            ViewBag.NavHide = true;
            return View();
        }
        [Authorize(Roles = "Administrator, WarehouseSparepart")]
        public ActionResult ReportD365()
        {
            //var spl = db.V_SCM_Sparepart_Master_List.Where(w =>  w.CostName == CurrUser.CostName && w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling").ToList();
            var spl = dbsp.V_SCM_Sparepart_Master_List.Where(w => w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling");
            var query = spl.GroupBy(row => new { row.ProCateName, row.ProductCategory }).Select(group => new { ProCateName = group.Key.ProCateName, ProductCategory = group.Key.ProductCategory }).ToList();
            var section = db.Users_Section_AX.ToList();

            List<V_SCM_Sparepart_Master_List> par = new List<V_SCM_Sparepart_Master_List>();

            foreach (var data in query)
            {
                par.Add(new V_SCM_Sparepart_Master_List
                {
                    ProCateName = data.ProCateName,
                    ProductCategory = data.ProductCategory
                });
            }

            ViewBag.Section = section;
            ViewBag.ItemGroup = par;
            ViewBag.RackList = dbsp.SCM_Sparepart_Rack.Where(w => w.IsDelete == 0).ToList();
            ViewBag.NavHide = false;
            return View();
        }

        [Authorize(Roles = "AdminSparepart, Administrator, WarehouseSparepart, GroupLeader")]
        [HttpPost]
        public ActionResult GenerateReport(SCM_Sparepart_Report smodel)
        {
            string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            SqlConnection con = new SqlConnection(cnnString);

            List<SCM_Sparepart_Report> k3list = new List<SCM_Sparepart_Report>();

            SqlCommand cmd = new SqlCommand("sp_SCM_Sparepart_Generate_Report", con);
            cmd.CommandType = CommandType.StoredProcedure;

            var reportTitle = "";
            var StrDateStart = Convert.ToDateTime(smodel.dateFrom).ToString("dd MMM yyyy");
            var StrDateTo = Convert.ToDateTime(smodel.dateTo).ToString("dd MMM yyyy");

            // date from 
            if (smodel.dateFrom.HasValue)
            {

                cmd.Parameters.AddWithValue("@dateFrom", smodel.dateFrom);
            }
            else
            {
                DateTime todayDate = DateTime.UtcNow.Date.AddDays(-7);
                cmd.Parameters.AddWithValue("@dateFrom", todayDate);
            }
            // date to
            if (smodel.dateTo.HasValue)
            {
                cmd.Parameters.AddWithValue("@dateTo", smodel.dateTo);
            }
            else
            {
                DateTime date2 = DateTime.UtcNow.Date;
                cmd.Parameters.AddWithValue("@dateTo", date2);
            }

            cmd.Parameters.AddWithValue("@ItemGroup", smodel.ItemGroup);
            cmd.Parameters.AddWithValue("@ProductCategory", smodel.ProductCategory);
            cmd.Parameters.AddWithValue("@COSTNAME", smodel.COSTNAME);
            cmd.Parameters.AddWithValue("@Status", smodel.Status);
            cmd.Parameters.AddWithValue("@MaintenanceType", smodel.MaintenanceType);

            SqlDataAdapter sd = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            con.Open();
            sd.Fill(dt);
            con.Close();

            if (smodel.dateFrom.HasValue && smodel.dateTo.HasValue)
            {
                reportTitle = "report periode " + StrDateStart + " - " + StrDateTo;

            }
            else
            {
                reportTitle = "report periode last 30 days";
            }


            foreach (DataRow dr in dt.Rows)
            {
                var ct = Convert.ToDateTime(dr["Create_Time"]);
                var createTime = ct.ToString("yyyy-MM-dd HH:mm");
                // get status
                var status = "";
                if (Convert.ToString(dr["Status"]) == "1")
                {
                    status = "Open";
                } else if (Convert.ToString(dr["Status"]) == "2")
                {
                    status = "Prepare";
                } else if (Convert.ToString(dr["Status"]) == "3")
                {
                    status = "Ready";
                } else if (Convert.ToString(dr["Status"]) == "4")
                {
                    status = "Close";
                }
                else if (Convert.ToString(dr["Status"]) == "10")
                {
                    status = "Cancel";
                }

                string getSeconds = "";

                if (Convert.ToInt32(dr["TotalSeconds"]) > 0)
                {
                    getSeconds = Helpers.Convertion.ConvertSecondsToDHMS(Convert.ToInt32(dr["TotalSeconds"]));
                }
                else
                {
                    getSeconds = "item not ready";
                }

                k3list.Add(
                    new SCM_Sparepart_Report
                    {
                        ITEMID = Convert.ToString(dr["ITEMID"]),
                        Quantity = Convert.ToInt32(dr["Qty_Realization"]),
                        ProductName = Convert.ToString(dr["ProductName"]),
                        ItemGroup = Convert.ToString(dr["ItemGroup"]),
                        ProCateName = Convert.ToString(dr["ProCateName"]),
                        Create_Date = Convert.ToDateTime(createTime).ToString("M/d/yyyy"),
                        PrepareDate = Convert.ToDateTime(createTime).ToString("M/d/yyyy"),
                        Create_Time = Convert.ToDateTime(createTime).ToString("HH:mm"),
                        COSTNAME = Convert.ToString(dr["COSTNAME"]),
                        Status = status,
                        NameUserRequest = Convert.ToString(dr["Name"]),
                        ReadyByName = Convert.ToString(dr["ReadyByName"]),
                        MaintenanceType = Convert.ToString(dr["MaintenanceType"]),
                        TotalSeconds = Convert.ToInt32(dr["TotalSeconds"]),
                        Duration = getSeconds,
                        RackBoxName = Convert.ToString(dr["RackBoxName"]),
                        StockAvailable = Convert.ToInt32(dr["Stock"]),
                        QtyNegative = Convert.ToInt32(MakeNegative(Convert.ToInt32(dr["Qty_Realization"]))),
                        Site = "BNGK"

                    });
            }

            return Json(new { rows = k3list, title = reportTitle }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GenerateReport365(SCM_Sparepart_Report smodel)
        {
            string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            SqlConnection con = new SqlConnection(cnnString);

            List<SCM_Sparepart_Report> k3list = new List<SCM_Sparepart_Report>();

            SqlCommand cmd = new SqlCommand("sp_SCM_Sparepart_Generate_Report365", con);
            cmd.CommandType = CommandType.StoredProcedure;

            var reportTitle = "";
            var StrDateStart = Convert.ToDateTime(smodel.dateFrom).ToString("dd MMM yyyy");
            var StrDateTo = Convert.ToDateTime(smodel.dateTo).ToString("dd MMM yyyy");

            // date from 
            if (smodel.dateFrom.HasValue)
            {

                cmd.Parameters.AddWithValue("@dateFrom", smodel.dateFrom);
            }
            else
            {
                DateTime todayDate = DateTime.UtcNow.Date.AddDays(-7);
                cmd.Parameters.AddWithValue("@dateFrom", todayDate);
            }
            // date to
            if (smodel.dateTo.HasValue)
            {
                cmd.Parameters.AddWithValue("@dateTo", smodel.dateTo);
            }
            else
            {
                DateTime date2 = DateTime.UtcNow.Date;
                cmd.Parameters.AddWithValue("@dateTo", date2);
            }

            cmd.Parameters.AddWithValue("@ItemGroup", smodel.ItemGroup);
            cmd.Parameters.AddWithValue("@ProductCategory", smodel.ProductCategory);
            cmd.Parameters.AddWithValue("@COSTNAME", smodel.COSTNAME);
            cmd.Parameters.AddWithValue("@Status", smodel.Status);
            cmd.Parameters.AddWithValue("@MaintenanceType", smodel.MaintenanceType);

            SqlDataAdapter sd = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            con.Open();
            sd.Fill(dt);
            con.Close();

            if (smodel.dateFrom.HasValue && smodel.dateTo.HasValue)
            {
                reportTitle = "report periode " + StrDateStart + " - " + StrDateTo;

            }
            else
            {
                reportTitle = "report periode last 30 days";
            }


            foreach (DataRow dr in dt.Rows)
            {
                var ct = Convert.ToDateTime(dr["Create_Time"]);
                var pt = Convert.ToDateTime(dr["PrepareTime"]);
                var createTime = ct.ToString("yyyy-MM-dd HH:mm");
                var PrepareTime = pt.ToString("yyyy-MM-dd HH:mm");
                // get status
                var status = "";
                if (Convert.ToString(dr["Status"]) == "1")
                {
                    status = "Open";
                }
                else if (Convert.ToString(dr["Status"]) == "2")
                {
                    status = "Prepare";
                }
                else if (Convert.ToString(dr["Status"]) == "3")
                {
                    status = "Ready";
                }
                else if (Convert.ToString(dr["Status"]) == "4")
                {
                    status = "Close";
                }
                else if (Convert.ToString(dr["Status"]) == "10")
                {
                    status = "Cancel";
                }

                string getSeconds = "";

                if (Convert.ToInt32(dr["TotalSeconds"]) > 0)
                {
                    getSeconds = Helpers.Convertion.ConvertSecondsToDHMS(Convert.ToInt32(dr["TotalSeconds"]));
                }
                else
                {
                    getSeconds = "item not ready";
                }

                k3list.Add(
                    new SCM_Sparepart_Report
                    {
                        ITEMID = Convert.ToString(dr["ITEMID"]),
                        Quantity = Convert.ToInt32(dr["Qty_Realization"]),
                        ProductName = Convert.ToString(dr["ProductName"]),
                        ItemGroup = Convert.ToString(dr["ItemGroup"]),
                        ProCateName = Convert.ToString(dr["ProCateName"]),
                        Create_Date = Convert.ToDateTime(createTime).ToString("M/d/yyyy"),
                        PrepareDate = Convert.ToDateTime(PrepareTime).ToString("M/d/yyyy"),
                        Create_Time = Convert.ToDateTime(createTime).ToString("HH:mm"),
                        COSTNAME = Convert.ToString(dr["COSTNAME"]),
                        Status = status,
                        NameUserRequest = Convert.ToString(dr["Name"]),
                        ReadyByName = Convert.ToString(dr["ReadyByName"]),
                        MaintenanceType = Convert.ToString(dr["MaintenanceType"]),
                        TotalSeconds = Convert.ToInt32(dr["TotalSeconds"]),
                        Duration = getSeconds,
                        RackBoxName = Convert.ToString(dr["RackBoxName"]),
                        StockAvailable = Convert.ToInt32(dr["Stock"]),
                        QtyNegative = Convert.ToInt32(MakeNegative(Convert.ToInt32(dr["Qty_Realization"]))),
                        Site = "BNGK"

                    });
            }

            return Json(new { rows = k3list, title = reportTitle }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult exportExcelReport(SCM_Sparepart_Report smodel)
        {
            string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            SqlConnection con = new SqlConnection(cnnString);

            List<SCM_Sparepart_Report> k3list = new List<SCM_Sparepart_Report>();

            SqlCommand cmd = new SqlCommand("sp_SCM_Sparepart_Generate_Report365", con);
            cmd.CommandType = CommandType.StoredProcedure;

            var reportTitle = "";
            var StrDateStart = Convert.ToDateTime(smodel.dateFrom).ToString("dd MMM yyyy");
            var StrDateTo = Convert.ToDateTime(smodel.dateTo).ToString("dd MMM yyyy");

            // date from 
            if (smodel.dateFrom.HasValue)
            {

                cmd.Parameters.AddWithValue("@dateFrom", smodel.dateFrom);
            }
            else
            {
                DateTime todayDate = DateTime.UtcNow.Date.AddDays(-7);
                cmd.Parameters.AddWithValue("@dateFrom", todayDate);
            }
            // date to
            if (smodel.dateTo.HasValue)
            {
                cmd.Parameters.AddWithValue("@dateTo", smodel.dateTo);
            }
            else
            {
                DateTime date2 = DateTime.UtcNow.Date;
                cmd.Parameters.AddWithValue("@dateTo", date2);
            }

            cmd.Parameters.AddWithValue("@ItemGroup", smodel.ItemGroup);
            cmd.Parameters.AddWithValue("@ProductCategory", smodel.ProductCategory);
            cmd.Parameters.AddWithValue("@COSTNAME", smodel.COSTNAME);
            cmd.Parameters.AddWithValue("@Status", smodel.Status);
            cmd.Parameters.AddWithValue("@MaintenanceType", smodel.MaintenanceType);

            SqlDataAdapter sd = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            con.Open();
            sd.Fill(dt);
            con.Close();

            if (smodel.dateFrom.HasValue && smodel.dateTo.HasValue)
            {
                reportTitle = "report periode " + StrDateStart + " - " + StrDateTo;

            }
            else
            {
                reportTitle = "report periode last 30 days";
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "Prepare Date";
                worksheet.Cell(currentRow, 2).Value = "ITEMID";
                worksheet.Cell(currentRow, 3).Value = "Quantity";
                worksheet.Cell(currentRow, 4).Value = "Site";
                worksheet.Cell(currentRow, 5).Value = "Item Group";
                worksheet.Cell(currentRow, 6).Value = "Product Name";
                worksheet.Cell(currentRow, 7).Value = "Section";

                foreach (DataRow dr in dt.Rows)
                {
                    var ct = Convert.ToDateTime(dr["Create_Time"]);
                    var createTime = ct.ToString("yyyy-MM-dd HH:mm");
                    var pt = Convert.ToDateTime(dr["PrepareTime"]);
                    var prepareTime = pt.ToString("yyyy-MM-dd HH:mm");

                    // get status
                    var status = "";
                    if (Convert.ToString(dr["Status"]) == "1")
                    {
                        status = "Open";
                    }
                    else if (Convert.ToString(dr["Status"]) == "2")
                    {
                        status = "Prepare";
                    }
                    else if (Convert.ToString(dr["Status"]) == "3")
                    {
                        status = "Ready";
                    }
                    else if (Convert.ToString(dr["Status"]) == "4")
                    {
                        status = "Close";
                    }
                    else if (Convert.ToString(dr["Status"]) == "10")
                    {
                        status = "Cancel";
                    }

                    string getSeconds = "";

                    if (Convert.ToInt32(dr["TotalSeconds"]) > 0)
                    {
                        getSeconds = Helpers.Convertion.ConvertSecondsToDHMS(Convert.ToInt32(dr["TotalSeconds"]));
                    }
                    else
                    {
                        getSeconds = "item not ready";
                    }

                    currentRow++;
                    var itemGroup = "";
                    if (Convert.ToString(dr["ItemGroup"]) == "MachineP")
                    {
                        itemGroup = "MP";
                    } else
                    {
                        itemGroup = "TL";
                    }
                    worksheet.Cell(currentRow, 1).Value = Convert.ToDateTime(prepareTime).ToString("M/d/yyyy");
                    worksheet.Cell(currentRow, 2).Value = Convert.ToString(dr["ITEMID"]);
                    worksheet.Cell(currentRow, 3).Value = Convert.ToInt32(MakeNegative(Convert.ToInt32(dr["Qty_Realization"])));
                    worksheet.Cell(currentRow, 4).Value = "BNGK";
                    worksheet.Cell(currentRow, 5).Value = itemGroup;
                    worksheet.Cell(currentRow, 6).Value = Convert.ToString(dr["ProductName"]);
                    worksheet.Cell(currentRow, 7).Value = Convert.ToString(dr["COSTNAME"]);

                    k3list.Add(
                        new SCM_Sparepart_Report
                        {
                            ITEMID = Convert.ToString(dr["ITEMID"]),
                            Quantity = Convert.ToInt32(dr["Qty_Realization"]),
                            ProductName = Convert.ToString(dr["ProductName"]),
                            ItemGroup = Convert.ToString(dr["ItemGroup"]),
                            ProCateName = Convert.ToString(dr["ProCateName"]),
                            Create_Date = Convert.ToDateTime(createTime).ToString("M/d/yyyy"),
                            Create_Time = Convert.ToDateTime(createTime).ToString("HH:mm"),
                            COSTNAME = Convert.ToString(dr["COSTNAME"]),
                            Status = status,
                            NameUserRequest = Convert.ToString(dr["Name"]),
                            ReadyByName = Convert.ToString(dr["ReadyByName"]),
                            MaintenanceType = Convert.ToString(dr["MaintenanceType"]),
                            TotalSeconds = Convert.ToInt32(dr["TotalSeconds"]),
                            Duration = getSeconds,
                            RackBoxName = Convert.ToString(dr["RackBoxName"]),
                            StockAvailable = Convert.ToInt32(dr["Stock"]),
                            QtyNegative = Convert.ToInt32(MakeNegative(Convert.ToInt32(dr["Qty_Realization"]))),
                            Site = "BNGK"

                        });
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Report Sparepart to D365.xlsx");
                }
            }

            
        }

        [Authorize(Roles = "Administrator, WarehouseSparepart")]
        public ActionResult StockIn()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();

            ViewBag.CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            ViewBag.UserList = db.V_Users_Active.Where(w => w.Status == "Permanent").ToList();


            var spl = dbsp.V_SCM_Sparepart_Master_List.Where(w => w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling").ToList();
            ViewBag.SparepartList = spl;

            return View();
        }
        public class Tbl_SCM_D365ImporForm_ProductReceipt
        {
            public int No { get; set; }
            public string ProductReceipt { get; set; }
            public string Site { get; set; }
            public string Warehouse { get; set; }
            public string InternalProductReceipt { get; set; }
            public int TotalItem { get; set; }
            public string ItemGroup { get; set; }
            public DateTime? DateReceived { get; set; }
            public string ReceivedBy { get; set; }
        }
        [HttpPost]
        public ActionResult PackingSlipList()
        {
            ////var query = dbax.SCM_D365ImporForm_ProductReceipt.GroupBy(g => g.ProductReceipt).ToList();
            //var spl = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ItemGroup == "MachineP");
            //var CountRow = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ItemGroup == "MachineP").Count();

            //var results = dbax.SCM_D365ImporForm_ProductReceipt
            //                .Where(Q => Q.ItemGroup == "MachineP")
            //                .GroupBy(x => new { x.ProductReceipt, x.Warehouse, x.ItemGroup, x.Site }).Select(x =>
            //                new 
            //                {
            //                    ProductReceipt = x.Key,

            //                }).ToList();
            //var countResults = results.Count();

            string[] itemGroup = { "MachineP", "Tooling" };
            string s = "2023-06-06";
            DateTime dt = DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);


            var query = (from t in dbax.SCM_D365ImporForm_ProductReceipt.Where(w => itemGroup.Contains(w.ItemGroup) && w.ConfirmReceivedStatus == 0 && w.Site == "ITS" && w.Date > dt)
                         group t by new { t.ProductReceipt, t.Site }
             into grp
                         select new
                         {
                             ProductReceipt = grp.Key.ProductReceipt,
                             Site = grp.Key.Site,
                             TotalItem = grp.Count()
                         }).ToList();
            var countResults = query.Count();

            List<Tbl_SCM_D365ImporForm_ProductReceipt> actions = new List<Tbl_SCM_D365ImporForm_ProductReceipt>();
            var no = 0;
            foreach (var Item in query)
            {
                no++;
                var UrlAction = Url.Action("PackingSlipDetail", "Sparepart", new { area = "SCM", ProductReceipt = Item.ProductReceipt });
                actions.Add(
                    new Tbl_SCM_D365ImporForm_ProductReceipt
                    {
                        No = no,
                        ProductReceipt = "<a href='" + UrlAction + "' data-row-id='" + Item.ProductReceipt + "' class='tooltip-link' data-id='" + Item.ProductReceipt + "' >" + Item.ProductReceipt + "</a>",
                        Site = Item.Site,
                        TotalItem = Item.TotalItem
                    });
            }

            return Json(new
            {
                rows = actions,
                //totalNotFiltered = query,
                total = countResults,
            }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult PackingSlipItemList(string ProductReceipt)
        {
            var spl = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ProductReceipt == ProductReceipt).ToList();
            var countList = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ProductReceipt == ProductReceipt).Count();
            //cek if any status 0 (not yet confirm)
            var countNotConfirm = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ProductReceipt == ProductReceipt && w.ConfirmReceivedStatus == 0).Count();
            return Json(new
            {
                rows = spl,
                //totalNotFiltered = query,
                total = countList,
            }, JsonRequestBehavior.AllowGet);


        }
        [HttpGet]
        public ActionResult PackingSlipDetail(string ProductReceipt)
        {
            var spl = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ProductReceipt == ProductReceipt).ToList();
            var countList = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ProductReceipt == ProductReceipt).Count();
            //cek if any status 0 (not yet confirm)
            var countNotConfirm = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ProductReceipt == ProductReceipt && w.ConfirmReceivedStatus == 0).Count();

            ViewBag.ItemList = spl;
            ViewBag.ProductReceipt = ProductReceipt;
            var statusConfirm = 0;
            if (countNotConfirm > 0)
            {
                statusConfirm = 0;
            }
            else
            {
                statusConfirm = 1;
            }
            ViewBag.statusConfirmReceived = statusConfirm;

            return View();

        }
        [HttpPost]
        public ActionResult ConfirmReceivedStockIn(string ProductReceipt)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();

            var spl = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ProductReceipt == ProductReceipt).ToList();
            foreach (var data in spl)
            {
                // set status confirm received = 1 on table SCM_D365ImporForm_ProductReceipt 
                data.ConfirmReceivedStatus = 1;

                // insert all item to table SCM_sparepart_product_received
                SCM_Sparepart_Product_Received item = new SCM_Sparepart_Product_Received();

                item.ProductReceipt = ProductReceipt;
                item.QuantityReceived = Convert.ToInt32(data.ReceivedQuantity);
                item.ReceivedBy = currUser;
                item.ITEMID = data.Item;
                item.DateReceived = DateTime.Now;
                item.Site = data.Site;
                item.ItemGroup = data.ItemGroup;

                dbsp.SCM_Sparepart_Product_Received.Add(item);
                var save = dbsp.SaveChanges();

            }

            var update = dbax.SaveChanges();

            if (update > 0)
            {
                return Json(new { status = '1', msg = "Update Success" });
            }
            else
            {
                return Json(new { status = '0', msg = "Update Failed" });
            }
        }
        [HttpPost]
        public ActionResult PackingSlipListReceived()
        {
            ////var query = dbax.SCM_D365ImporForm_ProductReceipt.GroupBy(g => g.ProductReceipt).ToList();
            //var spl = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ItemGroup == "MachineP");
            //var CountRow = dbax.SCM_D365ImporForm_ProductReceipt.Where(w => w.ItemGroup == "MachineP").Count();
            string[] itemGroup = { "MachineP", "Tooling" };
            var result = dbsp.SCM_Sparepart_Product_Received.GroupBy(item => item.ProductReceipt)
                 .Select(grouping => grouping.FirstOrDefault())
                 .Where(w => itemGroup.Contains(w.ItemGroup))
                 .OrderByDescending(item => item.DateReceived)
                 .ToList();

            var query = (from t in dbsp.SCM_Sparepart_Product_Received.Where(w => itemGroup.Contains(w.ItemGroup))
                         group t by new { t.ProductReceipt, t.ItemGroup, t.Site, t.DateReceived }
             into grp
                         select new
                         {
                             ProductReceipt = grp.Key.ProductReceipt,
                             ItemGroup = grp.Key.ItemGroup,
                             Site = grp.Key.Site,
                             DateReceived = grp.Key.DateReceived,
                             TotalItem = grp.Count()
                         }).ToList();
            var countResults = query.Count();

            List<Tbl_SCM_D365ImporForm_ProductReceipt> actions = new List<Tbl_SCM_D365ImporForm_ProductReceipt>();
            var no = 0;
            foreach (var Item in result)
            {

                no++;
                var UrlAction = Url.Action("PackingSlipDetail", "Sparepart", new { area = "SCM", ProductReceipt = Item.ProductReceipt });
                actions.Add(
                    new Tbl_SCM_D365ImporForm_ProductReceipt
                    {
                        No = no,
                        ProductReceipt = "<a href='" + UrlAction + "'  >" + Item.ProductReceipt + "</a>",
                        ItemGroup = Item.ItemGroup,
                        Site = Item.Site,
                        DateReceived = Item.DateReceived
                    });
            }

            return Json(new
            {
                rows = actions,
                //totalNotFiltered = query,
                result = result,
                total = countResults,
            }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult GetDetailItem(string itemid)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();

            ViewBag.CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            ViewBag.UserList = db.V_Users_Active.Where(w => w.Status == "Permanent").ToList();


            var itemDetail = dbsp.V_SCM_Sparepart_Master_List.Where(w => w.ITEMID == itemid).FirstOrDefault();


            return Json(itemDetail, JsonRequestBehavior.AllowGet);
        }
        private class RackTableParms
        {
            public int No { get; set; }
            public Int64 RackId { get; set; }
            public string RackName { get; set; }
            public string RackLocation { get; set; }
            public string edtiButton { get; set; }
        }

        [HttpPost]
        public ActionResult RackList()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            var query = dbsp.SCM_Sparepart_Rack.Where(w => w.IsDelete == 0).OrderByDescending(x => x.CreateTime).ToList();
            var CountRow = query.Count();

            //int i = 0;

            List<RackTableParms> par = new List<RackTableParms>();
            //var query2 =
            //   (from a in db.SCM_Sparepart_Rack
            //    select new { r = i++, RackId = a.RackId, RackName = a.RackName, RackLocation = a.RackLocation, Action = "", IsDelete = a.IsDelete }).Where(w => w.IsDelete == "0").ToList() ;

            int no = 0;
            foreach (var Item in query)
            {
                no++;
                par.Add(new RackTableParms

                {
                    No = no,
                    RackId = Item.RackId,
                    RackName = Item.RackName,
                    RackLocation = Item.RackLocation,
                    edtiButton = "<a href='#EditRackModal' data-toggle='modal' data-id='" + Item.RackId + "' class='btn btn-warning btn-sm' id='BtnEditRack'><i class='fa fa-edit'></i></a> &nbsp; <a href='#' data-toggle='modal' data-id='" + Item.RackId + "' class='btn btn-danger btn-sm' id='BtnDeleteRack'><i class='fa fa-trash'></i></a> "
                });
            }



            //}

            //var ReqList = db.SCM_Sparepart_Request_Temp_Views.SqlQuery("SELECT a.ITEMID, a.userRequest , a.quantity, b.ProductName from SCM_Sparepart_Request_Temp a LEFT JOIN V_AXItemMaster b ON b.ITEMID = a.ITEMID WHERE a.userRequest = {0}", CurrUser.NIK).ToList();

            //var ReqList = db.SCM_Sparepart_Request_Temp.Where(w => w.userRequest == CurrUser.NIK).ToList();

            return Json(new
            {
                rows = par,
                totalNotFiltered = CountRow,
                total = CountRow
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddRack(SCM_Sparepart_Rack smodel)
        {

            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            SCM_Sparepart_Rack par = new SCM_Sparepart_Rack();

            par.RackName = smodel.RackName;
            par.RackLocation = smodel.RackLocation;
            par.IsDelete = 0;
            par.CreateTime = DateTime.Now;

            dbsp.SCM_Sparepart_Rack.Add(par);

            var ins = dbsp.SaveChanges();
            if (ins == 1)
            {
                return Json(new
                {
                    status = "1",
                    msg = "Save Success"
                });
            }
            else
            {
                return Json(new
                {
                    status = "0",
                    msg = "failed Save Form"
                });
            }


        }

        [HttpGet]
        public ActionResult EditRack(byte RackId)
        {
            var query = dbsp.SCM_Sparepart_Rack.Where(w => w.RackId == RackId).OrderByDescending(x => x.CreateTime).FirstOrDefault();
            return Json(new { item = query }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditRack(byte RackId, string RackName, string RackLocation)
        {
            var query = dbsp.SCM_Sparepart_Rack.Where(w => w.RackId == RackId).ToList();
            foreach (var data in query)
            {
                data.RackName = RackName;
                data.RackLocation = RackLocation;
            }

            var update = dbsp.SaveChanges();

            if (update > 0)
            {
                return Json(new { status = '1', msg = "Update Success" });
            }
            else
            {
                return Json(new { status = '0', msg = "Update Failed" });
            }
        }

        [HttpPost]
        public ActionResult RemoveRack(byte RackId)
        {
            var query = dbsp.SCM_Sparepart_Rack.Where(w => w.RackId == RackId).FirstOrDefault();


            var delete = dbsp.SCM_Sparepart_Rack.Remove(query);
            var result = dbsp.SaveChanges();
            if (result > 0)
            {
                return Json(new { status = '1', msg = "Delete Success" });
            }
            else
            {
                return Json(new { status = '0', msg = "Delete Failed" });
            }
        }

        [HttpPost]
        public ActionResult StockInAddItemTemp(SCM_Sparepart_Stock_In_Temp smodel)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();

            ViewBag.CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            ViewBag.UserList = db.V_Users_Active.Where(w => w.Status == "Permanent").ToList();
            ViewBag.UserList = db.V_Users_Active.Where(w => w.Status == "Permanent").ToList();


            dbsp.SCM_Sparepart_Stock_In_Temp.Add(smodel);
            var ins = dbsp.SaveChanges();
            if (ins > 0)
            {
                return Json(new { status = 1, msg = "Item Inserted" });
            }
            else
            {
                return Json(new { status = 0, msg = "Failed Insert Item" });
            }
        }

        [HttpPost]
        public ActionResult StockInItemTemp()
        {
            //var spl = db.V_SCM_Sparepart_Request_Detail.Where(w => w.RequestNo == RequestNo).ToList();
            //var CountRow = db.V_SCM_Sparepart_Request_Detail.Count(w => w.RequestNo == RequestNo);

            //var id = 1;
            var query =
               (from a in dbsp.SCM_Sparepart_Stock_In_Temp
                join b in dbsp.V_SCM_Sparepart_Master_List on a.ITEMID equals b.ITEMID
                select new { itemID = a.ITEMID, ProductName = b.ProductName, ProductCategory = b.ProductCategory, ItemGroup = b.ItemGroup, QtyIn = a.QtyIn, Action = "" }).ToList();

            var CountRow = query.Count();
            // var countNotReady = db.V_SCM_Sparepart_Request_Detail.Count(w => w.RequestNo == RequestNo && w.IsReady == 0);

            //foreach (var Item in spl)
            //{
            //    var Tools = "";
            //    if (Item.IsReady == 0)
            //    {
            //        Tools = "<a href=\"#\" title=\"ready\" id=\"procItem\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-success procItem\"><i class=\"fa fa-check\"></i></a>";
            //    }
            //    else
            //    {
            //        Tools = "<a href=\"#EditQuantityModal\" data-toggle=\"modal\" title=\"Decrease Qty\" id=\"editQty\"  data-id=\"" + Item.Id + "\" class=\"btn-sm btn-warning procItem\"><i class=\"fa fa-pencil btnEdit\"></i></a>";
            //    }

            //    actions.Add(
            //        new V_SCM_Sparepart_ItemList
            //        {
            //            ITEMID = Item.ITEMID,
            //            ProductName = Item.ProductName,
            //            Quantity = Item.Quantity,
            //            Qty_Realization = Item.Qty_Realization,
            //            Tools = Tools
            //        });

            //}

            return Json(new
            {
                rows = query,
                totalNotFiltered = CountRow,
                total = CountRow
            }, JsonRequestBehavior.AllowGet);
        }


        // POST: SCM/Sparepart/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        public void  EmailTest()
        {
            string FilePath = Path.Combine(Server.MapPath("~/Emails/SCM/Sparepart/"), "Test.html");
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            //Repalce [newusername] = signup user name   
            MailText = MailText.Replace("##UserName##", "863.09.22");
            MailText = MailText.Replace("##Dept##", "Information Technology");
            MailText = MailText.Replace("##Url##", "https://portal.ngkbusi.com");

            var senderEmail = new MailAddress("ngkportal-notification@ngkbusi.com", "Sparepart Apps");
            var receiverEmail = new MailAddress("ikhsan.sholihin@ngkbusi.com", "Receiver");
            var password = "100%NGKbusi!";
            var sub = "[no-reply] Sparepart Apps";
            var body = MailText;
            var smtp = new SmtpClient
            {
                Host = "ngkbusi.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,

                Credentials = new NetworkCredential(senderEmail.Address, password)
            };
            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = sub,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(mess);
            }
            //return Json(new
            //{
            //    status = '1',
            //    msg = "Request ready to pickup"
            //});
        }

        public static void SendEmail()
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                MailAddress fromAddress = new MailAddress("ngkportal-notification@ngkbusi.com");
                mailMessage.From = fromAddress;
                mailMessage.To.Add("ikhsan.sholihin@ngkbusi.com");
                mailMessage.Body = "This is Testing Email Without Configured SMTP Server";
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = " Testing Email";
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = "localhost";
                smtpClient.Send(mailMessage);
            }
            catch (Exception)
            {
            }
        }
    }
}
