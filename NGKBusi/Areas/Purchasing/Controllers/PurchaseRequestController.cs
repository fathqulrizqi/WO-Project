using NGKBusi.Models;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Web.Mvc;
using NGKBusi.Areas.Purchasing.Models;
using System.IO;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Data.OleDb;
using Newtonsoft.Json;

namespace NGKBusi.Areas.Purchasing.Controllers
{
    [Authorize]
    public class PurchaseRequestController : Controller
    {
        DefaultConnection db = new DefaultConnection();
        PurchaseRequestConnection dbPR = new PurchaseRequestConnection();
        // GET: Purchasing/PurchaseRequest
        public ActionResult PRequest(String ReqNumber)
        {
            ViewBag.NavHide = true;
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            var currYear = Request["iPRFilterYear"] ?? DateTime.Now.Year.ToString();
            var currStatus = Request["iPRFilterStatus"] ?? "Open";
            var currUserLevel = db.Approval_Master.Where(w => w.User_NIK == currUserID && w.Menu_Id == 40 && w.Document_Id == 1).FirstOrDefault();
            var currUserLevels = db.Approval_Master.Where(w => w.User_NIK == currUserID && w.Menu_Id == 40 && w.Document_Id == 1).ToList();
            var currFilterLevel = Request["iPRFilterLevel"] != null && Request["iPRFilterLevel"] != "" ? int.Parse(Request["iPRFilterLevel"].Split('|')[0]) : currUserLevel?.Levels;
            var currFilterLevelSub = Request["iPRFilterLevel"] != null && Request["iPRFilterLevel"] != "" ? int.Parse(Request["iPRFilterLevel"].Split('|')[1]) : currUserLevel?.Levels_Sub;
            var currUserSection = currUserLevels.Select(s => s.Dept_Code + " | " + s.Dept_Name);
            var currApprove = db.Approval_Master.Where(w => w.Menu_Id == 40 && w.Document_Id == 1 && w.User_NIK == currUserID);
            var currApproval = currApprove.Where(w => w.Levels == currFilterLevel && w.Levels_Sub == currFilterLevelSub).FirstOrDefault();
            var currApprovalList = db.Approval_Master.Where(w => w.Menu_Id == 40 && w.Document_Id == 1 && w.User_NIK == currUserID && w.Levels == currFilterLevel && w.Levels_Sub == currFilterLevelSub).Select(s => s.Dept_Code).Distinct().ToList();
            var currPR = dbPR.Purchasing_PurchaseRequest_Header.Where(w => w.ReqNumber == ReqNumber).FirstOrDefault();
            var currCOAItemGroup = db.AX_COA_to_ItemGroup.Select(s => s.COA_Code).ToList();
            if (currUserLevels.Count() == 0)
            {
                return View("UnAuthorized");
            }
            var currUserSectionCode = currUserLevel.Dept_Code;
            var currUserSectionCodes = currUserLevels.Select(s => s.Dept_Code).ToList();
            if (currPR != null)
            {
                currUserSectionCode = currPR.Section.Split('|')[0].Trim();
            }

            ViewBag.currFilterYear = currYear;
            ViewBag.currFilterStatus = currStatus;
            ViewBag.UserLevel = currFilterLevel != null ? currFilterLevel : currUserLevel.Levels;
            ViewBag.UserLevelSub = currFilterLevelSub != null ? currFilterLevelSub : currUserLevel.Levels_Sub;
            ViewBag.SectionList = currUserLevels;
            ViewBag.currUserLevels = currUserLevels;
            ViewBag.Units = db.V_AXItemMaster.Where(w => w.UnitPurchase != null).OrderBy(o => o.UnitPurchase).Select(s => s.UnitPurchase).Distinct();
            ViewBag.BudgetList = db.V_FA_Payment_Request_Budget_List.Where(w => w.Period_FY == "FY126" && currUserSectionCodes.Contains(w.Section_From_Code) && (w.Budget_Type == "BEX" || w.Budget_Type == "BIP" || w.Budget_Type == "CIP") && currCOAItemGroup.Contains(w.COA_Code)).ToList();
            ViewBag.ItemGroup = db.V_AXItemMaster.Where(w => w.ItemGroup != null).Select(s => s.ItemGroup).Distinct().OrderBy(o => o);
            ViewBag.PurchaseRequest = currPR;
            ViewBag.PurchaseRequestLine = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.ReqNumber == ReqNumber).ToList();
            ViewBag.Section = db.AX_Section.ToList();
            ViewBag.CurrApproval = currApproval;

            if (currStatus == "Open")
            {
                ViewBag.PurchaseRequestList = dbPR.Purchasing_PurchaseRequest_Header.Where(w => currUserSection.Contains(w.Section) && w.Created_At.Year.ToString() == currYear && w.Approval == currFilterLevel && w.Approval_Sub == currFilterLevelSub && w.Is_Reject == false).OrderByDescending(o => o.ID).ToList();
            }
            else if (currStatus == "Signed")
            {
                ViewBag.PurchaseRequestList = dbPR.Purchasing_PurchaseRequest_Header.Where(w => currUserSection.Contains(w.Section) && w.Created_At.Year.ToString() == currYear && (w.Approval > currFilterLevel || (w.Approval == currFilterLevel && w.Approval_Sub > currFilterLevelSub))).OrderByDescending(o => o.ID).ToList();
            }
            else
            {
                ViewBag.PurchaseRequestList = dbPR.Purchasing_PurchaseRequest_Header.Where(w => currUserSection.Contains(w.Section) && w.Created_At.Year.ToString() == currYear).OrderByDescending(o => o.ID).ToList();
            }

            return View();
        }

        public JsonResult GetItemID()
        {
            var iItemGroup = Request["iItemGroup"];
            var iItem = Request["iItemID"];

            var getData = db.V_AXItemMaster.Where(w => (w.ITEMID.Contains(iItem) || w.ProductName.Contains(iItem)) && w.ItemGroup == iItemGroup).Select(s => new { label = s.ITEMID + " || " + s.ProductName, value = s.ITEMID + " || " + s.ProductName, itemID = s.ITEMID, itemName = s.ProductName, unit = s.UnitPurchase, product = s.SearchName, itemGroup = s.ItemGroup }).Distinct().Take(35);
            return Json(getData, JsonRequestBehavior.AllowGet);

        }

