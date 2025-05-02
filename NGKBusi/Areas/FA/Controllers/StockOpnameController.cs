using NGKBusi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NGKBusi.Areas.SCM.Models;

namespace NGKBusi.Areas.FA.Controllers
{
    public class StockOpnameController : Controller
    {
        DefaultConnection db = new DefaultConnection();
        SparepartConnection dbSP = new SparepartConnection();
        // GET: FA/StockOpname

        [Authorize]
        public ActionResult Reconcile()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();
            var coll = (from usr in db.Users.DefaultIfEmpty()
                        from rol in usr.Users_Menus_Roles.DefaultIfEmpty()
                        where usr.NIK == currUserID && rol.menuID == 9
                        select new { usr, rol })
                                .AsEnumerable().Select(s => s.usr);
            if (coll.FirstOrDefault() == null)
            {
                return View("UnAuthorized");
            }

            ViewBag.SOList = db.V_StockOpname_Reconcile.ToList();

            return View();
        }

        [Authorize]
        public ActionResult Counting()
        {
            //var period = Int32.Parse(Request["iPeriod"]);
            ViewBag.NavHide = true;
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();
            var coll = (from usr in db.Users.DefaultIfEmpty()
                        from rol in usr.Users_Menus_Roles.DefaultIfEmpty()
                        where usr.NIK == currUserID && rol.menuID == 9
                        select new { usr, rol })
                                .AsEnumerable().Select(s => s.usr);
            if (coll.FirstOrDefault() == null)
            {
                return View("UnAuthorized");
            }

            var warehouse = new string[] { "PD", "HD", "TWH", "WH-KD", "WH-FG" };
            //var warehouse = new string[] { "MP", "TL", "WH-KD", "WH-FG", "PD", "HD", "TWH" };

            ViewBag.Warehouse = warehouse;

            return View();
        }

        [Authorize]
        public ActionResult CountingList()
        {
            Int32 period = (Request["iPeriod"] != null ? Int32.Parse(Request["iPeriod"]) : DateTime.Now.Year);
            ViewBag.NavHide = true;
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();
            var coll = (from usr in db.Users.DefaultIfEmpty()
                        from rol in usr.Users_Menus_Roles.DefaultIfEmpty()
                        where usr.NIK == currUserID && rol.menuID == 9
                        select new { usr, rol })
                                .AsEnumerable().Select(s => s.usr);
            if (coll.FirstOrDefault() == null)
            {
                return View("UnAuthorized");
            }

            var warehouse = new string[] { "PD", "HD", "TWH", "WH-KD", "WH-FG" };
            //var warehouse = new string[] { "MP", "TL", "WH-KD", "WH-FG", "PD", "HD", "TWH" };

            ViewBag.CountingList = db.FA_StockOpname_StockTake.Where(w => w.Created_By == currUserID && w.Created_At.Value.Year == period && warehouse.Contains(w.Warehouse)).OrderByDescending(o => o.ID).ToList();
            //ViewBag.CountingList = db.FA_StockOpname_StockTake.Where(w => w.Created_By == currUserID && w.Created_At.Value.Year == period && warehouse.Contains(w.Warehouse) && !w.ItemID.StartsWith("QID")).OrderByDescending(o => o.ID).ToList();
            ViewBag.Warehouse = warehouse;

            return View();
        }

        [Authorize]
        public ActionResult SampleList()
        {
            Int32 period = (Request["iPeriod"] != null ? Int32.Parse(Request["iPeriod"]) : DateTime.Now.Year);
            ViewBag.NavHide = true;
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();
            var coll = (from usr in db.Users.DefaultIfEmpty()
                        from rol in usr.Users_Menus_Roles.DefaultIfEmpty()
                        where usr.NIK == currUserID && rol.menuID == 9
                        select new { usr, rol })
                                .AsEnumerable().Select(s => s.usr);
            if (coll.FirstOrDefault() == null)
            {
                return View("UnAuthorized");
            }

            ViewBag.SampleList = db.V_StockOpname_Sample.Where(w => w.PIC == currUserID).OrderByDescending(o => o.Status).ToList();


            var warehouse = new string[] { "PD", "HD", "TWH", "WH-KD", "WH-FG" };
            //var warehouse = new string[] { "MP", "TL", "WH-KD", "WH-FG", "PD", "HD", "TWH" };

            ViewBag.Warehouse = warehouse;
            return View();
        }
        public JsonResult CountingGetProduct()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();
            var iWarehouse = Request["iWarehouse"];
            //var sectionArray = db.FA_StockOpname_Section.Where(w => w.NIK == currUserID && w.Section != null).Select(s => s.Section);
            //var getData = db.V_AXBegInventory.Where(w => w.Warehouse == iWarehouse).Select(s => s.AX_SearchName).Distinct();
            var period = "FY1" + (Int32.Parse(DateTime.Now.Year.ToString().Substring(DateTime.Now.Year.ToString().Length - 2, 2)));
            var getData = db.FA_StockOpname_EndInventory.Where(w => w.Warehouse == iWarehouse && w.Period == period).Select(s => s.AX_SearchName).Distinct();
            //var getData = db.FA_StockOpname_EndInventory.Where(w => w.Warehouse == iWarehouse && w.Period == period).Select(s => s.AX_SearchName).Distinct();

