using MiniFinansRaporlama.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MiniFinansRaporlama.Controllers
{
    /// <summary>
    /// FinanceController
    /// Handles financial transaction operations such as listing, filtering,
    /// creating, updating, deleting, and generating dashboard data.
    /// </summary>
    public class FinanceController : Controller
    {
        // Entity Framework database context
        private MiniFinansDBEntities db = new MiniFinansDBEntities();

        // =========================================================
        // INDEX - Dashboard, Listing and Filtering
        // =========================================================
        /// <summary>
        /// Lists transactions, applies filters and prepares dashboard data.
        /// </summary>
        public ActionResult Index(DateTime? startDate, DateTime? endDate, string category)
        {
            // Initialize query on Transactions table
            var query = db.Transactions.AsQueryable();

            // Apply start date filter if provided
            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.Date >= start);
            }

            // Apply end date filter (inclusive of the day)
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.Date < end);
            }

            // Apply category filter if selected
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(x => x.Category == category);
            }

            // Order transactions by creation date descending
            var transactions = query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            // Calculate total income
            var toplamGelir = transactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            // Calculate total expense
            var toplamGider = transactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            // Group expenses by category
            var giderKategorileri = transactions
                .Where(x => x.Type == "Gider")
                .GroupBy(x => string.IsNullOrEmpty(x.Category) ? "Other" : x.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .ToList();

            // Prepare monthly income and expense data
            var aylikVeriler = transactions
                .GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Income = g.Where(x => x.Type == "Gelir").Sum(x => x.Amount),
                    Expense = g.Where(x => x.Type == "Gider").Sum(x => x.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            // Create labels for chart (month names)
            var ayEtiketleri = aylikVeriler
                .Select(x => new DateTime(x.Year, x.Month, 1)
                .ToString("MMM yyyy", new System.Globalization.CultureInfo("en-US")))
                .ToList();

            // Send chart data to View
            ViewBag.AylikLabels = ayEtiketleri;
            ViewBag.AylikGelirler = aylikVeriler.Select(x => x.Income).ToList();
            ViewBag.AylikGiderler = aylikVeriler.Select(x => x.Expense).ToList();

            // Dashboard summary data
            ViewBag.ToplamGelir = toplamGelir;
            ViewBag.ToplamGider = toplamGider;
            ViewBag.NetBakiye = toplamGelir - toplamGider;
            ViewBag.IslemSayisi = transactions.Count();
            ViewBag.SonIslemler = transactions.Take(5).ToList();

            // Preserve filter values
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Category = category;

            // Expense category chart data
            ViewBag.GiderKategoriLabels = giderKategorileri.Select(x => x.Category).ToList();
            ViewBag.GiderKategoriTotals = giderKategorileri.Select(x => x.Total).ToList();

            // Return data to view
            return View(transactions);
        }

        // =========================================================
        // DETAILS
        // =========================================================
        /// <summary>
        /// Displays details of a single transaction.
        /// </summary>
        public ActionResult Details(int? id)
        {
            // Return 400 if id is null
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Find transaction
            Transactions transaction = db.Transactions.Find(id);

            // Return 404 if not found
            if (transaction == null)
                return HttpNotFound();

            return View(transaction);
        }

        // =========================================================
        // CREATE
        // =========================================================
        /// <summary>
        /// Returns create transaction view.
        /// </summary>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Handles creation of a new transaction.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Transactions transaction)
        {
            if (ModelState.IsValid)
            {
                // Set timestamps
                transaction.CreatedAt = DateTime.Now;
                transaction.UpdatedAt = DateTime.Now;

                // Save transaction
                db.Transactions.Add(transaction);
                db.SaveChanges();

                // Create log
                Logs log = new Logs
                {
                    Action = "Create",
                    Description = "Transaction created. ID: " + transaction.Id +
                                  " | Type: " + transaction.Type +
                                  " | Amount: " + transaction.Amount,
                    LogDate = DateTime.Now
                };

                db.Logs.Add(log);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(transaction);
        }

        // =========================================================
        // EDIT
        // =========================================================
        /// <summary>
        /// Returns edit page for a transaction.
        /// </summary>
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Transactions transaction = db.Transactions.Find(id);

            if (transaction == null)
                return HttpNotFound();

            return View(transaction);
        }

        /// <summary>
        /// Handles updating an existing transaction.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Transactions transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.UpdatedAt = DateTime.Now;

                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                Logs log = new Logs
                {
                    Action = "Update",
                    Description = "Transaction updated. ID: " + transaction.Id +
                                  " | Type: " + transaction.Type +
                                  " | Amount: " + transaction.Amount,
                    LogDate = DateTime.Now
                };

                db.Logs.Add(log);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(transaction);
        }

        // =========================================================
        // DELETE
        // =========================================================
        /// <summary>
        /// Returns delete confirmation page.
        /// </summary>
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Transactions transaction = db.Transactions.Find(id);

            if (transaction == null)
                return HttpNotFound();

            return View(transaction);
        }

        /// <summary>
        /// Handles deletion of a transaction.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transactions transaction = db.Transactions.Find(id);

            if (transaction != null)
            {
                string description = "Transaction deleted. ID: " + transaction.Id +
                                     " | Type: " + transaction.Type +
                                     " | Amount: " + transaction.Amount;

                db.Transactions.Remove(transaction);
                db.SaveChanges();

                Logs log = new Logs
                {
                    Action = "Delete",
                    Description = description,
                    LogDate = DateTime.Now
                };

                db.Logs.Add(log);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // =========================================================
        // DISPOSE
        // =========================================================
        /// <summary>
        /// Disposes database context properly.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}