        public JsonResult getSignList()
        {
            var iQuoNumber = Request["iQuoNumber"];
            var iLevel = int.Parse(Request["iLevel"]);
            var iLevelSub = int.Parse(Request["iLevelSub"]);

            var getData = db.Approval_List.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.Reveral_ID == iQuoNumber && w.Levels == iLevel && w.Levels_Sub == iLevelSub && w.Is_Skip == false).ToList();
            return Json(getData, JsonRequestBehavior.AllowGet);

        }
        public IQueryable<string> getPRQuotation(string reqNumber)
        {
            var _reqNumber = reqNumber;
            var getData = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.ReqNumber == _reqNumber && w.QuoNumber != null).Select(s => s.QuoNumber).Distinct();

            return getData;
        }
        public ActionResult setSignList()
        {
            int[] iSkipSignID = Array.ConvertAll(Request.Form.GetValues("iSkipSignID[]"), s => int.Parse(s));
            db.Approval_List.Where(w => iSkipSignID.Contains(w.id)).ToList().ForEach(s => { s.Is_Skip = true; });
            db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());

        }
        public ActionResult resetSignList()
        {
            var iQuoNumber = Request["iQuoNumber"];
            var resetData = db.Approval_List.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.Reveral_ID == iQuoNumber).ToList();
            if (resetData.Count > 0)
            {
                resetData.ForEach(s => { s.Is_Skip = false; });
            }
            else
            {
                var currQuotation = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.QuoNumber == iQuoNumber).FirstOrDefault();
                var getApprovalList = db.Approval_Master.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.Dept_Code == currQuotation.Section.Trim().Substring(0, 5)).ToList();
                foreach (var dList in getApprovalList)
                {
                    var newApprovalList = new Approval_List();
                    newApprovalList.Reveral_ID = iQuoNumber;
                    newApprovalList.Menu_Id = dList.Menu_Id;
                    newApprovalList.Document_Id = dList.Document_Id;
                    newApprovalList.User_NIK = dList.User_NIK;
                    newApprovalList.Dept_Code = dList.Dept_Code;
                    newApprovalList.Dept_Name = dList.Dept_Name;
                    newApprovalList.Title = dList.Title;
                    newApprovalList.Header = dList.Header;
                    newApprovalList.Label = dList.Label;
                    newApprovalList.Levels = dList.Levels;
                    newApprovalList.Levels_Sub = dList.Levels_Sub;
                    newApprovalList.Is_Skip = false;
                    db.Approval_List.Add(newApprovalList);
                }
            }

            db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());

        }
        public JsonResult GetRemainingBudget()
        {
            var iCategory = Request["iCategory"] != null ? Request["iCategory"] : "Purchase Order";
            var iBudgetNumber = Request["iBudgetNumber"];
            var iBudgetName = Request["iBudgetName"];
            var budget = iBudgetNumber + "|" + iBudgetName;
            var iQuoID = int.Parse(Request["iQuoID"]);
            var getPrevBudgetUsage = (double?)0;
            var getEVoucherBudgetUsage = (double?)0;
            var getTotalBudgetUsage = (double?)0;
            var currQuotationHeader = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.ID == iQuoID).FirstOrDefault();
            var getBudgetLines = dbPR.Purchasing_PurchaseRequest_Quotation_Line.Where(w => w.Budget_Number == budget && w.Headers.Approval > 1 && w.Headers.ID < iQuoID && w.Headers.Is_Reject == false).Select(s => s.ID).ToList();
            if (iQuoID == 0)
            {
                getBudgetLines = dbPR.Purchasing_PurchaseRequest_Quotation_Line.Where(w => w.Budget_Number == budget && w.Headers.Approval > 1 && w.Headers.Is_Reject == false).Select(s => s.ID).ToList();
            }
            if (getBudgetLines != null && getBudgetLines.Count() > 0)
            {
                getPrevBudgetUsage = dbPR.Purchasing_PurchaseRequest_Quotation_Line_Vendor.Where(w => getBudgetLines.Contains(w.Line_ID) && w.IsChoosen == true).Sum(s => s.Total);
            }
            if (iCategory != "Working Order")
            {
                var headerCreatedAt = currQuotationHeader != null ? currQuotationHeader.Created_At : DateTime.Now;
                // 1851104 => exclude VAT
                var getEvoucherNonPOUsage = db.FA_Payment_Request_Non_PO_Sub.Where(w => w.Budget_Number == iBudgetNumber && w.COA_Code != "1851104" && w.Non_PO.Approval + w.Non_PO.Approval_Sub > 1 && w.Non_PO.Created_At <= headerCreatedAt && w.Non_PO.Is_Working_Order == false)?.Sum(s => (double?)s.Allocation_Amount) ?? (double)0;
                var getEvoucherNonPaymentUsage = db.FA_Payment_Request_Non_Payment_Sub.Where(w => w.Budget_Number == iBudgetNumber && w.COA_Code != "1851104" && w.Non_PO.Approval + w.Non_PO.Approval_Sub > 1 && w.Non_PO.Created_At <= headerCreatedAt)?.Sum(s => (double?)s.Allocation_Amount) ?? (double)0;
                getEVoucherBudgetUsage = getEvoucherNonPOUsage + getEvoucherNonPaymentUsage;
            }
            getTotalBudgetUsage = getPrevBudgetUsage + getEVoucherBudgetUsage;
            //V_Budget_Usage_CutOff
            var getData = dbPR.V_Budget_Usage_CutOff.Where(w => w.Budget_Number == iBudgetNumber).FirstOrDefault();
            if (getData != null)
            {
                getData.Usage += (double)getTotalBudgetUsage;
            }
            else
            {
                getData = new V_Budget_Usage_CutOff();
            }

            return Json(getData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetThirdParty()
        {
            var iThirdParty = Request["iThirdParty"];
            var getData = db.V_AXVendorList.Where(w => (w.ACCOUNTNUM.Contains(iThirdParty) || w.Name.Contains(iThirdParty))).Select(s => new { label = s.ACCOUNTNUM + " | " + s.Name, value = s.ACCOUNTNUM + " | " + s.Name, thirdPartyID = s.ACCOUNTNUM, thirdPartyName = s.Name }).Distinct().Take(35);

            return Json(getData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RequestAdd()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            IFormatProvider culture = new CultureInfo("en-US", true);

            var newData = new Purchasing_PurchaseRequest_Header();
            newData.ReqNumber = this.getSequence("PR");
            newData.Section = Request["iSection"];
            newData.Due_Date = DateTime.ParseExact(Request["iDueDate"], "dd-MM-yyyy", culture);
            newData.Description = Request["iDescription"];
            newData.Created_At = DateTime.Now;
            newData.Created_By = currUserID;
            newData.Approval = 1;
            newData.Approval_Sub = 0;
            newData.Is_Reject = false;
            dbPR.Purchasing_PurchaseRequest_Header.Add(newData);
            dbPR.SaveChanges();

            for (var i = 0; i <= Request.Form.GetValues("iItem[]").Count() - 1; i++)
            {
                var newDataSub = new Purchasing_PurchaseRequest_Line();
                newDataSub.ReqNumber = newData.ReqNumber;
                newDataSub.Budget_Number = Request.Form.GetValues("iBudgetNumber[]")[i];
                newDataSub.Section_To = Request.Form.GetValues("iSectionTo[]")[i];
                newDataSub.Item_Group = Request.Form.GetValues("iItemGroup[]")[i];
                newDataSub.Item_ID = Request.Form.GetValues("iItemID[]")[i];
                newDataSub.Item_Name = Request.Form.GetValues("iItemName[]")[i];
                newDataSub.Unit = Request.Form.GetValues("iUnit[]")[i];
                newDataSub.Qty = double.Parse(Request.Form.GetValues("iQty[]")[i]);
                newDataSub.Price_Estimation = double.Parse(Request.Form.GetValues("iPrice[]")[i]);
                newDataSub.Note = Request.Form.GetValues("iRemark[]")[i];
                dbPR.Purchasing_PurchaseRequest_Line.Add(newDataSub);
            }
            dbPR.SaveChanges();

            uploadAttachment(newData.ReqNumber);

            return RedirectToAction("PRequest", "PurchaseRequest", new { area = "Purchasing", ReqNumber = newData.ReqNumber, iPRFilterLevel = "1|0" });
        }

        public ActionResult RequestEdit()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            var currReqNumber = Request["iReqNumber"];
            IFormatProvider culture = new CultureInfo("en-US", true);

            var newData = dbPR.Purchasing_PurchaseRequest_Header.Where(w => w.ReqNumber == currReqNumber).FirstOrDefault();
            newData.Section = Request["iSection"];
            newData.Due_Date = DateTime.ParseExact(Request["iDueDate"], "dd-MM-yyyy", culture);
            newData.Description = Request["iDescription"];

            var delDataSub = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.ReqNumber == currReqNumber).ToList();
            dbPR.Purchasing_PurchaseRequest_Line.RemoveRange(delDataSub);
            dbPR.SaveChanges();

            for (var i = 0; i <= Request.Form.GetValues("iItem[]").Count() - 1; i++)
            {
                var newDataSub = new Purchasing_PurchaseRequest_Line();
                newDataSub.ReqNumber = newData.ReqNumber;
                newDataSub.Budget_Number = Request.Form.GetValues("iBudgetNumber[]")[i];
                newDataSub.Section_To = Request.Form.GetValues("iSectionTo[]")[i];
                newDataSub.Item_Group = Request.Form.GetValues("iItemGroup[]")[i];
                newDataSub.Item_ID = Request.Form.GetValues("iItemID[]")[i];
                newDataSub.Item_Name = Request.Form.GetValues("iItemName[]")[i];
                newDataSub.Unit = Request.Form.GetValues("iUnit[]")[i];
                newDataSub.Qty = double.Parse(Request.Form.GetValues("iQty[]")[i]);
                newDataSub.Price_Estimation = double.Parse(Request.Form.GetValues("iPrice[]")[i]);
                newDataSub.Note = Request.Form.GetValues("iRemark[]")[i];
                dbPR.Purchasing_PurchaseRequest_Line.Add(newDataSub);
            }
            dbPR.SaveChanges();

            uploadAttachment(newData.ReqNumber);
            return RedirectToAction("PRequest", "PurchaseRequest", new { area = "Purchasing", ReqNumber = newData.ReqNumber });
        }

        public ActionResult RequestDelete()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            var currReqNumber = Request["iReqNumber"];
            IFormatProvider culture = new CultureInfo("en-US", true);

            var deleteData = dbPR.Purchasing_PurchaseRequest_Header.Where(w => w.ReqNumber == currReqNumber).FirstOrDefault();
            var delDataSub = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.ReqNumber == currReqNumber).ToList();
            dbPR.Purchasing_PurchaseRequest_Line.RemoveRange(delDataSub);
            dbPR.Purchasing_PurchaseRequest_Header.Remove(deleteData);

            var deleteAttachment = dbPR.Purchasing_PurchaseRequest_Attachment.Where(w => w.ReqNumber == currReqNumber).ToList();
            foreach (var del in deleteAttachment)
            {
                deletePRAttachment(del.ID);
            }
            dbPR.Purchasing_PurchaseRequest_Attachment.RemoveRange(deleteAttachment);
            dbPR.SaveChanges();

            return RedirectToAction("PRequest", "PurchaseRequest", new { area = "Purchasing" });
        }

        public ActionResult Quotation(String QuoNumber)
        {
            ViewBag.NavHide = true;
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            var currYear = Request["iPRFilterYear"] ?? DateTime.Now.Year.ToString();
            var currStatus = Request["iPRFilterStatus"] ?? "Open";
            var currUserLevel = db.Approval_Master.Where(w => w.User_NIK == currUserID && w.Menu_Id == 40 && w.Document_Id == 2).FirstOrDefault();
            var currUserLevels = db.Approval_Master.Where(w => w.User_NIK == currUserID && w.Menu_Id == 40 && w.Document_Id == 2).ToList();
            var currFilterLevel = Request["iPRFilterLevel"] != null ? int.Parse(Request["iPRFilterLevel"].Split('|')[0]) : currUserLevel?.Levels;
            var currFilterLevelSub = Request["iPRFilterLevel"] != null ? int.Parse(Request["iPRFilterLevel"].Split('|')[1]) : currUserLevel?.Levels_Sub;
            var currUserSection = currUserLevels.Select(s => s.Dept_Code + " | " + s.Dept_Name);
            var currApprove = db.Approval_Master.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.User_NIK == currUserID);
            var currApproval = currApprove.Where(w => w.Levels == currFilterLevel && w.Levels_Sub == currFilterLevelSub).FirstOrDefault();
            var currApprovalList = db.Approval_Master.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.User_NIK == currUserID && w.Levels == currFilterLevel && w.Levels_Sub == currFilterLevelSub).Select(s => s.Dept_Code).Distinct().ToList();

            if (currUserLevels.Count() == 0)
            {
                return View("UnAuthorized");
            }
            if (Request["addNew"] != null)
            {
                var sectionList = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.QuoNumber == null && w.Headers.Approval == 2 && w.Headers.Is_Reject == false).Select(s => s.Headers.Section).Distinct();
                var section = Request["iSection"];
                var receivedBy = Request["iReceivedBy"];
                ViewBag.SectionList = sectionList;
                var OpenRequest = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.QuoNumber == null && w.Headers.Approval == 2 && w.Headers.Is_Reject == false);
                if (section != null && section != "")
                {
                    OpenRequest = OpenRequest.Where(w => w.Headers.Section == section);
                }
                if (receivedBy != null && receivedBy != "")
                {
                    OpenRequest = OpenRequest.Where(w => w.Assign_To == receivedBy);
                }
                else
                {
                    OpenRequest = OpenRequest.Where(w => w.Assign_To == null);
                }
                ViewBag.OpenRequest = OpenRequest.ToList();
            }
            string[] PurchasingMember = { "616.08.12", "626.10.12", "703.04.15", "801.08.18", "845.01.20", "816.09.18", "672.08.14", "715.06.15" };

            ViewBag.currFilterYear = currYear;
            ViewBag.currFilterStatus = currStatus;
            ViewBag.UserLevel = currFilterLevel != null ? currFilterLevel : currUserLevel.Levels;
            ViewBag.UserLevelSub = currFilterLevelSub != null ? currFilterLevelSub : currUserLevel.Levels_Sub;
            ViewBag.currUserLevels = currUserLevels;
            ViewBag.CurrApproval = currApproval;
            ViewBag.ThirdParty = db.V_AXVendorList.ToList();
            ViewBag.AssignMember = db.V_Users_Active.Where(w => (w.SectionName == "INDIRECT PROCUREMENT" || w.SectionName == "DIRECT PROCUREMENT") && w.CostName == "SUPPLY & PROCUREMENT" && w.Status != "Not Active" && PurchasingMember.Contains(w.NIK)).ToList();
            ViewBag.Quotation = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.QuoNumber == QuoNumber).FirstOrDefault();

            if (currStatus == "Open")
            {
                ViewBag.QuotationList = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.Created_At.Year.ToString() == currYear && w.Approval == currFilterLevel && w.Approval_Sub == currFilterLevelSub && currApprovalList.Contains(w.Section.Substring(0, 5)) && w.Is_Reject == false).OrderByDescending(o => o.ID).ToList();
            }
            else if (currStatus == "Signed")
            {
                ViewBag.QuotationList = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.Created_At.Year.ToString() == currYear && (w.Approval > currFilterLevel || (w.Approval == currFilterLevel && w.Approval_Sub > currFilterLevelSub)) && currApprovalList.Contains(w.Section.Substring(0, 5))).OrderByDescending(o => o.ID).ToList();
            }
            else
            {
                ViewBag.QuotationList = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.Created_At.Year.ToString() == currYear && currApprovalList.Contains(w.Section.Substring(0, 5))).OrderByDescending(o => o.ID).ToList();
            }
            return View();
        }

        public ActionResult QuotationAdd()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            IFormatProvider culture = new CultureInfo("en-US", true);

            var newData = new Purchasing_PurchaseRequest_Quotation_Header();
            newData.QuoNumber = this.getSequence("QT");
            newData.Section = Request.Form.GetValues("iSection[]")[0];
            newData.Category = "Purchase Order";
            newData.Due_Date = null;
            newData.Created_At = DateTime.Now;
            newData.Created_By = currUserID;
            newData.Approval = 1;
            newData.Approval_Sub = 0;
            newData.Is_Reject = false;
            dbPR.Purchasing_PurchaseRequest_Quotation_Header.Add(newData);
            dbPR.SaveChanges();

            int[] updateRequestLine = Array.ConvertAll(Request.Form.GetValues("iLineID[]"), s => int.Parse(s));
            dbPR.Purchasing_PurchaseRequest_Line.Where(w => updateRequestLine.Contains(w.ID)).ToList().ForEach(s => { s.QuoNumber = newData.QuoNumber; });

            for (var i = 0; i <= Request.Form.GetValues("iLineID[]").Count() - 1; i++)
            {
                var newDataSub = new Purchasing_PurchaseRequest_Quotation_Line();
                newDataSub.Sequence = i + 1;
                newDataSub.QuoNumber = newData.QuoNumber;
                newDataSub.ReqNumber = Request.Form.GetValues("iReqNumber[]")[i];
                newDataSub.Budget_Number = Request.Form.GetValues("iBudgetNumber[]")[i];
                newDataSub.Section_To = Request.Form.GetValues("iSectionTo[]")[i];
                newDataSub.ETA_Date = DateTime.ParseExact(Request.Form.GetValues("iETADate[]")[i], "dd-MM-yyyy", culture);
                newDataSub.Item_Group = Request.Form.GetValues("iItemGroup[]")[i];
                newDataSub.Item_ID = Request.Form.GetValues("iItemID[]")[i];
                newDataSub.Item_Name = Request.Form.GetValues("iItemName[]")[i];
                newDataSub.Unit = Request.Form.GetValues("iUnit[]")[i];
                newDataSub.Qty = double.Parse(Request.Form.GetValues("iQty[]")[i]);
                newDataSub.Price_Estimation = double.Parse(Request.Form.GetValues("iPrice[]")[i]);
                newDataSub.Note = Request.Form.GetValues("iNote[]")[i];
                newDataSub.Is_Reject = false;
                dbPR.Purchasing_PurchaseRequest_Quotation_Line.Add(newDataSub);
            }
            dbPR.SaveChanges();
            var getApprovalList = db.Approval_Master.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.Dept_Code == newData.Section.Trim().Substring(0, 5)).ToList();
            foreach (var dList in getApprovalList)
            {
                var newApprovalList = new Approval_List();
                newApprovalList.Reveral_ID = newData.QuoNumber;
                newApprovalList.Menu_Id = dList.Menu_Id;
                newApprovalList.Document_Id = dList.Document_Id;
                newApprovalList.User_NIK = dList.User_NIK;
                newApprovalList.Dept_Code = dList.Dept_Code;
                newApprovalList.Dept_Name = dList.Dept_Name;
                newApprovalList.Title = dList.Title;
                newApprovalList.Header = dList.Header;
                newApprovalList.Label = dList.Label;
                newApprovalList.Levels = dList.Levels;
                newApprovalList.Levels_Sub = dList.Levels_Sub;
                newApprovalList.Is_Skip = false;
                db.Approval_List.Add(newApprovalList);
            }
            db.SaveChanges();
            uploadQTAttachment(newData.QuoNumber, "Quotation");

            return RedirectToAction("Quotation", "PurchaseRequest", new { area = "Purchasing", QuoNumber = newData.QuoNumber });
        }
        public ActionResult QuotationAssignMember(string iUnAssign)
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            var currAssign = iUnAssign;
            var currAssignMember = Request["iAssignTo"].Split(',')[0];
            IFormatProvider culture = new CultureInfo("en-US", true);
            if (Request.Form.GetValues("iLineID[]") != null)
            {
                for (var i = 0; i <= Request.Form.GetValues("iLineID[]").Count() - 1; i++)
                {
                    var lineID = int.Parse(Request.Form.GetValues("iLineID[]")[i]);
                    var newDataSub = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.ID == lineID).FirstOrDefault();
                    newDataSub.Assign_To = (currAssign == "True" ? null : currAssignMember);
                }
                dbPR.SaveChanges();
            }

            return RedirectToAction("Quotation", "PurchaseRequest", new { area = "Purchasing", addNew = "addNew" });
        }
        public ActionResult InputQuotationPO()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            IFormatProvider culture = new CultureInfo("en-US", true);
            var currQuoNumber = Request["iQuoNumber"];
            var currETDDate = Request["iETDDate"];
            var currPONumber = Request["iPONo"];
            var editData = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.QuoNumber == currQuoNumber).FirstOrDefault();
            editData.Due_Date = DateTime.ParseExact(currETDDate, "dd-MM-yyyy", culture);
            editData.PONo = currPONumber;
            dbPR.SaveChanges();

            return RedirectToAction("Quotation", "PurchaseRequest", new { area = "Purchasing", QuoNumber = editData.QuoNumber });
        }

        public ActionResult QuotationEdit()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            var currQuoNumber = Request["iQuoNumber"];
            IFormatProvider culture = new CultureInfo("en-US", true);

            var newData = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.QuoNumber == currQuoNumber).FirstOrDefault();
            newData.Due_Date = DateTime.ParseExact(Request["iDueDate"], "dd-MM-yyyy", culture);
            newData.Category = Request["iCategory"];
            newData.Description = Request["iDescription"];
            newData.Comment = Request["iComment"];
            newData.Basis_Award = Request["iBasisAward[]"];
            newData.Currency = Request["iCurrency"];
            dbPR.SaveChanges();
            var thirdPartyNameList = Request.Form.GetValues("iThirdPartyName[]").ToArray();
            var delDataSub = dbPR.Purchasing_PurchaseRequest_Quotation_Line_Vendor.Where(w => w.QuoNumber == currQuoNumber && !thirdPartyNameList.Contains(w.Third_Party_Name)).ToList();
            dbPR.Purchasing_PurchaseRequest_Quotation_Line_Vendor.RemoveRange(delDataSub);
            dbPR.SaveChanges();

            var ik = 0;
            for (var i = 0; i <= Request.Form.GetValues("iThirdParty[]").Count() - 1; i++)
            {
                var thirdPartyID = Request.Form.GetValues("iThirdPartyID[]")[i];
                var thirdPartyName = Request.Form.GetValues("iThirdPartyName[]")[i];
                for (var j = 0; j <= Request.Form.GetValues("iLineID[]").Count() - 1; j++)
                {
                    var lineSequence = int.Parse(Request.Form.GetValues("iLineSequence[]")[j]);
                    var lineID = int.Parse(Request.Form.GetValues("iLineID[]")[j]);
                    var qty = double.Parse(Request.Form.GetValues("iQty[]")[j]);
                    var updateLineSequence = dbPR.Purchasing_PurchaseRequest_Quotation_Line.Where(w => w.ID == lineID).FirstOrDefault();
                    updateLineSequence.Sequence = lineSequence;
                    var currVendorLineID = int.Parse(Request.Form.GetValues("iVendorID" + lineID.ToString() + "[]")[ik]);
                    var newDataSubCheck = dbPR.Purchasing_PurchaseRequest_Quotation_Line_Vendor.Where(w => w.ID == currVendorLineID).FirstOrDefault();
                    if (newDataSubCheck == null)
                    {
                        var newDataSub = new Purchasing_PurchaseRequest_Quotation_Line_Vendor();
                        newDataSub.QuoNumber = currQuoNumber;
                        newDataSub.Third_Party_ID = thirdPartyID;
                        newDataSub.Third_Party_Name = thirdPartyName.Trim();
                        newDataSub.Line_ID = lineID;
                        newDataSub.Discount_Type = Request.Form.GetValues("iDiscountType" + lineID.ToString() + "[]")[ik];
                        newDataSub.Discount = double.Parse(Request.Form.GetValues("iDiscount" + lineID.ToString() + "[]")[ik]);
                        newDataSub.Price = double.Parse(Request.Form.GetValues("iPrice" + lineID.ToString() + "[]")[ik]);
                        newDataSub.Total = double.Parse(Request.Form.GetValues("iTotal" + lineID.ToString() + "[]")[ik]);
                        newDataSub.IsChoosen = double.Parse(Request.Form.GetValues("iVendorChoose" + lineID.ToString() + "[]")[ik]) == 1 ? true : false;
                        newDataSub.Qty = qty;
                        dbPR.Purchasing_PurchaseRequest_Quotation_Line_Vendor.Add(newDataSub);
                    }
                    else
                    {
                        newDataSubCheck.QuoNumber = currQuoNumber;
                        newDataSubCheck.Third_Party_ID = thirdPartyID;
                        newDataSubCheck.Third_Party_Name = thirdPartyName.Trim();
                        newDataSubCheck.Line_ID = lineID;
                        newDataSubCheck.Discount_Type = Request.Form.GetValues("iDiscountType" + lineID.ToString() + "[]")[ik];
                        newDataSubCheck.Discount = double.Parse(Request.Form.GetValues("iDiscount" + lineID.ToString() + "[]")[ik]);
                        newDataSubCheck.Price = double.Parse(Request.Form.GetValues("iPrice" + lineID.ToString() + "[]")[ik]);
                        newDataSubCheck.Total = double.Parse(Request.Form.GetValues("iTotal" + lineID.ToString() + "[]")[ik]);
                        newDataSubCheck.IsChoosen = double.Parse(Request.Form.GetValues("iVendorChoose" + lineID.ToString() + "[]")[ik]) == 1 ? true : false;
                        newDataSubCheck.Qty = qty;
                    }
                }
                ik++;
            }

            dbPR.SaveChanges();

            uploadQTAttachment(newData.QuoNumber, "Quotation");

            return RedirectToAction("Quotation", "PurchaseRequest", new { area = "Purchasing", QuoNumber = newData.QuoNumber });
        }
        public ActionResult QuotationDelete()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            var currQuoNumber = Request["iQuoNumber"];
            IFormatProvider culture = new CultureInfo("en-US", true);

            var deleteData = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.QuoNumber == currQuoNumber).FirstOrDefault();
            var delDataSub = dbPR.Purchasing_PurchaseRequest_Quotation_Line.Where(w => w.QuoNumber == currQuoNumber).ToList();
            var delDataSubVendor = dbPR.Purchasing_PurchaseRequest_Quotation_Line_Vendor.Where(w => w.QuoNumber == currQuoNumber).ToList();
            dbPR.Purchasing_PurchaseRequest_Quotation_Line_Vendor.RemoveRange(delDataSubVendor);
            dbPR.Purchasing_PurchaseRequest_Quotation_Line.RemoveRange(delDataSub);
            dbPR.Purchasing_PurchaseRequest_Quotation_Header.Remove(deleteData);

            var deleteAttachment = dbPR.Purchasing_PurchaseRequest_Quotation_Attachment.Where(w => w.QuoNumber == currQuoNumber).ToList();
            foreach (var del in deleteAttachment)
            {
                deleteQTAttachment(del.ID);
            }
            var removeQuotation = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.QuoNumber == currQuoNumber).ToList();
            foreach (var rmv in removeQuotation)
            {
                rmv.QuoNumber = null;
            }
            dbPR.SaveChanges();

            var deleteApprovalHistory = db.Approval_History.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.Reveral_ID == currQuoNumber).ToList();
            var deleteApprovalList = db.Approval_List.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.Reveral_ID == currQuoNumber).ToList();
            db.Approval_History.RemoveRange(deleteApprovalHistory);
            db.Approval_List.RemoveRange(deleteApprovalList);
            db.SaveChanges();

            return RedirectToAction("Quotation", "PurchaseRequest", new { area = "Purchasing" });
        }
        public ActionResult QuotationCreateEdit()
        {
            var currID = int.Parse(Request["iID"]);
            var currItemID = Request["iItemID"];
            var currItemName = Request["iItemName"];
            var changePRLine = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.ID == currID).First();
            changePRLine.Item_ID = currItemID;
            changePRLine.Item_Name = currItemName;

            dbPR.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());
        }
        public JsonResult QuotationCommentGet()
        {
            var currQuoNumber = Request["iQuoNumber"];
            var currNIK = Request["iNIK"];
            var getComments = dbPR.Purchasing_PurchaseRequest_Quotation_Comments.Include("Attachments").Where(w => w.QuoNumber == currQuoNumber).ToList();
            foreach (var data in getComments)
            {
                bool checkCurrentUser = (currNIK == data.nik ? true : false);
                data.created_by_current_user = checkCurrentUser;
            }


            return Json(getComments, JsonRequestBehavior.AllowGet);
        }
        public JsonResult QuotationCommentAdd()
        {
            var currID = Request["id"];
            var currParent = Request["parent"];
            var currCreated = Request["created"];
            var currModified = Request["modified"];
            var currContent = Request["content"];
            var currAttachments = Request["attachments"];
            var currFullName = Request["fullname"];
            var currProfilePicture = Request["profile_picture_url"];
            var currCurrentUser = Request["created_by_current_user"];
            var currUpvoteCount = Request["upvote_count"];
            var currUserHasUpvoted = Request["user_has_upvoted"];
            var currQuoNumber = Request["QuoNumber"];
            var currNIK = Request["nik"];
            var checkData = dbPR.Purchasing_PurchaseRequest_Quotation_Comments.Where(w => w.id == currID && w.QuoNumber == currQuoNumber).OrderByDescending(o => o.id).FirstOrDefault();

            var newData = new Purchasing_PurchaseRequest_Quotation_Comments();
            newData.id = checkData != null ? "c" + (int.Parse(checkData.id.Replace("c", "")) + 1) : currID;
            newData.parent = currParent.Length > 0 && currParent != "null" ? currParent : null;
            newData.created = currCreated;
            newData.content = currContent;
            //newData.attachments = currAttachments != null ? currAttachments : "attachments";
            newData.fullname = currFullName;
            newData.created_by_current_user = bool.Parse(currCurrentUser);
            newData.upvoteCount = int.Parse(currUpvoteCount);
            newData.userHasUpvoted = bool.Parse(currUserHasUpvoted);
            newData.QuoNumber = currQuoNumber;
            newData.nik = currNIK;
            dbPR.Purchasing_PurchaseRequest_Quotation_Comments.Add(newData);
            dbPR.SaveChanges();

            uploadQTAttachment(currQuoNumber, "", newData.comment_id);
            sendNotification(currQuoNumber, "Quotation", "Comment", "", "", 0, 0, currContent);

            return Json(newData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult QuotationCommentDelete()
        {
            var currID = Request["id"];
            var currQuoNumber = Request["QuoNumber"];
            var currNIK = Request["nik"];
            var deleteData = dbPR.Purchasing_PurchaseRequest_Quotation_Comments.Where(w => w.id == currID && w.QuoNumber == currQuoNumber).FirstOrDefault();
            dbPR.Purchasing_PurchaseRequest_Quotation_Comments.Remove(deleteData);
            dbPR.SaveChanges();

            return Json(JsonConvert.SerializeObject(deleteData), JsonRequestBehavior.AllowGet);
        }
        public ActionResult QuotationCommentEdit()
        {
            var currID = Request["id"];
            var currParent = Request["parent"];
            var currCreated = Request["created"];
            var currModified = Request["modified"];
            var currContent = Request["content"];
            var currAttachments = Request["attachments"];
            var currFullName = Request["fullname"];
            var currProfilePicture = Request["profile_picture_url"];
            var currCurrentUser = Request["created_by_current_user"];
            var currQuoNumber = Request["QuoNumber"];
            var currNIK = Request["nik"];
            var newData = dbPR.Purchasing_PurchaseRequest_Quotation_Comments.Include("Attachments").Where(w => w.id == currID && w.QuoNumber == currQuoNumber).FirstOrDefault();
            newData.id = currID;
            newData.parent = currParent.Length > 0 && currParent != "null" ? currParent : null;
            newData.created = currCreated;
            newData.modified = long.Parse(currModified);
            newData.content = currContent;
            //newData.attachments = currAttachments != null ? currAttachments : "attachments";
            newData.fullname = currFullName;
            newData.created_by_current_user = bool.Parse(currCurrentUser);
            newData.QuoNumber = currQuoNumber;
            newData.nik = currNIK;
            dbPR.SaveChanges();
            sendNotification(currQuoNumber, "Quotation", "Comment", "", "", 0, 0, currContent);

            return Json(newData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult QuotationDiscountTypeChange()
        {
            var currDiscountType = Request["DiscountType"];
            var currQuoNumber = Request["QuoNumber"];
            var currVendorName = Request["VendorName"];
            var editData = dbPR.Purchasing_PurchaseRequest_Quotation_Line_Vendor.Where(w => w.QuoNumber == currQuoNumber && w.Third_Party_Name == currVendorName).ToList();
            editData.ForEach(f =>
            {
                f.Discount_Type = currDiscountType;
            });

            dbPR.SaveChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PRForm()
        {
            return View();
        }

        public void uploadAttachment(string currReqNumber)
        {
            string checkFolder = "~/Files/Purchasing/PurchaseRequest/PR/" + currReqNumber; // Your code goes here
            bool exists = System.IO.Directory.Exists(Server.MapPath(checkFolder));
            if (!exists)
                System.IO.Directory.CreateDirectory(Server.MapPath(checkFolder));
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase iFile = Request.Files[i];
                // extract only the filename
                if (iFile.ContentLength > 0)
                {
                    var fileName = iFile.FileName;
                    string extension = Path.GetExtension(fileName);
                    // store the file inside ~/App_Data/uploads folder
                    var path = Path.Combine(Server.MapPath("~/Files/Purchasing/PurchaseRequest/PR/" + currReqNumber), fileName);
                    iFile.SaveAs(path);
                    var checkFile = dbPR.Purchasing_PurchaseRequest_Attachment.Where(w => w.ReqNumber == currReqNumber && w.Filename == fileName).FirstOrDefault();
                    if (checkFile == null)
                    {
                        dbPR.Purchasing_PurchaseRequest_Attachment.Add(new Purchasing_PurchaseRequest_Attachment()
                        {
                            ReqNumber = currReqNumber,
                            Filename = fileName,
                            Ext = extension

                        });
                    }
                }
            }

            dbPR.SaveChanges();
        }

        [HttpPost]
        public ActionResult getPRAttachment()
        {
            var currReqNumber = Request["iReqNumber"];

            var getFiles = dbPR.Purchasing_PurchaseRequest_Attachment.Where(w => w.ReqNumber == currReqNumber).Select(s => new { filename = s.Filename, ext = s.Ext, id = s.ID });

            return Json(new { files = getFiles }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult deletePRAttachment(int iID)
        {
            var currID = iID;
            var del = dbPR.Purchasing_PurchaseRequest_Attachment.Where(w => w.ID == currID).FirstOrDefault();

            var path = Server.MapPath("~/Files/Purchasing/PurchaseRequest/PR/" + del.ReqNumber + "/" + del.Filename);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            dbPR.Purchasing_PurchaseRequest_Attachment.Remove(del);
            dbPR.SaveChanges();

            return Content(Boolean.TrueString);
        }

        public ActionResult uploadQTAttachment(string iQuoNumber, string iThirdPartyName, int iCommentID = 0)
        {
            iThirdPartyName = iThirdPartyName != "" ? iThirdPartyName.Trim() : "Comments";

            string checkFolder = "~/Files/Purchasing/PurchaseRequest/QT/" + iQuoNumber + "/" + iThirdPartyName; // Your code goes here
            bool exists = System.IO.Directory.Exists(Server.MapPath(checkFolder));
            if (!exists)
                System.IO.Directory.CreateDirectory(Server.MapPath(checkFolder));
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase iFile = Request.Files[i];
                // extract only the filename
                if (iFile.ContentLength > 0)
                {
                    var fileName = iFile.FileName;
                    string extension = Path.GetExtension(fileName);
                    // store the file inside ~/App_Data/uploads folder
                    var path = Path.Combine(Server.MapPath("~/Files/Purchasing/PurchaseRequest/QT/" + iQuoNumber + "/" + iThirdPartyName), fileName);
                    iFile.SaveAs(path);
                    if (iThirdPartyName == "Comments")
                    {
                        var checkFile = dbPR.Purchasing_PurchaseRequest_Quotation_Comments_Attachment.Where(w => w.Comment_ID == iCommentID && w.file == path).FirstOrDefault();
                        if (checkFile == null)
                        {
                            dbPR.Purchasing_PurchaseRequest_Quotation_Comments_Attachment.Add(new Purchasing_PurchaseRequest_Quotation_Comments_Attachment()
                            {
                                Comment_ID = iCommentID,
                                file = Path.Combine("/NGKBusi/Files/Purchasing/PurchaseRequest/QT/" + iQuoNumber + "/" + iThirdPartyName, fileName),
                                mime_type = HeyRed.Mime.MimeTypesMap.GetMimeType(fileName)
                            });
                        }
                    }
                    else
                    {
                        var checkFile = dbPR.Purchasing_PurchaseRequest_Quotation_Attachment.Where(w => w.QuoNumber == iQuoNumber && w.Third_Party_Name == iThirdPartyName && w.Filename == fileName).FirstOrDefault();
                        if (checkFile == null)
                        {
                            dbPR.Purchasing_PurchaseRequest_Quotation_Attachment.Add(new Purchasing_PurchaseRequest_Quotation_Attachment()
                            {
                                QuoNumber = iQuoNumber,
                                Third_Party_Name = iThirdPartyName,
                                Filename = fileName,
                                Ext = extension
                            });
                        }
                    }
                }
            }

            dbPR.SaveChanges();

            return RedirectToAction("Quotation", "PurchaseRequest", new { area = "Purchasing", QuoNumber = iQuoNumber });
        }

        [HttpPost]
        public ActionResult getQTAttachment()
        {
            var currQuoNumber = Request["iQuoNumber"];
            var currThirdPartyName = Request["iThirdPartyName"];

            var getFiles = dbPR.Purchasing_PurchaseRequest_Quotation_Attachment.Where(w => w.QuoNumber == currQuoNumber && w.Third_Party_Name == currThirdPartyName).Select(s => new { filename = s.Filename, ext = s.Ext, id = s.ID });

            return Json(new { files = getFiles }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult deleteQTAttachment(int iID)
        {
            var currID = iID;
            var del = dbPR.Purchasing_PurchaseRequest_Quotation_Attachment.Where(w => w.ID == currID).FirstOrDefault();
            try
            {
                var path = Server.MapPath("~/Files/Purchasing/PurchaseRequest/QT/" + del.QuoNumber + "/" + del.Third_Party_Name + "/" + del.Filename);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                dbPR.Purchasing_PurchaseRequest_Quotation_Attachment.Remove(del);
                dbPR.SaveChanges();
            }
            catch
            {
            }

            return Content(Boolean.TrueString);
        }

        public String getSequence(string type)
        {
            var lastSeq = "";
            var seqHeader = type + DateTime.Now.ToString("yy");
            var latestSequence = "";
            if (type == "PR")
            {
                latestSequence = dbPR.Purchasing_PurchaseRequest_Header.Where(w => w.ReqNumber.Substring(0, 4) == seqHeader).OrderByDescending(o => o.ID).Select(s => s.ReqNumber.Substring(s.ReqNumber.Length - 4, 4)).FirstOrDefault();
            }
            else if (type == "QT")
            {
                latestSequence = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.QuoNumber.Substring(0, 4) == seqHeader).OrderByDescending(o => o.ID).Select(s => s.QuoNumber.Substring(s.QuoNumber.Length - 4, 4)).FirstOrDefault();
            }
            lastSeq = latestSequence != null ? "0000" + (Int32.Parse(latestSequence) + 1) : "0001";
            lastSeq = seqHeader + "-" + lastSeq.Substring(lastSeq.Length - 4, 4);

            return lastSeq;
        }
        public String ApprovalStatus(int Approval, int Approval_Sub, int Type = 1)
        {
            var stat = "Submitted";
            if (Type == 1)
            {
                switch (Approval_Sub)
                {
                    case 1:
                        stat = "Submitted";
                        break;
                    default:
                        stat = "Created";
                        break;
                }

                if (Approval > 1)
                {
                    stat = "Approved";
                }
            }
            else if (Type == 2)
            {

                if (Approval == 1)
                {
                    switch (Approval_Sub)
                    {
                        case 1:
                            stat = "Submitted";
                            break;
                        case 2:
                            stat = "Purchasing - Checked";
                            break;
                        case 3:
                            stat = "Purchasing - Reviewed";
                            break;
                        case 4:
                            stat = "Purchasing - Approved";
                            break;
                        default:
                            stat = "Created";
                            break;
                    }
                }
                else if (Approval == 2)
                {
                    switch (Approval_Sub)
                    {
                        case 1:
                            stat = "Dept - Signed";
                            break;
                        case 2:
                            stat = "Dept - Checked";
                            break;
                        case 3:
                            stat = "Dept - Approved";
                            break;
                        case 4:
                            stat = "Dept - Approved";
                            break;
                        default:
                            stat = "Purchasing - Approved";
                            break;
                    }
                }
                else if (Approval == 3)
                {
                    switch (Approval_Sub)
                    {
                        case 1:
                            stat = "Management - Reviewed";
                            break;
                        case 2:
                            stat = "Management - Reviewed";
                            break;
                        case 3:
                            stat = "Management - Approved";
                            break;
                        case 4:
                            stat = "Management - Approved";
                            break;
                        default:
                            stat = "Dept - Approved";
                            break;
                    }
                }
                else
                {
                    stat = "Management - Approved";
                }

            }
            return stat;
        }
        public void sendNotification(string currReqNumber, string currMenu, string currStatus, string currNIK = "", string deptCode = "", int approval = 0, int approval_sub = 0, string note = "-")
        {
            var currUser = ((ClaimsIdentity)User.Identity);
            var currUserID = currUser.GetUserId();
            var currUserName = currUser.FindFirstValue("fullName");
            string FilePath = Path.Combine(Server.MapPath("~/Emails/Purchasing/PurchaseRequest/"), "PurchaseRequestApproval.html");
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            var stat = "";
            var needs = "Approval";
            if (currStatus == "Reject")
            {
                stat = "Rejected!";
                needs = "Attention";
            }
            else if (currStatus == "Return")
            {
                stat = "Returned!";
                needs = "Attention";
            }
            else if (currStatus == "Comment")
            {
                stat = "Commented";
                needs = "Attention";
            }

            var settlementSubject = " - Purchase Request";
            var doc = "Purchase Request - Request";
            if (currMenu == "Quotation")
            {
                doc = "Purchase Request - Quotation";
                settlementSubject = " - Quotation";
            }

            var currURL = Url.Action(currMenu, "PurchaseRequest", new { area = "Purchasing", ReqNumber = currReqNumber }, this.Request.Url.Scheme);
            var currURLOpen = Url.Action(currMenu, "PurchaseRequest", new { area = "Purchasing", iPRFilterStatus = "Open" }, this.Request.Url.Scheme);
            var documentID = 1;
            var emailList = db.Approval_Master.Where(w => w.Menu_Id == 40 && w.Document_Id == documentID && w.Dept_Code == deptCode && w.Levels == approval && w.Levels_Sub == approval_sub).Select(s => s.Users.Email).Distinct().ToList();


            if (currMenu == "Quotation")
            {
                documentID = 2;
                emailList = db.Approval_List.Where(w => w.Menu_Id == 40 && w.Document_Id == documentID && w.Reveral_ID == currReqNumber && w.Dept_Code == deptCode && w.Levels == approval && w.Levels_Sub == approval_sub && w.Is_Skip == false).Select(s => s.Users.Email).Distinct().ToList();
                currURL = Url.Action(currMenu, "PurchaseRequest", new { area = "Purchasing", QuoNumber = currReqNumber }, this.Request.Url.Scheme);
                currURLOpen = Url.Action(currMenu, "PurchaseRequest", new { area = "Purchasing", iPRFilterStatus = "Open" }, this.Request.Url.Scheme);
            }
            if (documentID == 2 && currStatus == "Comment")
            {
                var latestRejectID = db.Approval_History.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.Reveral_ID == currReqNumber && (w.Status == "Return" || w.Status == "Reject")).OrderByDescending(o => o.id).FirstOrDefault()?.id ?? 0;
                var emailListQRY = db.Approval_History.Where(w => w.Menu_Id == 40 && w.Document_Id == 2 && w.Reveral_ID == currReqNumber);
                var commentList = dbPR.Purchasing_PurchaseRequest_Quotation_Comments.Where(w => w.QuoNumber == currReqNumber).Select(s => s.Users.Email).Distinct().ToList();
                if (latestRejectID != 0)
                {
                    emailListQRY = emailListQRY.Where(w => w.id > latestRejectID && w.Status != "Return" && w.Status != "Reject");
                }
                emailList = emailListQRY.Select(s => s.Users.Email).Distinct().ToList();

                foreach (var comment in commentList)
                {
                    emailList.Add(comment);
                }
            }
            if (currStatus != "Sign" && currStatus != "Comment")
            {
                emailList = db.Users.Where(w => w.NIK == currNIK).Select(s => s.Email).Distinct().ToList();
            }

            //Repalce [newusername] = signup user name   
            MailText = MailText.Replace("##document##", doc);
            MailText = MailText.Replace("##needs##", needs);
            MailText = MailText.Replace("##link##", currURL.Replace("http://192.168.1.248/", "https://portal.ngkbusi.com/"));
            MailText = MailText.Replace("##url##", currURL.Replace("http://192.168.1.248/", "https://portal.ngkbusi.com/"));
            MailText = MailText.Replace("##note##", note);
            MailText = MailText.Replace("##noteby##", currUserName);
            MailText = MailText.Replace("##linkOpen##", currURLOpen.Replace("http://192.168.1.248/", "https://portal.ngkbusi.com/"));
            MailText = MailText.Replace("##urlOpen##", currURLOpen.Replace("http://192.168.1.248/", "https://portal.ngkbusi.com/"));
            MailText = MailText.Replace("##guid##", Guid.NewGuid().ToString());

            var senderEmail = new MailAddress("ngkportal-notification@ngkbusi.com", "Niterra-Portal-Notification");
            var password = "100%NGKbusi!";
            var sub = "[Niterra-Portal-Notification]" + stat + " - Purchase Request -" + currReqNumber + settlementSubject;
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
            using (var mess = new MailMessage()
            {
                From = senderEmail,
                Subject = sub,
                Body = body,
                IsBodyHtml = true
            })
            {
                foreach (var dataEmail in emailList)
                {
                    if (dataEmail.Length > 0)
                    {
                        mess.To.Add(new MailAddress(dataEmail));
                    }
                }
                mess.Bcc.Add(new MailAddress("azis.abdillah@ngkbusi.com"));
                smtp.Send(mess);
            }
        }
        public String ApprovalHistory(string Reveral_ID, int Approval, int Approval_Sub, int getType, int documentID = 1)
        {
            var str = "";
            var getApprovalHistory = db.Approval_History.Where(w => w.Menu_Id == 40 && w.Document_Id == documentID && w.Reveral_ID == Reveral_ID && w.Approval == Approval && w.Approval_Sub == Approval_Sub).OrderByDescending(o => o.id).FirstOrDefault();

            if (getType == 1)
            {
                str = getApprovalHistory?.Created_By_Name ?? "";
            }
            else if (getType == 2)
            {
                str = getApprovalHistory?.Created_At.ToString("dd-MMM-yyyy") ?? "";
            }
            else
            {
                str = getApprovalHistory?.Note ?? "";
            }

            return str;
        }

        public ActionResult requestSign()
        {
            var currUser = ((ClaimsIdentity)User.Identity);
            var currUserID = currUser.GetUserId();
            var currUserName = currUser.FindFirstValue("fullName");
            var reqNumber = Request["iSignReqNumber"];
            var btnType = Request["btnType"];
            var currNote = Request["iPRNote"] ?? "";
            var updateSign = dbPR.Purchasing_PurchaseRequest_Header.Where(w => w.ReqNumber == reqNumber).FirstOrDefault();

            if (btnType == "Sign")
            {
                if (updateSign.Approval == 1 && updateSign.Approval_Sub == 1)
                {
                    updateSign.Approval += 1;
                    updateSign.Approval_Sub = 0;
                }
                else
                {
                    updateSign.Approval_Sub += 1;
                }
            }
            else if (btnType == "Return")
            {
                Purchasing_PurchaseRequest_Header retPR = returnPRbyPurchasing(updateSign, Request["iPRReturnList"]);
                if (retPR == null)
                {
                    updateSign.Approval = 1;
                    updateSign.Approval_Sub = 0;
                }
                else
                {
                    updateSign = retPR;
                }
            }
            else
            {
                if (updateSign.Approval == 1 || (updateSign.Approval == 2 && updateSign.Approval_Sub == 2) || updateSign.Approval == 3)
                {
                    updateSign.Approval += 1;
                    updateSign.Approval_Sub = 0;
                }
                else
                {
                    updateSign.Approval_Sub += 1;
                }
                updateSign.Is_Reject = true;
            }
            dbPR.SaveChanges();

            var currApproval = btnType == "Reject" ? 1 : updateSign.Approval;
            var currApprovalSub = btnType == "Reject" ? 0 : updateSign.Approval_Sub;

            var getApprovalMaster = db.Approval_Master.Where(w => w.User_NIK == currUserID && w.Document_Id == 1 && w.Menu_Id == 40).FirstOrDefault();
            var updateSignHistory = new Approval_History();
            updateSignHistory.Menu_Id = 40;
            updateSignHistory.Menu_Name = "Purchase Request";
            updateSignHistory.Document_Id = 1;
            updateSignHistory.Document_Name = "Form Request";
            updateSignHistory.Reveral_ID = reqNumber;
            updateSignHistory.Reveral_ID_Sub = null;
            updateSignHistory.Title = getApprovalMaster.Title;
            updateSignHistory.Header = getApprovalMaster.Header;
            updateSignHistory.Label = getApprovalMaster.Label;
            updateSignHistory.Note = currNote;
            updateSignHistory.Approval = updateSign.Approval;
            updateSignHistory.Approval_Sub = updateSign.Approval_Sub;
            updateSignHistory.IsReject = updateSign.Is_Reject ?? false;
            updateSignHistory.IsRevise = false;
            updateSignHistory.Status = (btnType != "Sign" ? btnType : ApprovalStatus(updateSign.Approval, updateSign.Approval_Sub));
            updateSignHistory.Created_At = DateTime.Now;
            updateSignHistory.Created_By_ID = currUserID;
            updateSignHistory.Created_By_Name = currUserName;
            db.Approval_History.Add(updateSignHistory);
            db.SaveChanges();

            sendNotification(reqNumber, "PRequest", btnType, updateSign.Created_By, updateSign.Section.Split('|')[0].Trim(), currApproval, currApprovalSub, currNote);

            return RedirectToAction("PRequest", "PurchaseRequest", new { area = "Purchasing", ReqNumber = reqNumber });
        }

        public Purchasing_PurchaseRequest_Header returnPRbyPurchasing(Purchasing_PurchaseRequest_Header PR, string PRLineID = "")
        {
            var PRLineIDList = PRLineID?.Split(',')?.Select(s => Convert.ToInt32(s));
            var currPR = PR;
            var currPRReqNumber = currPR.ReqNumber;

            var checkLines = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.ReqNumber == currPRReqNumber).ToList().Count;
            if (PRLineIDList != null && PRLineIDList.Count() > 0 && checkLines != PRLineIDList.Count())
            {
                var newData = new Purchasing_PurchaseRequest_Header();
                newData.ReqNumber = this.getSequence("PR");
                newData.Section = currPR.Section;
                newData.Due_Date = currPR.Due_Date;
                newData.Description = "(Partial return from " + currPRReqNumber + ")" + currPR.Description;
                newData.Created_At = DateTime.Now;
                newData.Created_By = currPR.Created_By;
                newData.Approval = 1;
                newData.Approval_Sub = 0;
                newData.Is_Reject = false;
                dbPR.Purchasing_PurchaseRequest_Header.Add(newData);
                dbPR.SaveChanges();

                var returnLines = dbPR.Purchasing_PurchaseRequest_Line.Where(w => PRLineIDList.Contains(w.ID)).ToList();
                foreach (var data in returnLines)
                {
                    data.ReqNumber = newData.ReqNumber;
                }

                dbPR.SaveChanges();

                return newData;
            }

            return null;
        }

        public ActionResult QuotationSign()
        {
            var currUser = ((ClaimsIdentity)User.Identity);
            var currUserID = currUser.GetUserId();
            var currUserName = currUser.FindFirstValue("fullName");
            var quoNumber = Request["iSignQuoNumber"];
            var btnType = Request["btnType"];
            var currNote = Request["iPRNote"] ?? "";
            var updateSign = dbPR.Purchasing_PurchaseRequest_Quotation_Header.Where(w => w.QuoNumber == quoNumber).FirstOrDefault();
            var curApproval = updateSign.Approval;
            var curApprovalSub = updateSign.Approval_Sub;
            var checkApprovalMaster = db.Approval_List.Where(w => w.Reveral_ID == quoNumber && w.Dept_Code == updateSign.Section.Substring(0, 5) && w.Document_Id == 2 && w.Menu_Id == 40 && w.Levels == updateSign.Approval && w.Levels_Sub > updateSign.Approval_Sub && w.Is_Skip == false).OrderBy(o => o.Levels_Sub).FirstOrDefault();
            if (checkApprovalMaster == null)
            {
                checkApprovalMaster = db.Approval_List.Where(w => w.Reveral_ID == quoNumber && w.Dept_Code == updateSign.Section.Substring(0, 5) && w.Document_Id == 2 && w.Menu_Id == 40 && w.Levels > updateSign.Approval && w.Is_Skip == false).OrderBy(o => o.Levels).ThenBy(o => o.Levels_Sub).FirstOrDefault();
            }
            var getApprovalMaster = db.Approval_List.Where(w => w.Reveral_ID == quoNumber && w.User_NIK == currUserID && w.Document_Id == 2 && w.Menu_Id == 40 && w.Levels == updateSign.Approval && w.Levels_Sub == updateSign.Approval_Sub && w.Is_Skip == false).FirstOrDefault();

            if (btnType == "Return")
            {
                updateSign.Approval = 1;
                updateSign.Approval_Sub = 0;
            }
            else
            {
                if (checkApprovalMaster != null)
                {
                    updateSign.Approval = checkApprovalMaster.Levels;
                    updateSign.Approval_Sub = checkApprovalMaster.Levels_Sub;
                }
                else
                {
                    updateSign.Approval += 1;
                    updateSign.Approval_Sub = 0;
                }

                if (btnType == "Reject")
                {
                    updateSign.Is_Reject = true;
                }
            }
            dbPR.SaveChanges();

            var currApproval = btnType == "Reject" ? 1 : updateSign.Approval;
            var currApprovalSub = btnType == "Reject" ? 0 : updateSign.Approval_Sub;

            var updateSignHistory = new Approval_History();
            updateSignHistory.Menu_Id = 40;
            updateSignHistory.Menu_Name = "Purchase Request";
            updateSignHistory.Document_Id = 2;
            updateSignHistory.Document_Name = "Quotation";
            updateSignHistory.Reveral_ID = quoNumber;
            updateSignHistory.Reveral_ID_Sub = null;
            updateSignHistory.Title = getApprovalMaster.Title;
            updateSignHistory.Header = getApprovalMaster.Header;
            updateSignHistory.Label = getApprovalMaster.Label;
            updateSignHistory.Note = currNote;
            updateSignHistory.Approval = curApproval;
            updateSignHistory.Approval_Sub = curApprovalSub;
            updateSignHistory.IsReject = updateSign.Is_Reject ?? false;
            updateSignHistory.IsRevise = false;
            updateSignHistory.Status = (btnType != "Sign" ? btnType : ApprovalStatus(updateSign.Approval, updateSign.Approval_Sub));
            updateSignHistory.Created_At = DateTime.Now;
            updateSignHistory.Created_By_ID = currUserID;
            updateSignHistory.Created_By_Name = currUserName;
            db.Approval_History.Add(updateSignHistory);
            db.SaveChanges();

            sendNotification(quoNumber, "Quotation", btnType, updateSign.Created_By, updateSign.Section.Split('|')[0].Trim(), currApproval, currApprovalSub, currNote);

            return RedirectToAction("Quotation", "PurchaseRequest", new { area = "Purchasing", QuoNumber = quoNumber });
        }

        public ActionResult WFL(String QuoNumber)
        {
            ViewBag.NavHide = true;
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            var currPayLevel = db.Approval_Master.Where(w => w.User_NIK == currUserID && w.Menu_Id == 40).FirstOrDefault();
            var currPayLevels = db.Approval_Master.Where(w => w.User_NIK == currUserID && w.Menu_Id == 40).ToList();
            if (currPayLevels.Count() == 0)
            {
                return View("UnAuthorized");
            }

            ViewBag.WFLHeaderList = dbPR.Purchasing_PurchaseRequest_WFL_Header.ToList();
            ViewBag.WFLLineList = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.Headers.Approval >= 2 && w.Item_ID.Length == 0).ToList();
            ViewBag.WFLList = dbPR.Purchasing_PurchaseRequest_Line.Where(w => w.Headers.Approval >= 2 && w.Item_ID.Length == 0).ToList();

            return View();
        }


        public ActionResult DownloadQuotation()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currUserID = currUser.GetUserId().Trim();
            var quoNumber = Request["QuoNumber"];

            var path = Server.MapPath("~/Files/Purchasing/PurchaseRequest/D365/Upload/Purchase Order.xlsx");
            System.IO.File.Copy(Server.MapPath("~/Files/Purchasing/PurchaseRequest/D365/Master/Purchase Order.xlsx"), path, true);
            string ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties='Excel 12.0; HDR=YES;READONLY=FALSE;'";
            using (OleDbConnection conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                var queryText = "";
                using (OleDbCommand command = conn.CreateCommand())
                {

                    var quotation = dbPR.Purchasing_PurchaseRequest_Quotation_Line.Where(w => w.QuoNumber == quoNumber).ToList();
                    foreach (var data in quotation)
                    {
                        var vendor = dbPR.Purchasing_PurchaseRequest_Quotation_Line_Vendor.Where(w => w.Line_ID == data.ID && w.IsChoosen == true).FirstOrDefault();

                        queryText = string.Format("Insert into [Quotation$] " +
                                    "(`Item Number`,`Quantity`,`Discount`,`Customer Requisition`,`Item Name`) " +
                                    "values('" + data?.Item_ID.ToString() + "','" + vendor?.Qty.ToString() + "', '" + vendor?.Discount.ToString() + "', '" + data?.Budget_Number.ToString() + "', '" + data?.Item_Name.ToString() + "');");

                        command.CommandText = queryText;
                        command.ExecuteNonQuery();

                    }
                }
            }

            Response.ContentType = "application/x-msexcel";
            Response.AppendHeader("Content-Disposition", "attachment; filename=Purchase Order.xlsx");
            Response.TransmitFile(path);
            Response.End();

            return RedirectToAction("Quotation", "PurchaseRequest", new { area = "Purchasing" });
        }

    }
}