using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using NGKBusi.Areas.HC.Models;
using NGKBusi.Areas.IT.Models;
using NGKBusi.Areas.Marketing.Models;
using NGKBusi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace NGKBusi.Areas.HC.Controllers
{
    public class LegalPermitController : Controller
    {
        LegalPermitConnection dblp = new LegalPermitConnection();
        ReminderConnection dbr = new ReminderConnection();
        DefaultConnection db = new DefaultConnection();

        List<string> userAdmin = new List<string>() { "632.11.12", "831.03.19", "629.01.13" };

        // GET: HC/LegalPermit
        public ActionResult Index()
        {
            return View();
        }
        public class ListEmail
        {
            public string Email { get; set; }
        }
        public ActionResult Flow()
        {
            var spl = dblp.HC_LegalPermit_Flow.ToList();
            ViewBag.Flow = spl;
            return View();

        }
        [HttpPost]
        public ActionResult AddFlow(string FlowName)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();

            HC_LegalPermit_Flow flow = new HC_LegalPermit_Flow();
            flow.FlowName = FlowName;
            flow.IsActive = "1";
            flow.CreateBy = currUser;
            flow.CreateTime = DateTime.Now;

            dblp.HC_LegalPermit_Flow.Add(flow);
            int i = dblp.SaveChanges();
            if (i > 0)
            {
                return Json(new { status = 1, msg = "Add Flow Success" });
            } else
            {
                return Json(new { status = 1, msg = "Failed to Add Flow" });
            }
        }
        [HttpPost]
        public ActionResult UpdateFlow(string FlowName, int ID)
        {
            var spl = dblp.HC_LegalPermit_Flow.Where(w => w.ID == ID).FirstOrDefault();
            spl.FlowName = FlowName;

            int save = dblp.SaveChanges();
            if (save > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "update success",
                    save = save
                });
            }
            else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Failed to update",
                    save = save,
                    id = ID,
                    name = FlowName
                });
            }
        }
        [HttpPost]
        public ActionResult FormAddStep(int ID)
        {
            var flow = dblp.HC_LegalPermit_Flow.Where(w => w.ID == ID).FirstOrDefault();
            var listUser = db.V_Users_Active.ToList();

            ViewBag.flow = flow;
            ViewBag.listUser = listUser;
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddStep(HC_LegalPermit_Flow_Detail smodel)
        {
            //get count step
            int count = dblp.HC_LegalPermit_Flow_Detail.Where(w => w.Flow_ID == smodel.Flow_ID).ToList().Count();

            HC_LegalPermit_Flow_Detail flowDetail = new HC_LegalPermit_Flow_Detail();
            flowDetail.StepName = smodel.StepName;
            flowDetail.Description = smodel.Description;
            flowDetail.Estimation_Time = smodel.Estimation_Time;
            flowDetail.PIC = smodel.PIC;
            flowDetail.Flow_ID = smodel.Flow_ID;
            flowDetail.Requirement_Document = smodel.Requirement_Document;
            flowDetail.StepNumber = count + 1;
            dblp.HC_LegalPermit_Flow_Detail.Add(flowDetail);

            int save = dblp.SaveChanges();

            if(save > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Save success",
                    save = save
                });
            } else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Failed to save data",
                    save = save
                });
            }
            
        }
        public ActionResult DetailTemplateData(string FlowID)
        {

            var spl = dblp.HC_LegalPermit_Flow_Detail.Where(w => w.Flow_ID.ToString() == FlowID && w.IsDelete == 0).OrderBy(x => x.StepNumber).ToList();
            var CountRow = dblp.HC_LegalPermit_Flow_Detail.Count(w => w.Flow_ID.ToString() == FlowID);

            List<Tbl_HC_LegalPermit_Flow_Detail> actions = new List<Tbl_HC_LegalPermit_Flow_Detail>();

            int no = 0;
            foreach (var Item in spl)
            {
                no++;
                var Tools = "";

                if (Item.StepNumber == 1)
                {
                    Tools += "<div class='btn-group'>";
                    Tools += "<button class='btn btn-danger' id='btndeleteStep' data-id='" + Item.ID + "'>";
                    Tools += "<i class='fas fa-trash'></i>";
                    Tools += "</button>";
                    Tools += "<button class='btn btn-primary ' id='btnDown' data-id='" + Item.ID+"' >";
                    Tools += "<i class='fas fa-caret-down'></i>";
                    Tools += "</button></div>";
                } else if (Item.StepNumber == CountRow)
                {
                    Tools += "<div class='btn-group'>";
                    Tools += "<button class='btn btn-danger' id='btndeleteStep' data-id='" + Item.ID + "'>";
                    Tools += "<i class='fas fa-trash'></i>";
                    Tools += "</button>";
                    Tools += "<button class='btn btn-info' id='btnUp' data-id='" + Item.ID + "'>";
                    Tools += "<i class='fas fa-caret-up'></i>";
                    Tools += "</button></div>";
                } else
                {
                    Tools += "<div class='btn-group'>";
                    Tools += "<button class='btn btn-danger' id='btndeleteStep' data-id='" + Item.ID + "'>";
                    Tools += "<i class='fas fa-trash'></i>";
                    Tools += "</button>";
                    Tools += "<button class='btn btn-info' id='btnUp' data-id='" + Item.ID + "'>";
                    Tools += "<i class='fas fa-caret-up'></i>";
                    Tools += "</button>";
                    Tools += "<button class='btn btn-primary' id='btnDown' data-id='" + Item.ID + "'>";
                    Tools += "<i class='fas fa-caret-down'></i>";
                    Tools += "</button></div>";
                }
                //var getPICInfo = db.V_Users_Active.Where(w => w.NIK == Item.PIC).FirstOrDefault();
                
                //Tools = "<a href=\"#\" title=\"Up\" id=\"btnUp\"  data-id=\"" + Item.ID + "\" class=\"btn-sm btn-danger UpStep\"><i class=\"fa fa-trash\"></i></a>";

                actions.Add(
                    new Tbl_HC_LegalPermit_Flow_Detail()
                    {
                        PIC = Item.PIC,
                        StepName = Item.StepName,
                        StepNumber = Item.StepNumber.ToString(),
                        Description = Item.Description,
                        EstimationTime = Item.Estimation_Time.ToString(),
                        Requirement_Document = Item.Requirement_Document,
                        No = no,
                        Button = Tools

                    }) ;
            }

            return Json(new
            {
                rows = actions,
                totalNotFiltered = CountRow,
                total = CountRow,
                status = 1
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult StepDown(int ID)
        {
            //turunkan step numbernya
            var spl = dblp.HC_LegalPermit_Flow_Detail.Where(w => w.ID == ID).FirstOrDefault();
            int newStepNumber = spl.StepNumber = spl.StepNumber + 1;
            spl.StepNumber = newStepNumber;
            // naikkan step number dibawahnya
            var spl2 = dblp.HC_LegalPermit_Flow_Detail.Where(w => w.Flow_ID == spl.Flow_ID && w.StepNumber == newStepNumber).FirstOrDefault();
            spl2.StepNumber = spl2.StepNumber - 1;
            int save = dblp.SaveChanges();
            if (save > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Save success",
                    save = save
                });
            }
            else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Failed to save data",
                    save = save
                });
            }

        }
        [HttpPost]
        public ActionResult StepUp(int ID)
        {
            //turunkan step numbernya
            var spl = dblp.HC_LegalPermit_Flow_Detail.Where(w => w.ID == ID).FirstOrDefault();
            int newStepNumber = spl.StepNumber = spl.StepNumber - 1;
            spl.StepNumber = newStepNumber;
            // naikkan step number dibawahnya
            var spl2 = dblp.HC_LegalPermit_Flow_Detail.Where(w => w.Flow_ID == spl.Flow_ID && w.StepNumber == newStepNumber).FirstOrDefault();
            spl2.StepNumber = spl2.StepNumber + 1;
            int save = dblp.SaveChanges();
            if (save > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Save success",
                    save = save
                });
            }
            else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Failed to save data",
                    save = save
                });
            }

        }
        [HttpPost]
        public ActionResult DeleteStep(int ID)
        {
            //turunkan step numbernya
            var spl = dblp.HC_LegalPermit_Flow_Detail.Where(w => w.ID == ID).FirstOrDefault();
            spl.IsDelete = 1;
            int save = dblp.SaveChanges();
            if (save > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Save success",
                    save = save
                });
            }
            else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Failed to save data",
                    save = save
                });
            }

        }

        /* ----------------------- Request controller ---------------------------------- */
        public ActionResult RequestList()
        {
            return View();

        }

        public JsonResult GetRequestList()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            //var spl = db.V_SCM_Sparepart_Master_List.Where(w =>  w.CostName == CurrUser.CostName && w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling").ToList();
            var spl = dblp.HC_LegalPermit_Request.OrderByDescending(o => o.ID).ToList();
            var CountRow = dblp.HC_LegalPermit_Request.Count();
            List<Tbl_HC_LegalPermit_Request> actions = new List<Tbl_HC_LegalPermit_Request>();

            foreach (var Item in spl)
            {
                //var Tools = "";

                var UrlAction = Url.Action("RequestDetail", "LegalPermit", new { area = "HC", ID = Item.ID.ToString() });

                var editButton = "";

                editButton = "<a href=\"" + UrlAction + "\" title=\"Detail\" >"+Item.ID+"</i></a>";

                actions.Add(
                    new Tbl_HC_LegalPermit_Request
                    {
                        RequestNo = editButton,
                        RequestDate = Item.CreateTime.ToString("MMMM dd, yyyy"),
                        SupplierName = Item.SupplierName,
                        ProjectName = Item.ProjectName,
                        FinalPrice = Item.FinalPrice.ToString(),
                        PlanStartDate = Item.PlanStartDate.ToString("MMMM dd, yyyy"),
                        //IsActive = activation,
                        PlanEndDate = Item.PlanEndDate.ToString("MMMM dd, yyyy"),
                    }); ;
            }

            var jsonResult = Json(new { rows = actions, totalNotFiltered = CountRow, total = CountRow }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult RequestForm()
        {
            return View();

        }
        [HttpPost]
        public ActionResult SubmitRequest(HC_LegalPermit_Request smodel, string breakdowncost, HttpPostedFileBase LegalDocument)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            HC_LegalPermit_Request request = new HC_LegalPermit_Request();
            request.RefQuotationNo = smodel.RefQuotationNo;
            request.SupplierName = smodel.SupplierName;
            request.PresidentDirector = smodel.PresidentDirector;
            request.Address = smodel.Address;
            request.TelpNo = smodel.TelpNo;
            request.FaxNo = smodel.FaxNo;
            request.ProjectName = smodel.ProjectName;
            request.Price = decimal.Parse(smodel.Price.ToString().Replace(".", ""));
            request.Discount = decimal.Parse(smodel.Discount.ToString().Replace(".", ""));
            request.FinalPrice = decimal.Parse(smodel.FinalPrice.ToString().Replace(".", "")); 
            request.TermPayment = smodel.TermPayment;
            request.LeadTime = smodel.LeadTime;
            request.Penalty = int.Parse(smodel.Penalty.ToString().Replace(".", "")); 
            request.BankAccountDetail = smodel.BankAccountDetail;
            request.Warranty = smodel.Warranty;
            request.PlanStartDate = smodel.PlanStartDate;
            request.PlanEndDate = smodel.PlanEndDate;
            request.CreateBy = CurrUser.NIK;
            request.CreateTime = DateTime.Now;

            if (LegalDocument.ContentLength > 0)
            {
                string extension = Path.GetExtension(LegalDocument.FileName);
                string filePath = Server.MapPath("~/Files/HC/LegalPermit/");
                string fileName = LegalDocument.FileName;
                filePath = filePath + fileName;

                LegalDocument.SaveAs(filePath);

                request.LegalDocument = fileName;
            } else
            {
                request.LegalDocument = "";
            }

            dblp.HC_LegalPermit_Request.Add(request);

            int r = dblp.SaveChanges();

            int ReqID = request.ID;

            int p = 0;
            if (r > 0)
            {
                // insert progress from prosedur template
                var prosedur = dblp.HC_LegalPermit_Flow_Detail.Where(w => w.Flow_ID == "1").ToList();
                foreach(var prs in prosedur)
                {
                    HC_LegalPermit_Request_Progress itemPrg= new HC_LegalPermit_Request_Progress
                    {
                         StepID = prs.ID,
                         ReqID = ReqID,
                         StepNumber = prs.StepNumber,
                         StepName = prs.StepName,
                         PIC = prs.PIC,
                         Status = 0,
                         Estimation_Time = prs.Estimation_Time
                    };
                    dblp.HC_LegalPermit_Request_Progress.Add(itemPrg);
                    int ip = dblp.SaveChanges();
                    if (ip > 0)
                    {
                        p++;
                    }
                }

            }

            // Deserialisasi ProdukList JSON menjadi List<ProdukModel>
            int s = 0;
            var detailBreakdown = JsonConvert.DeserializeObject<List<HC_LegalPermit_Request_BreakdownCost>>(breakdowncost);
            foreach (var brkdown in detailBreakdown)
            {

                HC_LegalPermit_Request_BreakdownCost item = new HC_LegalPermit_Request_BreakdownCost
                {
                    ItemName = brkdown.ItemName,
                    Price = decimal.Parse(brkdown.Price.ToString().Replace(".","")),
                    ReqID = ReqID
                };

                dblp.HC_LegalPermit_Request_BreakdownCost.Add(item);
                int i = dblp.SaveChanges();
                if(i > 0)
                {
                    s++;
                }
            }

            if(s > 0 && r > 0 && p > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Save success",
                    data = request,
                    breakdownCost = detailBreakdown,
                    detailBreakdown = detailBreakdown
                });
            } else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Failed Save Request",
                    data = request,
                    breakdownCost = detailBreakdown
                });
            }
        }

        public ActionResult RequestDetail(int ID)
        {
            var information = dblp.HC_LegalPermit_Request.Where(w => w.ID == ID).FirstOrDefault();
            var progress = dblp.HC_LegalPermit_Request_Progress.Where(w => w.ReqID == ID).ToList().OrderBy(o=>o.StepNumber);
            var breakdownCost = dblp.HC_LegalPermit_Request_BreakdownCost.Where(w => w.ReqID == ID).ToList();

            ViewBag.information = information;
            ViewBag.progress = progress;
            ViewBag.breakdownCost = breakdownCost;
            return View();

        }

        public ActionResult ShowPdf(string fileName)
        {
            string filePath = Server.MapPath("~/Files/HC/LegalPermit/"+fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound("File Not Found");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf");
        }
        public ActionResult Recap()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();
            

            ViewBag.subSection = CurrUser.SubSectionName;
            ViewBag.NavHide = true;
            ViewBag.userAdmin = userAdmin;
            ViewBag.User = CurrUser;
            return View();
        }

        public JsonResult GetRecapAgreementData()
        {            
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            string status = "";
            string badge = "";

            string[] userAdmin = { "632.11.12", "831.03.19", "629.01.13" };

            Expression<Func<HC_LegalPermit_Recap_Agreement, bool>> filter = null;
            if (userAdmin.Contains(currUser))
            {
                filter = b => 1 == 1;
            }
            else
            {
                var idList = dblp.HC_LegalPermit_Share_User
                    .Where(a => a.NIK == currUser && a.LegalType == "Agreement")
                    .Select(a => a.Legal_ID);
                filter = b => idList.Contains(b.ID);
            }

            var spl = dblp.HC_LegalPermit_Recap_Agreement
                            .Where(filter).OrderByDescending(o=>o.ID)
                            .ToList();

            //var spl = dblp.HC_LegalPermit_Recap_Agreement.OrderByDescending(o => o.ID).ToList();
            var CountRow = spl.Count();
            List<Tbl_HC_LegalPermit_Recap_Agreement> actions = new List<Tbl_HC_LegalPermit_Recap_Agreement>();
            int No = 0;
            foreach (var Item in spl)
            {
                //var Tools = "";
                No++;
                var UrlAction = Url.Action("AgreementDetail", "LegalPermit", new { area = "HC", ID = Item.ID.ToString(), Legal = "Agreement" });
                var UrlAttachment = Url.Action("ViewPDF", "LegalPermit", new { area = "HC", fileName = Item.Attachment });

                // define status
                if (Item.PeriodeEnd <= DateTime.Now)
                {
                    status = "Invalid";
                    badge = "danger";
                }
                else
                {
                    status = "Valid";
                    badge = "success";
                }
                //var editButton = "";
                var shareButton = "<a href=\"#\" title=\"Share to User\" class=\"btn btn-sm btn-primary\" id=\"shareToUser\" data-id=\"" + Item.ID + "\" data-legal=\"Agreement\"><i class=\"fa fa-share-alt\"></i> </a>";
                var renewalButton = "";
                if (Item.IsRenewal == 1)
                {
                    var qRenewal = dblp.HC_LegalPermit_Recap_Agreement.Where(w => w.ID == Item.RenewalRefID).FirstOrDefault();
                    renewalButton = "<a href=\"" + UrlAction + "\" class=\"link-offset-2 link-offset-3-hover link-underline link-underline-opacity-0 link-underline-opacity-75-hover\">" + qRenewal.AgreementNo + "</a>";
                } else
                {
                    renewalButton = "<a href =\"#\" title=\"Renewal Document\" class=\"btn btn-sm btn-warning\" id=\"btnRenewal\" data-id=\"" + Item.ID + "\" data-legal=\"Agreement\"><i class=\"fa fa-refresh\"></i> </a>";
                }
                
                var bNote = "<span class=\"badge badge-pill badge-" + badge + "\">" + status + "</span>";

                actions.Add(
                    new Tbl_HC_LegalPermit_Recap_Agreement
                    {
                        Name = Item.AgreementName,
                        No = No,
                        SecondParty = Item.SecondParty,
                        AgreementNo = "<a href=\"" + UrlAction + "\" class=\"link-offset-2 link-offset-3-hover link-underline link-underline-opacity-0 link-underline-opacity-75-hover\">" + Item.AgreementNo+ "</a>",
                        Attachment = "<a href=\"" + UrlAttachment + "\" class=\"link-offset-2 link-offset-3-hover link-underline link-underline-opacity-0 link-underline-opacity-75-hover\" title=\"Share to user\" target=\"_blank\">" + Item.Attachment + "</a>",
                        AgreementType = Item.AgreementType,
                        Note = bNote,
                        PeriodeStart = Item.PeriodeStart.ToString("dd MMM, yyyy"),
                        //IsActive = activation,
                        PeriodeEnd = Item.PeriodeEnd.ToString("dd MMM, yyyy"),
                        btnAlert = "<span>" + shareButton + " " + renewalButton + "</span>"
                    }); 
            }

            var jsonResult = Json(new { rows = actions, totalNotFiltered = CountRow, total = CountRow }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public JsonResult GetRecapPermitData()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            string[] userAdmin = { "632.11.12", "831.03.19", "629.01.13" };

            Expression<Func<HC_LegalPermit_Recap_Permit, bool>> filter = null;
            if (userAdmin.Contains(currUser))
            {
                filter = b => 1 == 1;
            }
            else
            {
                //get share user
                var idList = dblp.HC_LegalPermit_Share_User
                    .Where(a => a.NIK == currUser && a.LegalType == "Permit")
                    .Select(a => a.Legal_ID);
                filter = b => idList.Contains(b.ID);
            }

            var spl = dblp.HC_LegalPermit_Recap_Permit
                            .Where(filter).OrderByDescending(o=>o.ID)
                            .ToList();

            var CountRow = spl.Count();
            List<Tbl_HC_LegalPermit_Recap_Permit> actions = new List<Tbl_HC_LegalPermit_Recap_Permit>();
            int No = 0;
            foreach (var Item in spl)
            {
                No++;

                string status = "";
                string badge = "";
                // get PIC Name
                var PIC = db.V_Users_Active.Where(w => w.NIK == Item.PIC).FirstOrDefault();

                // date expired condition
                string expired = "";
                if (string.IsNullOrEmpty(Item.Expired.ToString()))
                {
                    expired = "empty date";
                }
                else
                {
                    expired = Item.Expired?.ToString("dd MMM yyyy");
                    //var Tools = "";
                }
                // define status & badge
                if (Item.Expired <= DateTime.Now)
                {
                    status = "Expired";
                    badge = "danger";
                    
                } else if (expired == "empty date")
                {
                    status = "Not Found End Date";
                    badge = "secondary";
                } else
                {
                    status = "Valid";
                    badge = "success";
                }

                var UrlAction = Url.Action("AgreementDetail", "LegalPermit", new { area = "HC", ID = Item.ID.ToString(), Legal = "Permit" });
                var UrlAttachment = Url.Action("ViewPDF", "LegalPermit", new { area = "HC", fileName = Item.Attachment });
                //var editButton = "";
                var alertButton = "<a href=\"#\" title=\"Share to user\" class=\"btn btn-sm btn-primary\" id=\"shareToUser\" data-id=\"" + Item.ID + "\" data-legal=\"Permit\"><i class=\"fa fa-share-alt\"></i></a>";
                var bNote = "<span class=\"badge badge-pill badge-" + badge + "\">" + status + "</span>";
                var renewalButton = "";
                if (Item.IsRenewal == 1)
                {
                    var qRenewal = dblp.HC_LegalPermit_Recap_Permit.Where(w => w.ID == Item.RenewalRefID).FirstOrDefault();
                    renewalButton = "<a href=\"" + UrlAction + "\" class=\"link-offset-2 link-offset-3-hover link-underline link-underline-opacity-0 link-underline-opacity-75-hover\">" + qRenewal.Number + "</a>";
                }
                else
                {
                    renewalButton = "<a href =\"#\" title=\"Renewal Document\" class=\"btn btn-sm btn-warning\" id=\"btnRenewal\" data-id=\"" + Item.ID + "\" data-legal=\"Permit\"><i class=\"fa fa-refresh\"></i> </a>";
                }
                actions.Add(
                    new Tbl_HC_LegalPermit_Recap_Permit
                    {
                        Permit = Item.Permit,
                        No = No,
                        Number = "<a href=\"" + UrlAction + "\" class=\"link-offset-2 link-offset-3-hover link-underline link-underline-opacity-0 link-underline-opacity-75-hover\">" + Item.Number + "</a>",
                        SectionHandling = Item.SectionHandling,
                        Goverment = Item.Goverment,
                        PIC = PIC.Name,
                        Status = bNote,
                        Expired = expired,
                        Attachment = "<a href=\"" + UrlAttachment + "\" class=\"link-offset-2 link-offset-3-hover link-underline link-underline-opacity-0 link-underline-opacity-75-hover\" title=\"Share to user\" target=\"_blank\" >" + Item.Attachment + "</a>",
                        btnAlert =  "<span>" + alertButton + " " + renewalButton + "</span>"
                    }); ;
            }

            var jsonResult = Json(new { rows = actions, totalNotFiltered = CountRow, total = CountRow }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        
        public ActionResult formAddAgreement(string ID)
        {
            ViewBag.ID = ID;
            var ListEmail = db.V_Users_Active.Where(w => w.Email != null).GroupBy(g => g.Email).Select(s => new ListEmail { Email = s.Key }).ToList();
            var ListVendor = db.V_AXVendorList.ToList();
            var ListUser = db.V_Users_Active.Where(w => w.SectionName == "GA" && w.Status == "Permanent" && w.PositionName != "OPERATOR").ToList();
            ViewBag.ListEmail = ListEmail;
            ViewBag.ListVendor = ListVendor;
            ViewBag.ListUser = ListUser;
            if (ID == "1")
            {
                 var section = db.Users_Section_AX.ToList();
                ViewBag.Section = section;

            }
            
            return View();
        }
        [HttpPost]
        public ActionResult AddAgreement(HC_LegalPermit_Recap_Agreement smodelAgreement, HC_LegalPermit_Recap_Permit smodelPermit, IT_Reminder smodelReminder, string PeriodeStart, string PeriodeEnd, string[] selReminderUser)
        {
            int i;
            int insertReminder;

            // get formID
            var idForm = Request.Form.Get("IDForm");

            HttpPostedFileBase uploadFile = Request.Files["FileAttachment"];
            // get file attachment
            string filePath = "";
            string fileName = "";
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                fileName = uploadFile.FileName;
                string extension = Path.GetExtension(fileName);
                filePath = Server.MapPath("~/Files/HC/LegalPermit/");
                filePath = filePath + fileName;                
            }
            else
            {
                fileName = "not found";
                filePath = "not found";
            }

            /* insert reminder */
            IT_Reminder reminder = new IT_Reminder();
            reminder.ReminderTitle = idForm == "0" ? smodelAgreement.AgreementName : smodelPermit.Permit;
            reminder.Module = idForm == "0" ? "Agreement" : "Permit";
            reminder.Type = "internal";
            reminder.Thirdparty = idForm == "0" ?  smodelAgreement.SecondParty : smodelPermit.Goverment;
            reminder.DueDate = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            reminder.Description = idForm == "0" ? smodelAgreement.AgreementName : smodelPermit.Permit;
            reminder.NotifStart = Convert.ToInt32(Request.Form.Get("NotifStart")) * -1;
            reminder.NotifTime = Request.Form.Get("NotifTime");
            reminder.IntervalRepetReminderType = "OneTime"; // no repeat reminder
            reminder.IntervalRepeatReminderNumber = 0;
            reminder.IntervalRepeatNotifType = Request.Form.Get("IntervalRepeatNotifType");
            reminder.IntervalRepeatNotifNumber = smodelReminder.IntervalRepeatNotifNumber;
            reminder.CreateTime = DateTime.Now;
            reminder.CreateBy = ((ClaimsIdentity)User.Identity).GetUserId();
            reminder.Attachment = fileName;
            reminder.IsActive = 1;

            dbr.IT_Reminder.Add(reminder);
            insertReminder = dbr.SaveChanges();
            int ReminderID = reminder.ID;

            
            if (idForm == "0")
            {
                /* insert agreement process */
                HC_LegalPermit_Recap_Agreement data = new HC_LegalPermit_Recap_Agreement();
                data.AgreementName = smodelAgreement.AgreementName;
                data.AgreementNo = smodelAgreement.AgreementNo;
                data.SecondParty = smodelAgreement.SecondParty;
                data.PeriodeStart = DateTime.ParseExact(PeriodeStart, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                data.PeriodeEnd = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                data.AgreementType = smodelAgreement.AgreementType;
                data.ReminderID = ReminderID;
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    data.Attachment = fileName;
                    uploadFile.SaveAs(filePath);
                }

                dblp.HC_LegalPermit_Recap_Agreement.Add(data);
                i = dblp.SaveChanges();
                if (i > 0)
                {
                    
                    if(insertReminder > 0)
                    {
                        // insert recipient user
                        foreach (var user in selReminderUser)
                        {
                            //var userInfo = db.V_Users_Active.Where(w => w.NIK == user).FirstOrDefault();
                            IT_Reminder_User userList = new IT_Reminder_User();
                            userList.ReminderID = reminder.ID;
                            userList.SendToUser = user;
                            userList.SendToUserEmail = user;
                            userList.IsActive = 1;

                            dbr.IT_Reminder_User.Add(userList);
                            int ins_user = dbr.SaveChanges();
                        }

                        IT_Reminder_Task newTask = new IT_Reminder_Task();
                        newTask.ReminderID = reminder.ID;
                        newTask.ReminderDate = reminder.DueDate.AddDays(reminder.NotifStart).Date;
                        newTask.ReminderTime = Convert.ToInt32(smodelReminder.NotifTime);
                        newTask.ReminderDueDate = reminder.DueDate;

                        dbr.IT_Reminder_Task.Add(newTask);
                        int ins_task = dbr.SaveChanges();
                    }
                } else
                {
                    
                    return Json(new
                    {
                        status = '0',
                        msg = "Failed to save Agreement",
                        //data = smodel
                    });
                }
            } else if (idForm == "1")
            {
                HC_LegalPermit_Recap_Permit data = new HC_LegalPermit_Recap_Permit();
                data.Permit = smodelPermit.Permit;
                data.Number = smodelPermit.Number;
                data.SectionHandling = smodelPermit.SectionHandling;
                data.Goverment = smodelPermit.Goverment;
                data.Expired = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                data.PIC = smodelPermit.PIC;
                data.Status = "Valid";
                data.ReminderID = ReminderID;
                //HttpPostedFileBase uploadFile = Request.Files["FileAttachment"];
                //// get file attachment
                //string filePath = "";
                //string fileName = "";
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    //fileName = uploadFile.FileName;
                    //string extension = Path.GetExtension(fileName);
                    //filePath = Server.MapPath("~/Files/HC/LegalPermit/");
                    //filePath = filePath + fileName;

                    data.Attachment = fileName;
                    uploadFile.SaveAs(filePath);
                }
                //else
                //{
                //    fileName = "not found";
                //    filePath = "not found";
                //}
                dblp.HC_LegalPermit_Recap_Permit.Add(data);
                i = dblp.SaveChanges();
                if (i > 0)
                {
                    ///* insert reminder */
                    //IT_Reminder reminder = new IT_Reminder();
                    //reminder.ReminderTitle = smodelPermit.Permit;
                    //reminder.Module = "Permit";
                    //reminder.Type = "internal";
                    //reminder.Thirdparty = smodelPermit.Goverment;
                    //reminder.DueDate = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    //reminder.Description = smodelPermit.Permit;
                    //reminder.NotifStart = Convert.ToInt32(Request.Form.Get("NotifStart")) * -1;
                    //reminder.NotifTime = Request.Form.Get("NotifTime");
                    //reminder.IntervalRepetReminderType = "OneTime"; // no repeat reminder
                    //reminder.IntervalRepeatReminderNumber = 0;
                    //reminder.IntervalRepeatNotifType = Request.Form.Get("IntervalRepeatNotifType");
                    //reminder.IntervalRepeatNotifNumber = smodelReminder.IntervalRepeatNotifNumber;
                    //reminder.CreateTime = DateTime.Now;
                    //reminder.CreateBy = ((ClaimsIdentity)User.Identity).GetUserId();
                    //reminder.Attachment = fileName;
                    //reminder.IsActive = 1;

                    //dbr.IT_Reminder.Add(reminder);
                    //insertReminder = dbr.SaveChanges();
                    if (insertReminder > 0)
                    {
                        // insert recipient user
                        foreach (var user in selReminderUser)
                        {
                            //var userInfo = db.V_Users_Active.Where(w => w.NIK == user).FirstOrDefault();
                            IT_Reminder_User userList = new IT_Reminder_User();
                            userList.ReminderID = reminder.ID;
                            userList.SendToUser = user;
                            userList.SendToUserEmail = user;
                            userList.IsActive = 1;

                            dbr.IT_Reminder_User.Add(userList);
                            int ins_user = dbr.SaveChanges();
                        }

                        IT_Reminder_Task newTask = new IT_Reminder_Task();
                        newTask.ReminderID = reminder.ID;
                        newTask.ReminderDate = reminder.DueDate.AddDays(reminder.NotifStart).Date;
                        newTask.ReminderTime = Convert.ToInt32(smodelReminder.NotifTime);
                        newTask.ReminderDueDate = reminder.DueDate;

                        dbr.IT_Reminder_Task.Add(newTask);
                        int ins_task = dbr.SaveChanges();
                    }
                }
                else
                {

                    return Json(new
                    {
                        status = '0',
                        msg = "Failed to save Agreement",
                        //data = smodel
                    });
                }
            } 
            else
            {
                i = 0;
            }
            
            if (i > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Save success",
                    //data = smodel,
                    //fileName = fileName,
                    //filepath = filePath
                });
            } else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Save Failed",
                    //data = smodel
                });
            }
            
        }
        [HttpPost]
        public ActionResult UpdateAgreement(HC_LegalPermit_Recap_Agreement smodelAgreement, HC_LegalPermit_Recap_Permit smodelPermit, IT_Reminder smodelReminder, string PeriodeStart, string PeriodeEnd, string[] selReminderUser)
        {
            int i;

            /* step 1 - upload file */
            HttpPostedFileBase uploadFile = Request.Files["FileAttachment"];
            // get file attachment
            string filePath = "";
            string fileName = "";
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                fileName = uploadFile.FileName;
                string extension = Path.GetExtension(fileName);
                filePath = Server.MapPath("~/Files/HC/LegalPermit/");
                filePath = filePath + fileName;
            }
            else
            {
                fileName = "not found";
                filePath = "not found";
            }
            
            // get formID
            var idForm = Request.Form.Get("IDForm");
            if (idForm == "Agreement")
            {
                /* update agreement process */
                var dataAgreement = dblp.HC_LegalPermit_Recap_Agreement.Where(w => w.ID == smodelAgreement.ID).FirstOrDefault();
                dataAgreement.AgreementName = smodelAgreement.AgreementName;
                dataAgreement.AgreementNo = smodelAgreement.AgreementNo;
                dataAgreement.SecondParty = smodelAgreement.SecondParty;
                dataAgreement.PeriodeStart = DateTime.ParseExact(PeriodeStart, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                dataAgreement.PeriodeEnd = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                dataAgreement.AgreementType = smodelAgreement.AgreementType;
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    dataAgreement.Attachment = fileName;
                    uploadFile.SaveAs(filePath);
                }

                i = dblp.SaveChanges();
                if (i > 0)
                {
                    //update reminder header information
                    var reminder = dbr.IT_Reminder.Where(w => w.ID == dataAgreement.ReminderID).FirstOrDefault();
                    reminder.ReminderTitle = dataAgreement.AgreementName;
                    reminder.Thirdparty = dataAgreement.SecondParty;
                    reminder.Description = dataAgreement.AgreementName;
                    reminder.DueDate = dataAgreement.PeriodeEnd;

                    if (uploadFile != null && uploadFile.ContentLength > 0)
                    {
                        reminder.Attachment = fileName;
                    }

                    // update reminder task
                    var reminderTask = dbr.IT_Reminder_Task.Where(w => w.ReminderID == dataAgreement.ReminderID).FirstOrDefault();
                    reminderTask.ReminderDate = reminder.DueDate.AddDays(reminder.NotifStart).Date;
                    reminderTask.ReminderTime = Convert.ToInt32(reminder.NotifTime);
                    reminderTask.ReminderDueDate = reminder.DueDate;
                    dbr.SaveChanges();
                }
                else
                {

                    return Json(new
                    {
                        status = '0',
                        msg = "Failed to save Agreement",
                        //data = smodel
                    });
                }
            }
            else if (idForm == "Permit")
            {
                var dataPermit = dblp.HC_LegalPermit_Recap_Permit.Where(w => w.ID == smodelPermit.ID).FirstOrDefault();
                dataPermit.Permit = smodelPermit.Permit;
                dataPermit.Number = smodelPermit.Number;
                dataPermit.SectionHandling = smodelPermit.SectionHandling;
                dataPermit.Goverment = smodelPermit.Goverment;
                dataPermit.Expired = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                dataPermit.PIC = smodelPermit.PIC;
                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    dataPermit.Attachment = fileName;
                    uploadFile.SaveAs(filePath);
                }
                i = dblp.SaveChanges();
                if (i > 0)
                {
                    /////update reminder header information
                    var reminder = dbr.IT_Reminder.Where(w => w.ID == dataPermit.ReminderID).FirstOrDefault();
                    reminder.ReminderTitle = dataPermit.Permit;
                    reminder.Thirdparty = dataPermit.Goverment;
                    reminder.Description = dataPermit.Permit;
                    reminder.DueDate = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    if (uploadFile != null && uploadFile.ContentLength > 0)
                    {
                        reminder.Attachment = fileName;
                    }
                    var reminderTask = dbr.IT_Reminder_Task.Where(w => w.ReminderID == dataPermit.ReminderID).FirstOrDefault();
                    reminderTask.ReminderDate = reminder.DueDate.AddDays(reminder.NotifStart).Date;
                    reminderTask.ReminderTime = Convert.ToInt32(reminder.NotifTime);
                    reminderTask.ReminderDueDate = reminder.DueDate;
                    dbr.SaveChanges();
                }
                else
                {

                    return Json(new
                    {
                        status = '0',
                        msg = "Failed to save Document",
                    });
                }
            }
            else
            {
                i = 0;
            }

            if (i > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Save success",
                });
            }
            else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Save Failed",
                    //data = smodel
                });
            }

        }
        [HttpPost]
        public ActionResult UpdateReminder(IT_Reminder smodel, string[] selReminderUser)
        {
            int u;
            int ReminerID = smodel.ID;

            // update table IT_Reminder            
            var ReminderData = dbr.IT_Reminder.Where(w => w.ID == smodel.ID).FirstOrDefault();
            ReminderData.NotifStart = smodel.NotifStart * -1;
            ReminderData.NotifTime = smodel.NotifTime;
            ReminderData.IntervalRepeatNotifNumber = smodel.IntervalRepeatNotifNumber;
            ReminderData.IntervalRepeatNotifType = smodel.IntervalRepeatNotifType;
            u = dbr.SaveChanges();

            // update table IT_Reminder_Task
            var ReminderTask = dbr.IT_Reminder_Task.Where(w => w.ReminderID == smodel.ID).FirstOrDefault();
            ReminderTask.ReminderDate = ReminderData.DueDate.AddDays(ReminderData.NotifStart).Date;
            ReminderTask.ReminderTime = Convert.ToInt32(ReminderData.NotifTime);
            ReminderTask.ReminderDueDate = ReminderData.DueDate;
            dbr.SaveChanges();

            // delete & insert reminder user
            var ReminderUser = dbr.IT_Reminder_User.Where(w => w.ReminderID == smodel.ID).ToList();
            dbr.IT_Reminder_User.RemoveRange(ReminderUser);
            int r = 0;
            foreach (var user in selReminderUser)
            {
                //var userInfo = db.V_Users_Active.Where(w => w.NIK == user).FirstOrDefault();
                IT_Reminder_User userList = new IT_Reminder_User();
                userList.ReminderID = smodel.ID;
                userList.SendToUser = user;
                userList.SendToUserEmail = user;
                userList.IsActive = 1;

                dbr.IT_Reminder_User.Add(userList);
                int ins_user = dbr.SaveChanges();
                r++;
            }

            if (u > 0 || r > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Save success",
                });
            }
            else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Save Failed",
                    //data = smodel
                });
            }

        }
        [HttpGet]
        public ActionResult formRenewal(int DocumentID, string LegalType)
        {
            var qReminder = from p in dbr.IT_Reminder select p;
            var qReminderUser = from q in dbr.IT_Reminder_User select q;
            if (LegalType == "Agreement")
            {
                var qAgreement = dblp.HC_LegalPermit_Recap_Agreement.Where(w => w.ID == DocumentID).FirstOrDefault();

                qReminder = from p in qReminder where p.ID == qAgreement.ReminderID select p;
                qReminderUser = from q in qReminderUser where q.ReminderID == qAgreement.ReminderID select q;

                ViewBag.agreement = qAgreement;
                ViewBag.PrevAgreementID = DocumentID;
                ViewBag.PrevAgreementNo = qAgreement.AgreementNo;
            }
            else
            {
                var section = db.Users_Section_AX.ToList();
                var qPermit = dblp.HC_LegalPermit_Recap_Permit.Where(w => w.ID == DocumentID).FirstOrDefault();
                var qPICUser = db.V_Users_Active.Where(w => w.NIK == qPermit.PIC).ToList();
                qReminder = from p in qReminder where p.ID == qPermit.ReminderID select p;
                qReminderUser = from q in qReminderUser where q.ReminderID == qPermit.ReminderID select q;

                ViewBag.permit = qPermit;
                ViewBag.Section = section;
                ViewBag.PIC = qPICUser;
                ViewBag.PrevAgreementID = DocumentID;
                ViewBag.PrevAgreementNo = qPermit.Number;
            }

            var ListEmail = db.V_Users_Active.Where(w => w.Email != null).GroupBy(g => g.Email).Select(s => new ListEmail { Email = s.Key }).ToList();
            var ListVendor = db.V_AXVendorList.ToList();
            var ListUser = db.V_Users_Active.Where(w => w.SectionName == "GA" && w.Status == "Permanent" && w.PositionName != "OPERATOR").ToList();


            ViewBag.ListEmail = ListEmail;
            ViewBag.ListVendor = ListVendor;
            ViewBag.ListUser = ListUser;
            ViewBag.Reminder = qReminder.FirstOrDefault();
            ViewBag.ReminderUser = qReminderUser.ToList();
            

            ViewBag.LegalType = LegalType;

            return View();
        }
        [HttpPost]
        public ActionResult AddRenewal(HC_LegalPermit_Recap_Agreement smodelAgreement, HC_LegalPermit_Recap_Permit smodelPermit, IT_Reminder smodelReminder, string PeriodeStart, string PeriodeEnd, string[] selReminderUser, int PrevAgreementID, string PrevAgreementNo)
        {
            int i;
            int insertReminder;

            HttpPostedFileBase uploadFile = Request.Files["FileAttachment"];
            // get file attachment
            string filePath = "";
            string fileName = "";
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                fileName = uploadFile.FileName;
                string extension = Path.GetExtension(fileName);
                filePath = Server.MapPath("~/Files/HC/LegalPermit/");
                filePath = filePath + fileName;
            }
            else
            {
                fileName = "not found";
                filePath = "not found";
            }

            /* insert reminder */
            IT_Reminder reminder = new IT_Reminder();
            reminder.ReminderTitle = smodelAgreement.AgreementName;
            reminder.Module = "Agreement";
            reminder.Type = "internal";
            reminder.Thirdparty = smodelAgreement.SecondParty;
            reminder.DueDate = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            reminder.Description = smodelAgreement.AgreementName;
            reminder.NotifStart = Convert.ToInt32(Request.Form.Get("NotifStart")) * -1;
            reminder.NotifTime = Request.Form.Get("NotifTime");
            reminder.IntervalRepetReminderType = "OneTime"; // no repeat reminder
            reminder.IntervalRepeatReminderNumber = 0;
            reminder.IntervalRepeatNotifType = Request.Form.Get("IntervalRepeatNotifType");
            reminder.IntervalRepeatNotifNumber = smodelReminder.IntervalRepeatNotifNumber;
            reminder.CreateTime = DateTime.Now;
            reminder.CreateBy = ((ClaimsIdentity)User.Identity).GetUserId();
            reminder.Attachment = fileName;
            reminder.IsActive = 1;

            dbr.IT_Reminder.Add(reminder);
            insertReminder = dbr.SaveChanges();
            int ReminderID = reminder.ID;

            // get formID
            var idForm = Request.Form.Get("IDForm");
            if (idForm == "Agreement")
            {
                /* insert agreement process */
                HC_LegalPermit_Recap_Agreement data = new HC_LegalPermit_Recap_Agreement();
                data.AgreementName = smodelAgreement.AgreementName;
                data.AgreementNo = smodelAgreement.AgreementNo;
                data.SecondParty = smodelAgreement.SecondParty;
                data.PeriodeStart = DateTime.ParseExact(PeriodeStart, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                data.PeriodeEnd = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                data.AgreementType = smodelAgreement.AgreementType;
                data.ReminderID = ReminderID;
                data.PrevAgreementID = PrevAgreementID;
                data.PrevAgreementNo = PrevAgreementNo;

                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    data.Attachment = fileName;
                    uploadFile.SaveAs(filePath);
                }

                dblp.HC_LegalPermit_Recap_Agreement.Add(data);
                i = dblp.SaveChanges();

                // update prev agreement status is Renewal
                var qPrevAgreement = dblp.HC_LegalPermit_Recap_Agreement.Where(w => w.ID == PrevAgreementID).FirstOrDefault();
                qPrevAgreement.IsRenewal = 1;
                qPrevAgreement.RenewalRefID = data.ID;
                dblp.SaveChanges();

                // insert share user
                var shareUserList = dblp.HC_LegalPermit_Share_User.Where(w => w.Legal_ID == PrevAgreementID && w.LegalType == "Agreement" && w.IsDelete == 0).ToList();
                foreach(var shareUser in shareUserList)
                {
                    HC_LegalPermit_Share_User shr = new HC_LegalPermit_Share_User();
                    shr.NIK = shareUser.NIK;
                    shr.IsDelete = shareUser.IsDelete;
                    shr.CreateBy = shareUser.CreateBy;
                    shr.CreateTime = shareUser.CreateTime;
                    shr.Legal_ID = data.ID;
                    shr.LegalType = "Agreement";
                    dblp.HC_LegalPermit_Share_User.Add(shr);
                }
                dblp.SaveChanges();

                if (i > 0)
                {
                    // insert reminder user
                    if (insertReminder > 0)
                    {
                        // insert recipient user
                        foreach (var user in selReminderUser)
                        {
                            //var userInfo = db.V_Users_Active.Where(w => w.NIK == user).FirstOrDefault();
                            IT_Reminder_User userList = new IT_Reminder_User();
                            userList.ReminderID = reminder.ID;
                            userList.SendToUser = user;
                            userList.SendToUserEmail = user;
                            userList.IsActive = 1;

                            dbr.IT_Reminder_User.Add(userList);
                            int ins_user = dbr.SaveChanges();
                        }
                        // insert reminder task
                        IT_Reminder_Task newTask = new IT_Reminder_Task();
                        newTask.ReminderID = reminder.ID;
                        newTask.ReminderDate = reminder.DueDate.AddDays(reminder.NotifStart).Date;
                        newTask.ReminderTime = Convert.ToInt32(smodelReminder.NotifTime);
                        newTask.ReminderDueDate = reminder.DueDate;

                        dbr.IT_Reminder_Task.Add(newTask);
                        int ins_task = dbr.SaveChanges();
                    }
                }
                else
                {

                    return Json(new
                    {
                        status = 0,
                        msg = "Failed to save Agreement",
                        //data = smodel
                    });
                }
            }
            else if (idForm == "Permit")
            {
                HC_LegalPermit_Recap_Permit data = new HC_LegalPermit_Recap_Permit();
                data.Permit = smodelPermit.Permit;
                data.Number = smodelPermit.Number;
                data.SectionHandling = smodelPermit.SectionHandling;
                data.Goverment = smodelPermit.Goverment;
                data.Expired = DateTime.ParseExact(PeriodeEnd, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                data.PIC = smodelPermit.PIC;
                data.Status = "Valid";
                data.ReminderID = ReminderID;
                data.PrevPermitID = PrevAgreementID;
                data.PrevPermitNo = PrevAgreementNo;

                if (uploadFile != null && uploadFile.ContentLength > 0)
                {
                    data.Attachment = fileName;
                    uploadFile.SaveAs(filePath);
                }
                dblp.HC_LegalPermit_Recap_Permit.Add(data);
                i = dblp.SaveChanges();

                // update prev agreement status is Renewal
                var qPrevPermit = dblp.HC_LegalPermit_Recap_Permit.Where(w => w.ID == PrevAgreementID).FirstOrDefault();
                qPrevPermit.IsRenewal = 1;
                qPrevPermit.RenewalRefID = data.ID;
                dblp.SaveChanges();

                // insert share user
                var shareUserList = dblp.HC_LegalPermit_Share_User.Where(w => w.Legal_ID == PrevAgreementID && w.LegalType == "Permit" && w.IsDelete == 0).ToList();
                foreach (var shareUser in shareUserList)
                {
                    HC_LegalPermit_Share_User shr = new HC_LegalPermit_Share_User();
                    shr.NIK = shareUser.NIK;
                    shr.IsDelete = shareUser.IsDelete;
                    shr.CreateBy = shareUser.CreateBy;
                    shr.CreateTime = shareUser.CreateTime;
                    shr.Legal_ID = data.ID;
                    shr.LegalType = "Permit";
                    dblp.HC_LegalPermit_Share_User.Add(shr);
                }
                dblp.SaveChanges();

                if (i > 0)
                {                    
                    if (insertReminder > 0)
                    {
                        // insert recipient user
                        foreach (var user in selReminderUser)
                        {
                            //var userInfo = db.V_Users_Active.Where(w => w.NIK == user).FirstOrDefault();
                            IT_Reminder_User userList = new IT_Reminder_User();
                            userList.ReminderID = reminder.ID;
                            userList.SendToUser = user;
                            userList.SendToUserEmail = user;
                            userList.IsActive = 1;

                            dbr.IT_Reminder_User.Add(userList);
                            int ins_user = dbr.SaveChanges();
                        }

                        IT_Reminder_Task newTask = new IT_Reminder_Task();
                        newTask.ReminderID = reminder.ID;
                        newTask.ReminderDate = reminder.DueDate.AddDays(reminder.NotifStart).Date;
                        newTask.ReminderTime = Convert.ToInt32(smodelReminder.NotifTime);
                        newTask.ReminderDueDate = reminder.DueDate;

                        dbr.IT_Reminder_Task.Add(newTask);
                        int ins_task = dbr.SaveChanges();
                    }
                }
                else
                {

                    return Json(new
                    {
                        status = 0,
                        msg = "Failed to save Agreement",
                    });
                }
            }
            else
            {
                i = 0;
            }

            if (i > 0)
            {
                return Json(new
                {
                    status = 1,
                    msg = "Save success"
                });
            }
            else
            {
                return Json(new
                {
                    status = 0,
                    msg = "Save Failed"
                });
            }

        }
        public ActionResult formShareToUser(string ID, string legalType)
        {
            var user = db.V_Users_Active.ToList();
            ViewBag.user = user;
            ViewBag.Legal_ID = ID;
            ViewBag.LegalType = legalType;

            return PartialView();
        }
        [HttpPost]
        public ActionResult AddShareToUser(HC_LegalPermit_Share_User smodel, string[] UserNIK)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            foreach(var usr in UserNIK)
            {
                HC_LegalPermit_Share_User data = new HC_LegalPermit_Share_User();
                data.NIK = usr;
                data.IsDelete = 0;
                data.Legal_ID = smodel.Legal_ID;
                data.LegalType = smodel.LegalType;
                data.CreateBy = currUser;
                data.CreateTime = DateTime.Now;
                dblp.HC_LegalPermit_Share_User.Add(data);
                
            }
            int s = dblp.SaveChanges();
            if (s > 0)
            {
                return Json(new
                {
                    status = '1',
                    msg = "Save success",
                    //data = smodel,
                    //fileName = fileName,
                    //filepath = filePath
                });
            }
            else
            {
                return Json(new
                {
                    status = '0',
                    msg = "Save Failed",
                    //data = smodel
                });
            }
        }
        public JsonResult GetShareUser(int id, string legalType)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            //var spl = db.V_SCM_Sparepart_Master_List.Where(w =>  w.CostName == CurrUser.CostName && w.ItemGroup == "MachineP" || w.ItemGroup == "Tooling").ToList();
            var spl = dblp.HC_LegalPermit_Share_User.Where(w=> w.Legal_ID == id && w.LegalType == legalType).OrderByDescending(o => o.ID).ToList();
            var CountRow = dblp.HC_LegalPermit_Share_User.Where(w => w.Legal_ID == id && w.LegalType == legalType).Count();
            List<Tbl_HC_LegalPermit_Share_User> actions = new List<Tbl_HC_LegalPermit_Share_User>();
            int No = 0;
            foreach (var Item in spl)
            {
                No++;

                // get user detail information
                var qUser = db.V_Users_Active.Where(w => w.NIK == Item.NIK).FirstOrDefault();
                actions.Add(
                    new Tbl_HC_LegalPermit_Share_User
                    {
                        NIK = Item.NIK,
                        No = No,
                        Name = qUser.Name,
                        Section = qUser.SectionName,
                        Action = "<a href=\"#\" title=\"delete user\" class=\"btn btn-sm btn-danger\" id=\"deleteShareUser\" data-id=\"" + Item.ID + "\" \"><i class=\"fa fa-trash\"></i></a>"
                    }); ;
            }

            var jsonResult = Json(new { rows = actions, totalNotFiltered = CountRow, total = CountRow }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult AgreementDetail(int ID, string Legal)
        {
            var qReminder = from p in dbr.IT_Reminder select p;
            var qReminderUser = from q in dbr.IT_Reminder_User select q;
            if (Legal == "Agreement")
            {
                var qAgreement = dblp.HC_LegalPermit_Recap_Agreement.Where(w => w.ID == ID).FirstOrDefault();
                
                qReminder = from p in qReminder where p.ID == qAgreement.ReminderID select p;
                qReminderUser = from q in qReminderUser where q.ReminderID == qAgreement.ReminderID select q;
                
                ViewBag.agreement = qAgreement;
            } 
            else 
            {
                var section = db.Users_Section_AX.ToList();
                var qPermit = dblp.HC_LegalPermit_Recap_Permit.Where(w => w.ID == ID).FirstOrDefault();
                var qPICUser = db.V_Users_Active.Where(w => w.NIK == qPermit.PIC).ToList();
                qReminder = from p in qReminder where p.ID == qPermit.ReminderID select p;
                qReminderUser = from q in qReminderUser where q.ReminderID == qPermit.ReminderID select q;
                
                ViewBag.permit = qPermit;
                ViewBag.Section = section;
                ViewBag.PIC = qPICUser;
            }
            
            var ListEmail = db.V_Users_Active.Where(w => w.Email != null).GroupBy(g => g.Email).Select(s => new ListEmail { Email = s.Key }).ToList();
            var ListNewEmail = dbr.IT_Reminder_User.GroupBy(g => g.SendToUser).Select(s => new ListEmail { Email = s.Key }).ToList();
            var ListVendor = db.V_AXVendorList.ToList();
            var ListUser = db.V_Users_Active.Where(w => w.SectionName == "GA" && w.Status == "Permanent" && w.PositionName != "OPERATOR").ToList();
            

            ViewBag.ListEmail = ListEmail.Union(ListNewEmail).GroupBy(x => x.Email).Select(y => new ListEmail { Email = y.Key }).ToList(); ;
            ViewBag.ListVendor = ListVendor;
            ViewBag.ListUser = ListUser;
            ViewBag.Reminder = qReminder.FirstOrDefault();
            ViewBag.ReminderUser = qReminderUser.ToList();            

            ViewBag.LegalType = Legal;
            
            return View();
        }
        [HttpGet]
        public ActionResult ViewPDF(string fileName)
        {
            string filePath = "~/Files/HC/LegalPermit/" + fileName;
            Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);

            return File(filePath, "application/pdf");
        }
    }
}