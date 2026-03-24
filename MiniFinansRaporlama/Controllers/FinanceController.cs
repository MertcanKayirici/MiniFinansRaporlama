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
        // Database context used for transaction and log operations
        private MiniFinansDBEntities db = new MiniFinansDBEntities();

        // =========================================================
        // INDEX
        // =========================================================
        // Displays the finance dashboard and transaction list
        // Supports optional filtering by start date, end date, and category
        public ActionResult Index(DateTime? startDate, DateTime? endDate, string category)
        {
            // Base query for transactions
            var query = db.Transactions.AsQueryable();

            // Apply start date filter if provided
            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.Date >= start);
            }

            // Apply end date filter inclusively by moving to the next day
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.Date < end);
            }

            // Apply category filter if selected
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category);
            }

            // Fetch filtered transactions ordered by creation date descending
            var transactions = query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            // Calculate total income
            decimal toplamGelir = transactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            // Calculate total expense
            decimal toplamGider = transactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            // Calculate net balance
            decimal netBakiye = toplamGelir - toplamGider;

            // Build expense category summary
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

            // Keep only top 5 expense categories
            var topCategories = giderKategorileri
                .Take(5)
                .ToList();

            // Build monthly finance summary for chart visualization
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

            // Create month labels in Turkish culture format
            var ayEtiketleri = aylikVeriler
                .Select(x => new DateTime(x.Year, x.Month, 1)
                .ToString("MMM yyyy", new CultureInfo("tr-TR")))
                .ToList();

            // Define current, next, and previous month ranges
            DateTime today = DateTime.Today;
            DateTime monthStart = new DateTime(today.Year, today.Month, 1);
            DateTime nextMonthStart = monthStart.AddMonths(1);
            DateTime prevMonthStart = monthStart.AddMonths(-1);

            // Get current month transactions
            var thisMonthTransactions = db.Transactions
                .Where(x => x.Date >= monthStart && x.Date < nextMonthStart)
                .ToList();

            // Get previous month transactions
            var prevMonthTransactions = db.Transactions
                .Where(x => x.Date >= prevMonthStart && x.Date < monthStart)
                .ToList();

            // Calculate current month income
            decimal thisMonthIncome = thisMonthTransactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            // Calculate current month expense
            decimal thisMonthExpense = thisMonthTransactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            // Calculate previous month income
            decimal prevMonthIncome = prevMonthTransactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            // Calculate previous month expense
            decimal prevMonthExpense = prevMonthTransactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            // Calculate percentage changes between current and previous month
            decimal incomeChangePercentage = CalculateChangePercentage(prevMonthIncome, thisMonthIncome);
            decimal expenseChangePercentage = CalculateChangePercentage(prevMonthExpense, thisMonthExpense);

            // Count distinct non-empty categories
            int toplamKategoriSayisi = transactions
                .Where(x => !string.IsNullOrWhiteSpace(x.Category))
                .Select(x => x.Category)
                .Distinct()
                .Count();

            // Calculate additional dashboard metrics
            decimal ortalamaIslemTutari = transactions.Any() ? transactions.Average(x => x.Amount) : 0;
            decimal enYuksekIslemTutari = transactions.Any() ? transactions.Max(x => x.Amount) : 0;
            DateTime? sonIslemTarihi = transactions.Any() ? transactions.Max(x => x.Date) : (DateTime?)null;

            // Calculate savings ratio based on total income
            decimal tasarrufOrani = toplamGelir > 0
                ? Math.Round((netBakiye / toplamGelir) * 100, 1)
                : 0;

            // Determine finance score based on savings ratio
            string finansSkoru = "Dengeli";
            if (tasarrufOrani >= 40) finansSkoru = "Çok İyi";
            else if (tasarrufOrani >= 20) finansSkoru = "İyi";
            else if (tasarrufOrani >= 0) finansSkoru = "Orta";
            else finansSkoru = "Riskli";

            // Retrieve latest audit logs
            var sonLoglar = db.Logs
                .OrderByDescending(x => x.LogDate)
                .Take(5)
                .ToList();

            // Define today and tomorrow for daily log statistics
            var bugun = DateTime.Today;
            var yarin = bugun.AddDays(1);

            // Calculate log counters
            int toplamLog = db.Logs.Count();
            int bugunkuLog = db.Logs.Count(x => x.LogDate >= bugun && x.LogDate < yarin);
            int createLogCount = db.Logs.Count(x => x.Action == "Create");
            int updateLogCount = db.Logs.Count(x => x.Action == "Update");
            int deleteLogCount = db.Logs.Count(x => x.Action == "Delete");

            // Pass summary values to the view
            ViewBag.ToplamGelir = toplamGelir;
            ViewBag.ToplamGider = toplamGider;
            ViewBag.NetBakiye = netBakiye;
            ViewBag.IslemSayisi = transactions.Count;
            ViewBag.SonIslemler = transactions.Take(3).ToList();

            // Pass current filter values back to the view
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Category = category;

            // Pass monthly chart data to the view
            ViewBag.AylikLabels = ayEtiketleri;
            ViewBag.AylikGelirler = aylikVeriler.Select(x => x.Income).ToList();
            ViewBag.AylikGiderler = aylikVeriler.Select(x => x.Expense).ToList();

            // Pass category expense chart data to the view
            ViewBag.GiderKategoriLabels = giderKategorileri.Select(x => x.Category).ToList();
            ViewBag.GiderKategoriTotals = giderKategorileri.Select(x => x.Total).ToList();
            ViewBag.TopCategories = topCategories;

            // Pass current month metrics to the view
            ViewBag.ThisMonthIncome = thisMonthIncome;
            ViewBag.ThisMonthExpense = thisMonthExpense;
            ViewBag.IncomeChangePercentage = incomeChangePercentage;
            ViewBag.ExpenseChangePercentage = expenseChangePercentage;

            // Pass additional analytical metrics to the view
            ViewBag.ToplamKategoriSayisi = toplamKategoriSayisi;
            ViewBag.OrtalamaIslemTutari = ortalamaIslemTutari;
            ViewBag.EnYuksekIslemTutari = enYuksekIslemTutari;
            ViewBag.SonIslemTarihi = sonIslemTarihi;
            ViewBag.TasarrufOrani = tasarrufOrani;
            ViewBag.FinansSkoru = finansSkoru;

            // Pass log metrics to the view
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
        // Displays the details of a single transaction
        public ActionResult Details(int? id)
        {
            // Return bad request if id is missing
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Find transaction by id
            Transactions transaction = db.Transactions.Find(id);

            // Return 404 if transaction does not exist
            if (transaction == null)
                return HttpNotFound();

            return View(transaction);
        }

        // =========================================================
        // CREATE
        // =========================================================
        // Displays the transaction creation form
        public ActionResult Create()
        {
            return View();
        }

        // Handles transaction creation form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Transactions transaction)
        {
            // Continue only if submitted model is valid
            if (ModelState.IsValid)
            {
                // Set audit timestamps
                transaction.CreatedAt = DateTime.Now;
                transaction.UpdatedAt = DateTime.Now;

                // Add transaction to database
                db.Transactions.Add(transaction);
                db.SaveChanges();

                // Add creation log entry
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
        // Displays the edit form for an existing transaction
        public ActionResult Edit(int? id)
        {
            // Return bad request if id is missing
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Find transaction by id
            Transactions transaction = db.Transactions.Find(id);

            // Return 404 if transaction is not found
            if (transaction == null)
                return HttpNotFound();

            return View(transaction);
        }

        // Handles transaction update form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Transactions transaction)
        {
            // Continue only if submitted model is valid
            if (ModelState.IsValid)
            {
                // Retrieve the original transaction from database
                var existingTransaction = db.Transactions.Find(transaction.Id);

                // Return 404 if the original transaction does not exist
                if (existingTransaction == null)
                    return HttpNotFound();

                // Update editable fields
                existingTransaction.Date = transaction.Date;
                existingTransaction.Category = transaction.Category;
                existingTransaction.Type = transaction.Type;
                existingTransaction.Amount = transaction.Amount;
                existingTransaction.Description = transaction.Description;
                existingTransaction.UpdatedAt = DateTime.Now;

                // Save updated transaction
                db.SaveChanges();

                // Add update log entry
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
        // Deletes a transaction and records the action in logs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            // Find transaction by id
            Transactions transaction = db.Transactions.Find(id);

            if (transaction != null)
            {
                // Prepare delete log description before removing the record
                string description =
                    "Transaction deleted. ID: " + transaction.Id +
                    " | Type: " + transaction.Type +
                    " | Amount: " + transaction.Amount.ToString("N2");

                // Remove transaction from database
                db.Transactions.Remove(transaction);
                db.SaveChanges();

                // Add delete log entry
                AddLog("Delete", description);
            }

            return RedirectToAction("Index");
        }

        // =========================================================
        // EXPORT TO EXCEL
        // =========================================================
        // Exports filtered transactions to an Excel file
        public ActionResult ExportToExcel(DateTime? startDate, DateTime? endDate, string category)
        {
            // Retrieve filtered transaction list
            var transactions = GetFilteredTransactions(startDate, endDate, category);

            using (var workbook = new XLWorkbook())
            {
                // Create worksheet
                var worksheet = workbook.Worksheets.Add("Transactions");

                // Set header titles
                worksheet.Cell(1, 1).Value = "Tarih";
                worksheet.Cell(1, 2).Value = "Kategori";
                worksheet.Cell(1, 3).Value = "Tip";
                worksheet.Cell(1, 4).Value = "Tutar";
                worksheet.Cell(1, 5).Value = "Açıklama";
                worksheet.Cell(1, 6).Value = "Oluşturulma Tarihi";
                worksheet.Cell(1, 7).Value = "Güncellenme Tarihi";

                // Apply header styling
                var titleRange = worksheet.Range(1, 1, 1, 7);
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontColor = XLColor.White;
                titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int row = 2;

                // Write transaction rows into worksheet
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

                // Auto-fit and adjust important columns
                worksheet.Columns().AdjustToContents();
                worksheet.Column(4).Width = 18;
                worksheet.Column(5).Width = 28;
                worksheet.Column(6).Width = 22;
                worksheet.Column(7).Width = 22;

                // Freeze header row
                worksheet.SheetView.FreezeRows(1);

                using (var stream = new MemoryStream())
                {
                    // Save workbook to memory and return as downloadable file
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
        // Exports filtered transactions to a styled PDF report
        public ActionResult ExportToPdf(DateTime? startDate, DateTime? endDate, string category)
        {
            // Retrieve filtered transaction list
            var transactions = GetFilteredTransactions(startDate, endDate, category);

            // Calculate summary values for the PDF report
            decimal toplamGelir = transactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            decimal toplamGider = transactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            decimal netBakiye = toplamGelir - toplamGider;

            using (var stream = new MemoryStream())
            {
                // Create PDF writer and document
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4.Rotate());
                document.SetMargins(25, 25, 25, 25);

                // Load Arial font from Windows fonts folder for Turkish character support
                string fontPath = @"C:\Windows\Fonts\arial.ttf";
                PdfFont font = PdfFontFactory.CreateFont(fontPath, "Identity-H");

                document.SetFont(font);

                // Add report title
                var title = new Paragraph("Mini Finans Raporu")
                    .SetFont(font)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(5);

                document.Add(title);

                // Add report generation timestamp
                document.Add(
                    new Paragraph("Oluşturulma Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"))
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetMarginBottom(15)
                );

                // Create summary cards table
                Table summaryTable = new Table(3).UseAllAvailableWidth();
                summaryTable.SetMarginBottom(20);

                summaryTable.AddCell(CreateSummaryCell("Toplam Gelir", toplamGelir.ToString("N2", new CultureInfo("tr-TR")) + " ₺", font, new DeviceRgb(25, 135, 84)));
                summaryTable.AddCell(CreateSummaryCell("Toplam Gider", toplamGider.ToString("N2", new CultureInfo("tr-TR")) + " ₺", font, new DeviceRgb(220, 53, 69)));
                summaryTable.AddCell(CreateSummaryCell("Net Bakiye", netBakiye.ToString("N2", new CultureInfo("tr-TR")) + " ₺", font, new DeviceRgb(13, 110, 253)));

                document.Add(summaryTable);

                // Define main report table column widths
                float[] columnWidths = { 80f, 100f, 70f, 90f, 210f, 110f, 110f };
                Table table = new Table(columnWidths).UseAllAvailableWidth();

                // Add table headers
                table.AddHeaderCell(CreateHeaderCell("Tarih", font));
                table.AddHeaderCell(CreateHeaderCell("Kategori", font));
                table.AddHeaderCell(CreateHeaderCell("Tip", font));
                table.AddHeaderCell(CreateHeaderCell("Tutar", font));
                table.AddHeaderCell(CreateHeaderCell("Açıklama", font));
                table.AddHeaderCell(CreateHeaderCell("Oluşturulma", font));
                table.AddHeaderCell(CreateHeaderCell("Güncellenme", font));

                // Add transaction rows to the PDF table
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

                // Finalize and return PDF file
                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "MiniFinansRaporu.pdf");
            }
        }

        // =========================================================
        // HELPERS
        // =========================================================
        // Returns transactions filtered by date range and category
        private List<Transactions> GetFilteredTransactions(DateTime? startDate, DateTime? endDate, string category)
        {
            // Base query for transaction export operations
            var query = db.Transactions.AsQueryable();

            // Apply start date filter if provided
            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.Date >= start);
            }

            // Apply end date filter inclusively
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.Date < end);
            }

            // Apply category filter if provided
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category);
            }

            // Return filtered data ordered by creation date descending
            return query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        // Adds an audit log entry to the database
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

        // Calculates percentage change between two values
        private decimal CalculateChangePercentage(decimal oldValue, decimal newValue)
        {
            // Return zero if both values are zero
            if (oldValue == 0 && newValue == 0)
                return 0;

            // Return 100 when previous value is zero and new value exists
            if (oldValue == 0)
                return 100;

            // Standard percentage change formula
            return Math.Round(((newValue - oldValue) / oldValue) * 100, 1);
        }

        // Creates a styled header cell for the PDF table
        private Cell CreateHeaderCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(10).SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(37, 99, 235))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8)
                .SetBorder(Border.NO_BORDER);
        }

        // Creates a standard body cell for the PDF table
        private Cell CreateBodyCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(9))
                .SetPadding(6)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        // Creates a summary metric cell used in the PDF summary section
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

        // Releases database resources when controller is disposed
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    // View model used for category-based expense summaries
    public class CategorySummaryViewModel
    {
        public string Category { get; set; }
        public decimal Total { get; set; }
    }

    // View model used for monthly finance chart summaries
    public class MonthlyFinanceSummaryViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }
}