using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using MiniFinansRaporlama.Models;
using System;
using System.Collections.Generic;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MiniFinansRaporlama.Controllers
{
    public class FinanceController : Controller
    {
        private MiniFinansDBEntities db = new MiniFinansDBEntities();

        // =========================================================
        // INDEX
        // =========================================================
        public ActionResult Index(DateTime? startDate, DateTime? endDate, string category)
        {
            var query = db.Transactions.AsQueryable();

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.Date >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.Date < end);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category);
            }

            var transactions = query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            decimal toplamGelir = transactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            decimal toplamGider = transactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            decimal netBakiye = toplamGelir - toplamGider;

            var giderKategorileri = transactions
                .Where(x => x.Type == "Gider")
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Category) ? "Diğer" : x.Category)
                .Select(g => new CategorySummaryViewModel
                {
                    Category = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var topCategories = giderKategorileri
                .Take(5)
                .ToList();

            var aylikVeriler = transactions
                .GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(g => new MonthlyFinanceSummaryViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Income = g.Where(x => x.Type == "Gelir").Sum(x => x.Amount),
                    Expense = g.Where(x => x.Type == "Gider").Sum(x => x.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            var ayEtiketleri = aylikVeriler
                .Select(x => new DateTime(x.Year, x.Month, 1)
                .ToString("MMM yyyy", new CultureInfo("tr-TR")))
                .ToList();

            DateTime today = DateTime.Today;
            DateTime monthStart = new DateTime(today.Year, today.Month, 1);
            DateTime nextMonthStart = monthStart.AddMonths(1);
            DateTime prevMonthStart = monthStart.AddMonths(-1);

            var thisMonthTransactions = db.Transactions
                .Where(x => x.Date >= monthStart && x.Date < nextMonthStart)
                .ToList();

            var prevMonthTransactions = db.Transactions
                .Where(x => x.Date >= prevMonthStart && x.Date < monthStart)
                .ToList();

            decimal thisMonthIncome = thisMonthTransactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            decimal thisMonthExpense = thisMonthTransactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            decimal prevMonthIncome = prevMonthTransactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            decimal prevMonthExpense = prevMonthTransactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            decimal incomeChangePercentage = CalculateChangePercentage(prevMonthIncome, thisMonthIncome);
            decimal expenseChangePercentage = CalculateChangePercentage(prevMonthExpense, thisMonthExpense);

            int toplamKategoriSayisi = transactions
                .Where(x => !string.IsNullOrWhiteSpace(x.Category))
                .Select(x => x.Category)
                .Distinct()
                .Count();

            decimal ortalamaIslemTutari = transactions.Any() ? transactions.Average(x => x.Amount) : 0;
            decimal enYuksekIslemTutari = transactions.Any() ? transactions.Max(x => x.Amount) : 0;
            DateTime? sonIslemTarihi = transactions.Any() ? transactions.Max(x => x.Date) : (DateTime?)null;

            decimal tasarrufOrani = toplamGelir > 0
                ? Math.Round((netBakiye / toplamGelir) * 100, 1)
                : 0;

            string finansSkoru = "Dengeli";
            if (tasarrufOrani >= 40) finansSkoru = "Çok İyi";
            else if (tasarrufOrani >= 20) finansSkoru = "İyi";
            else if (tasarrufOrani >= 0) finansSkoru = "Orta";
            else finansSkoru = "Riskli";

            var sonLoglar = db.Logs
                .OrderByDescending(x => x.LogDate)
                .Take(5)
                .ToList();

            var bugun = DateTime.Today;
            var yarin = bugun.AddDays(1);

            int toplamLog = db.Logs.Count();
            int bugunkuLog = db.Logs.Count(x => x.LogDate >= bugun && x.LogDate < yarin);
            int createLogCount = db.Logs.Count(x => x.Action == "Create");
            int updateLogCount = db.Logs.Count(x => x.Action == "Update");
            int deleteLogCount = db.Logs.Count(x => x.Action == "Delete");

            ViewBag.ToplamGelir = toplamGelir;
            ViewBag.ToplamGider = toplamGider;
            ViewBag.NetBakiye = netBakiye;
            ViewBag.IslemSayisi = transactions.Count;
            ViewBag.SonIslemler = transactions.Take(3).ToList();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Category = category;

            ViewBag.AylikLabels = ayEtiketleri;
            ViewBag.AylikGelirler = aylikVeriler.Select(x => x.Income).ToList();
            ViewBag.AylikGiderler = aylikVeriler.Select(x => x.Expense).ToList();

            ViewBag.GiderKategoriLabels = giderKategorileri.Select(x => x.Category).ToList();
            ViewBag.GiderKategoriTotals = giderKategorileri.Select(x => x.Total).ToList();
            ViewBag.TopCategories = topCategories;

            ViewBag.ThisMonthIncome = thisMonthIncome;
            ViewBag.ThisMonthExpense = thisMonthExpense;
            ViewBag.IncomeChangePercentage = incomeChangePercentage;
            ViewBag.ExpenseChangePercentage = expenseChangePercentage;

            ViewBag.ToplamKategoriSayisi = toplamKategoriSayisi;
            ViewBag.OrtalamaIslemTutari = ortalamaIslemTutari;
            ViewBag.EnYuksekIslemTutari = enYuksekIslemTutari;
            ViewBag.SonIslemTarihi = sonIslemTarihi;
            ViewBag.TasarrufOrani = tasarrufOrani;
            ViewBag.FinansSkoru = finansSkoru;

            ViewBag.SonLoglar = sonLoglar;
            ViewBag.ToplamLog = toplamLog;
            ViewBag.BugunkuLog = bugunkuLog;
            ViewBag.CreateLogCount = createLogCount;
            ViewBag.UpdateLogCount = updateLogCount;
            ViewBag.DeleteLogCount = deleteLogCount;

            return View(transactions);
        }

        // =========================================================
        // DETAILS
        // =========================================================
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Transactions transaction = db.Transactions.Find(id);

            if (transaction == null)
                return HttpNotFound();

            return View(transaction);
        }

        // =========================================================
        // CREATE
        // =========================================================
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Transactions transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.CreatedAt = DateTime.Now;
                transaction.UpdatedAt = DateTime.Now;

                db.Transactions.Add(transaction);
                db.SaveChanges();

                AddLog(
                    "Create",
                    "Transaction created. ID: " + transaction.Id +
                    " | Type: " + transaction.Type +
                    " | Amount: " + transaction.Amount.ToString("N2")
                );

                return RedirectToAction("Index");
            }

            return View(transaction);
        }

        // =========================================================
        // EDIT
        // =========================================================
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Transactions transaction = db.Transactions.Find(id);

            if (transaction == null)
                return HttpNotFound();

            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Transactions transaction)
        {
            if (ModelState.IsValid)
            {
                var existingTransaction = db.Transactions.Find(transaction.Id);

                if (existingTransaction == null)
                    return HttpNotFound();

                existingTransaction.Date = transaction.Date;
                existingTransaction.Category = transaction.Category;
                existingTransaction.Type = transaction.Type;
                existingTransaction.Amount = transaction.Amount;
                existingTransaction.Description = transaction.Description;
                existingTransaction.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                db.Logs.Add(new Logs
                {
                    Action = "Update",
                    Description = "Transaction updated. ID: " + existingTransaction.Id +
                                  " | Type: " + existingTransaction.Type +
                                  " | Amount: " + existingTransaction.Amount.ToString("N2"),
                    LogDate = DateTime.Now
                });

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(transaction);
        }

        // =========================================================
        // DELETE
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            Transactions transaction = db.Transactions.Find(id);

            if (transaction != null)
            {
                string description =
                    "Transaction deleted. ID: " + transaction.Id +
                    " | Type: " + transaction.Type +
                    " | Amount: " + transaction.Amount.ToString("N2");

                db.Transactions.Remove(transaction);
                db.SaveChanges();

                AddLog("Delete", description);
            }

            return RedirectToAction("Index");
        }

        // =========================================================
        // EXPORT TO EXCEL
        // =========================================================
        public ActionResult ExportToExcel(DateTime? startDate, DateTime? endDate, string category)
        {
            var transactions = GetFilteredTransactions(startDate, endDate, category);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Transactions");

                worksheet.Cell(1, 1).Value = "Tarih";
                worksheet.Cell(1, 2).Value = "Kategori";
                worksheet.Cell(1, 3).Value = "Tip";
                worksheet.Cell(1, 4).Value = "Tutar";
                worksheet.Cell(1, 5).Value = "Açıklama";
                worksheet.Cell(1, 6).Value = "Oluşturulma Tarihi";
                worksheet.Cell(1, 7).Value = "Güncellenme Tarihi";

                var titleRange = worksheet.Range(1, 1, 1, 7);
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontColor = XLColor.White;
                titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int row = 2;

                foreach (var item in transactions)
                {
                    worksheet.Cell(row, 1).Value = item.Date;
                    worksheet.Cell(row, 1).Style.DateFormat.Format = "dd.MM.yyyy";

                    worksheet.Cell(row, 2).Value = string.IsNullOrWhiteSpace(item.Category) ? "Diğer" : item.Category;
                    worksheet.Cell(row, 3).Value = item.Type;

                    worksheet.Cell(row, 4).Value = item.Amount;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₺";

                    worksheet.Cell(row, 5).Value = string.IsNullOrWhiteSpace(item.Description) ? "-" : item.Description;

                    worksheet.Cell(row, 6).Value = item.CreatedAt;
                    worksheet.Cell(row, 6).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";

                    if (item.UpdatedAt.HasValue)
                    {
                        worksheet.Cell(row, 7).Value = item.UpdatedAt.Value;
                        worksheet.Cell(row, 7).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
                    }
                    else
                    {
                        worksheet.Cell(row, 7).Value = "-";
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();
                worksheet.Column(4).Width = 18;
                worksheet.Column(5).Width = 28;
                worksheet.Column(6).Width = 22;
                worksheet.Column(7).Width = 22;

                worksheet.SheetView.FreezeRows(1);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "MiniFinansRaporu.xlsx"
                    );
                }
            }
        }

        // =========================================================
        // EXPORT TO PDF
        // =========================================================
        public ActionResult ExportToPdf(DateTime? startDate, DateTime? endDate, string category)
        {
            var transactions = GetFilteredTransactions(startDate, endDate, category);

            decimal toplamGelir = transactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            decimal toplamGider = transactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            decimal netBakiye = toplamGelir - toplamGider;

            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4.Rotate());
                document.SetMargins(25, 25, 25, 25);

                string fontPath = @"C:\Windows\Fonts\arial.ttf";
                PdfFont font = PdfFontFactory.CreateFont(fontPath, "Identity-H");

                document.SetFont(font);

                var title = new Paragraph("Mini Finans Raporu")
                    .SetFont(font)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(5);

                document.Add(title);

                document.Add(
                    new Paragraph("Oluşturulma Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"))
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetMarginBottom(15)
                );

                Table summaryTable = new Table(3).UseAllAvailableWidth();
                summaryTable.SetMarginBottom(20);

                summaryTable.AddCell(CreateSummaryCell("Toplam Gelir", toplamGelir.ToString("N2", new CultureInfo("tr-TR")) + " ₺", font, new DeviceRgb(25, 135, 84)));
                summaryTable.AddCell(CreateSummaryCell("Toplam Gider", toplamGider.ToString("N2", new CultureInfo("tr-TR")) + " ₺", font, new DeviceRgb(220, 53, 69)));
                summaryTable.AddCell(CreateSummaryCell("Net Bakiye", netBakiye.ToString("N2", new CultureInfo("tr-TR")) + " ₺", font, new DeviceRgb(13, 110, 253)));

                document.Add(summaryTable);

                float[] columnWidths = { 80f, 100f, 70f, 90f, 210f, 110f, 110f };
                Table table = new Table(columnWidths).UseAllAvailableWidth();

                table.AddHeaderCell(CreateHeaderCell("Tarih", font));
                table.AddHeaderCell(CreateHeaderCell("Kategori", font));
                table.AddHeaderCell(CreateHeaderCell("Tip", font));
                table.AddHeaderCell(CreateHeaderCell("Tutar", font));
                table.AddHeaderCell(CreateHeaderCell("Açıklama", font));
                table.AddHeaderCell(CreateHeaderCell("Oluşturulma", font));
                table.AddHeaderCell(CreateHeaderCell("Güncellenme", font));

                foreach (var item in transactions)
                {
                    table.AddCell(CreateBodyCell(item.Date.ToString("dd.MM.yyyy"), font));
                    table.AddCell(CreateBodyCell(string.IsNullOrWhiteSpace(item.Category) ? "Diğer" : item.Category, font));
                    table.AddCell(CreateBodyCell(item.Type, font));
                    table.AddCell(CreateBodyCell(item.Amount.ToString("N2", new CultureInfo("tr-TR")) + " ₺", font));
                    table.AddCell(CreateBodyCell(string.IsNullOrWhiteSpace(item.Description) ? "-" : item.Description, font));
                    table.AddCell(CreateBodyCell(item.CreatedAt.ToString("dd.MM.yyyy HH:mm"), font));
                    table.AddCell(CreateBodyCell(item.UpdatedAt.HasValue ? item.UpdatedAt.Value.ToString("dd.MM.yyyy HH:mm") : "-", font));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "MiniFinansRaporu.pdf");
            }
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private List<Transactions> GetFilteredTransactions(DateTime? startDate, DateTime? endDate, string category)
        {
            var query = db.Transactions.AsQueryable();

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.Date >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.Date < end);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category);
            }

            return query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        private void AddLog(string action, string description)
        {
            Logs log = new Logs
            {
                Action = action,
                Description = description,
                LogDate = DateTime.Now
            };

            db.Logs.Add(log);
            db.SaveChanges();
        }

        private decimal CalculateChangePercentage(decimal oldValue, decimal newValue)
        {
            if (oldValue == 0 && newValue == 0)
                return 0;

            if (oldValue == 0)
                return 100;

            return Math.Round(((newValue - oldValue) / oldValue) * 100, 1);
        }

        private Cell CreateHeaderCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(10).SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(37, 99, 235))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8)
                .SetBorder(Border.NO_BORDER);
        }

        private Cell CreateBodyCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(9))
                .SetPadding(6)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        private Cell CreateSummaryCell(string title, string value, PdfFont font, DeviceRgb color)
        {
            return new Cell()
                .Add(new Paragraph(title)
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetMarginBottom(4))
                .Add(new Paragraph(value)
                    .SetFont(font)
                    .SetFontSize(13)
                    .SetFontColor(color))
                .SetPadding(12)
                .SetBorder(new SolidBorder(new DeviceRgb(220, 226, 232), 1));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    public class CategorySummaryViewModel
    {
        public string Category { get; set; }
        public decimal Total { get; set; }
    }

    public class MonthlyFinanceSummaryViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }
}