            //if (sectionArray.Count() > 0)
            //{
            //    var getSearchName = db.V_AXItemMaster.Where(w => sectionArray.Contains(w.Section)).Select(s => s.SearchName);
            //    getData = db.FA_StockOpname_EndInventory.Where(w => w.Warehouse == iWarehouse && w.Period == period && getSearchName.Contains(w.AX_SearchName)).Select(s => s.AX_SearchName).Distinct();
            //    //getData = db.FA_StockOpname_EndInventory.Where(w => w.Warehouse == iWarehouse && w.Period == period && !w.AX_ItemID.StartsWith("QID")).Select(s => s.AX_SearchName).Distinct();
            //    //getData = db.V_AXItemMaster.Select(s => s.SearchName).Distinct();
            //}
            if (iWarehouse == "MP")
            {
                getData = db.V_AXBegInventory.Where(w => w.Warehouse == iWarehouse && w.AX_SearchName == "MACHINERY PART").Select(s => s.AX_SearchName).Distinct();
            }
            else if (iWarehouse == "TL")
            {
                getData = db.V_AXBegInventory.Where(w => w.Warehouse == iWarehouse && w.AX_SearchName == "TOOLING").Select(s => s.AX_SearchName).Distinct();
            }
            return Json(getData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult CountingGetItemID()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();

            var period = "FY1" + (Int32.Parse(DateTime.Now.Year.ToString().Substring(DateTime.Now.Year.ToString().Length - 2, 2)));
            var iIgnore = bool.Parse(Request["iIgnore"]);
            var iProduct = Request["iProduct"];
            var iItem = Request["iItemID"];
            var iWarehouse = Request["iWarehouse"];
            String[] productArray = { "MACHINERY PART", "TOOLING", "PROMOTION GOOD" };
            String[] itemGroup = { "FG-M", "Packaging", "FG-NGK-T" };
            String[] qid = { "QIDPS", "QIDMP", "QIDTL" };
            if (iIgnore == false)
            {
                //var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && w.SearchName == iProduct && !w.ITEMID.StartsWith("QID")).OrderBy(o => o.ITEMID).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(75);
                var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && w.SearchName == iProduct && qid.Contains(w.ITEMID.Substring(0, 5))).OrderBy(o => o.ITEMID).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(75);
                return Json(getData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //var getEndingItemID = db.FA_StockOpname_EndInventory.Where(w => w.Period == period && w.Warehouse == iWarehouse).Select(s => s.AX_ItemID);
                //var getEndingItemID = db.FA_StockOpname_EndInventory.Where(w => w.Period == period && productArray.Contains(w.AX_SearchName)).Select(s => s.AX_ItemID);    
                //var sectionArray = db.FA_StockOpname_Section.Where(w => w.NIK == currUserID && w.Section != null).Select(s => s.Section);
                //if (sectionArray.Count() > 0)
                //{
                //    //var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && getEndingItemID.Contains(w.ITEMID) && sectionArray.Contains(w.Section)).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(75);
                //    //var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && getEndingItemID.Contains(w.ITEMID) && !w.ITEMID.StartsWith("QID")).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(75);
                //    var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && getEndingItemID.Contains(w.ITEMID)).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(75);
                //    return Json(getData, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && getEndingItemID.Contains(w.ITEMID) && !w.ITEMID.StartsWith("QID")).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(75);
                //var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && getEndingItemID.Contains(w.ITEMID)).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(75);
                if (iWarehouse == "WH-FG")
                {
                    var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && !productArray.Contains(w.SearchName) && !qid.Contains(w.ITEMID.Substring(0, 5)) && itemGroup.Contains(w.ItemGroup)).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(50);
                    return Json(getData, JsonRequestBehavior.AllowGet);
                }else if(iWarehouse == "TWH")
                {
                    var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && !productArray.Contains(w.SearchName) && !qid.Contains(w.ITEMID.Substring(0, 5)) && w.ItemGroup== "FG-M").Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(50);
                    return Json(getData, JsonRequestBehavior.AllowGet);
                }
                else if (iWarehouse == "MP" || iWarehouse == "TL")
                {
                    var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && qid.Contains(w.ITEMID.Substring(0, 5))).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(50);
                    return Json(getData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && !productArray.Contains(w.SearchName) && !qid.Contains(w.ITEMID.Substring(0, 5))).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(50);
                    return Json(getData, JsonRequestBehavior.AllowGet);
                }
                //}
            }
        }
        [Authorize]
        public JsonResult CountingGetItemIDQR()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();

            var period = "FY1" + (Int32.Parse(DateTime.Now.Year.ToString().Substring(DateTime.Now.Year.ToString().Length - 2, 2)));
            var iItem = Request["iItemID"];
            var iWarehouse = Request["iWarehouse"];

            if(iWarehouse=="MP" || iWarehouse == "TL")
            {
                var getData = dbSP.V_SCM_Sparepart_Master_List.Where(w => w.ITEMID == iItem).Select(s => new { label = s.ITEMID + " || " + s.ProductName, value = s.ITEMID + " || " + s.ProductName, itemID = s.ITEMID, itemName = s.ProductName, unit = "PC", product = s.ProductName, itemGroup = s.ItemGroup, stock = s.Stock });
                return Json(getData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var getData = db.V_AXItemMaster.Where(w => w.ITEMID == iItem).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup });
                return Json(getData, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize]
        public JsonResult CountingGetItemIDBarcode()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();

            var iItem = Request["iItemID"];
            String[] itemGroup = { "FG-M", "Packaging", "FG-NGK-T" };

            var getData = db.V_AXItemMaster.Where(w => (w.ITEMID == iItem || w.ItemDescription == iItem || w.ProductName == iItem || w.SearchName == iItem || w.SearchNameAll == iItem) && itemGroup.Contains(w.ItemGroup)).Select(s => new { label = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, value = s.ITEMID + " || " + s.ProductName + " || " + s.SearchName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitInventory, product = s.SearchName, itemGroup = s.ItemGroup });
            return Json(getData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult CountingGetLot()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();

            var period = "FY1" + (Int32.Parse(DateTime.Now.Year.ToString().Substring(DateTime.Now.Year.ToString().Length - 2, 2)));
            var iItem = Request["iItemID"];
            var iLot = Request["iLot"];
            var iWarehouse = Request["iWarehouse"];


            var getData = db.FA_StockOpname_EndInventory.Where(w => w.Period == period && w.AX_ItemID == iItem && w.Warehouse == iWarehouse && w.Batch_Number.Contains(iLot)).Select(s => new { label = s.Batch_Number, value = s.Batch_Number }).Distinct().Take(75);

            return Json(getData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult CountingDelete()
        {
            var iID = Int32.Parse(Request["iID"]);
            var getData = db.FA_StockOpname_StockTake.Where(w => w.ID == iID).FirstOrDefault();
            db.FA_StockOpname_StockTake.Remove(getData);
            db.SaveChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult CountingAdd()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();
            var period = Request["iPeriod"];
            var warehouse = Request["iWarehouse"];
            var product = Request["iProduct"];
            var itemID = Request["iItemID"];
            var itemName = Request["iItemName"];
            var unit = Request["iUnit"];
            var lot = Request["iLot"];
            var qty = decimal.Parse(Request["iQty"]);
            var description = Request["iDescription"];
            var newData = new FA_StockOpname_StockTake();
            newData.Period = period;
            newData.Warehouse = warehouse;
            newData.Product = product;
            newData.ItemID = itemID;
            newData.ItemName = itemName;
            newData.Unit = unit;
            newData.Qty = qty;
            newData.Batch_Number = lot;
            newData.Description = description;
            newData.Created_At = DateTime.Now;
            newData.Created_By = currUserID;
            db.FA_StockOpname_StockTake.Add(newData);
            db.SaveChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CountingListAdd()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();
            var ID = Int32.Parse(Request["iID"]);
            var warehouse = Request["iWarehouse"];
            var product = Request["iProduct"];
            var itemID = Request["iItemID"];
            var itemName = Request["iItemName"];
            var unit = Request["iUnit"];
            var lot = Request["iLot"];
            var qty = decimal.Parse(Request["iQty"]);
            var description = Request["iDescription"];
            var editData = db.FA_StockOpname_StockTake.Where(w => w.ID == ID).FirstOrDefault();
            editData.Warehouse = warehouse;
            editData.Product = product;
            editData.ItemID = itemID;
            editData.ItemName = itemName;
            editData.Unit = unit;
            editData.Qty = qty;
            editData.Batch_Number = lot;
            editData.Description = description;
            editData.Updated_At = DateTime.Now;
            editData.Updated_By = currUserID;
            db.SaveChanges();

            return RedirectToAction("CountingList", "StockOpname", new { area = "FA" });
        }

        [Authorize]
        public ActionResult SampleListAdd()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId();
            var period = Request["iPeriod"];
            var warehouse = Request["iWarehouse"];
            var product = Request["iProduct"];
            var itemID = Request["iItemID"];
            var itemName = Request["iItemName"];
            var lot = Request["iLot"];
            var unit = Request["iUnit"];
            var qty = decimal.Parse(Request["iQty"]);
            var description = Request["iDescription"];
            var newData = new FA_StockOpname_StockTake();
            newData.Period = "FY124";
            newData.Warehouse = warehouse.Trim();
            newData.Product = product.Trim();
            newData.ItemID = itemID.Trim();
            newData.ItemName = itemName;
            newData.Batch_Number = lot;
            newData.Unit = "PC";
            newData.Qty = qty;
            newData.Description = description;
            newData.Created_At = DateTime.Now;
            newData.Created_By = currUserID;
            db.FA_StockOpname_StockTake.Add(newData);
            db.SaveChanges();

            return RedirectToAction("SampleList", "StockOpname", new { area = "FA" });
        }

    }
}