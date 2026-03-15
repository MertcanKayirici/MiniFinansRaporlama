using MiniFinansRaporlama.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MiniFinansRaporlama.Controllers
{
    public class FinanceController : Controller
    {
        // Veritabanı bağlantı nesnesi
        private MiniFinansDBEntities db = new MiniFinansDBEntities();

        // İşlem listesi, filtreleme, özet veriler ve grafik verileri
        public ActionResult Index(DateTime? startDate, DateTime? endDate, string category)
        {
            // Transactions tablosu üzerinde sorgu başlatılır
            var query = db.Transactions.AsQueryable();

            // Başlangıç tarihi seçildiyse o tarihten sonraki kayıtlar filtrelenir
            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.Date >= start);
            }

            // Bitiş tarihi seçildiyse o tarihe kadar olan kayıtlar filtrelenir
            // AddDays(1) kullanılarak gün sonu dahil edilir
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.Date < end);
            }

            // Kategori seçildiyse sadece o kategoriye ait kayıtlar getirilir
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(x => x.Category == category);
            }

            // Kayıtlar oluşturulma tarihine göre tersten sıralanır
            var transactions = query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            // Toplam gelir hesaplanır
            var toplamGelir = transactions
                .Where(x => x.Type == "Gelir")
                .Sum(x => x.Amount);

            // Toplam gider hesaplanır
            var toplamGider = transactions
                .Where(x => x.Type == "Gider")
                .Sum(x => x.Amount);

            // Giderler kategori bazında gruplanır
            // Kategori boşsa "Diğer" olarak gösterilir
            var giderKategorileri = transactions
                .Where(x => x.Type == "Gider")
                .GroupBy(x => string.IsNullOrEmpty(x.Category) ? "Diğer" : x.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .ToList();

            // Aylık gelir ve gider verileri hazırlanır
            var aylikVeriler = transactions
                .GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(g => new
                {
                    Yil = g.Key.Year,
                    Ay = g.Key.Month,
                    Gelir = g.Where(x => x.Type == "Gelir").Sum(x => x.Amount),
                    Gider = g.Where(x => x.Type == "Gider").Sum(x => x.Amount)
                })
                .OrderBy(x => x.Yil)
                .ThenBy(x => x.Ay)
                .ToList();

            // Grafik için ay etiketleri oluşturulur
            var ayEtiketleri = aylikVeriler
                .Select(x => new DateTime(x.Yil, x.Ay, 1).ToString("MMM yyyy", new System.Globalization.CultureInfo("tr-TR")))
                .ToList();

            // Aylık grafik verileri ViewBag ile View'a gönderilir
            ViewBag.AylikLabels = ayEtiketleri;
            ViewBag.AylikGelirler = aylikVeriler.Select(x => x.Gelir).ToList();
            ViewBag.AylikGiderler = aylikVeriler.Select(x => x.Gider).ToList();

            // Dashboard kartlarında kullanılacak özet veriler
            ViewBag.ToplamGelir = toplamGelir;
            ViewBag.ToplamGider = toplamGider;
            ViewBag.NetBakiye = toplamGelir - toplamGider;
            ViewBag.IslemSayisi = transactions.Count();
            ViewBag.SonIslemler = transactions.Take(5).ToList();

            // Filtre alanlarında seçili değerleri korumak için ViewBag'e atanır
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Category = category;

            // Kategori bazlı gider grafiği için label ve toplam değerler
            ViewBag.GiderKategoriLabels = giderKategorileri.Select(x => x.Category).ToList();
            ViewBag.GiderKategoriTotals = giderKategorileri.Select(x => x.Total).ToList();

            // Liste ekranına veriler gönderilir
            return View(transactions);
        }

        // Tek bir işlemin detay sayfası
        public ActionResult Details(int? id)
        {
            // Id boş gelirse 400 BadRequest döndürülür
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // İlgili kayıt veritabanından bulunur
            Transactions transaction = db.Transactions.Find(id);

            // Kayıt bulunamazsa 404 döndürülür
            if (transaction == null)
                return HttpNotFound();

            // Detay view'ına kayıt gönderilir
            return View(transaction);
        }

        // Yeni işlem ekleme sayfası
        public ActionResult Create()
        {
            return View();
        }

        // Yeni işlem ekleme POST işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Transactions transaction)
        {
            // Model doğrulama başarılıysa kayıt eklenir
            if (ModelState.IsValid)
            {
                // Oluşturulma ve güncellenme tarihleri atanır
                transaction.CreatedAt = System.DateTime.Now;
                transaction.UpdatedAt = System.DateTime.Now;

                // Yeni işlem veritabanına eklenir
                db.Transactions.Add(transaction);
                db.SaveChanges();

                // Log kaydı oluşturulur
                Logs log = new Logs();
                log.Action = "Create";
                log.Description = "Yeni işlem eklendi. ID: " + transaction.Id +
                                  " | Tür: " + transaction.Type +
                                  " | Tutar: " + transaction.Amount;
                log.LogDate = System.DateTime.Now;

                // Log veritabanına eklenir
                db.Logs.Add(log);
                db.SaveChanges();

                // Başarılı işlem sonrası liste sayfasına dönülür
                return RedirectToAction("Index");
            }

            // Model geçersizse form tekrar gösterilir
            return View(transaction);
        }

        // İşlem düzenleme sayfası
        public ActionResult Edit(int? id)
        {
            // Id boşsa 400 döndürülür
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // İlgili işlem bulunur
            Transactions transaction = db.Transactions.Find(id);

            // Kayıt yoksa 404 döndürülür
            if (transaction == null)
                return HttpNotFound();

            // Düzenleme sayfasına veri gönderilir
            return View(transaction);
        }

        // İşlem düzenleme POST işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Transactions transaction)
        {
            // Model doğrulama başarılıysa güncelleme yapılır
            if (ModelState.IsValid)
            {
                // Son güncellenme tarihi atanır
                transaction.UpdatedAt = System.DateTime.Now;

                // Entity Framework'e bu kaydın güncellendiği bildirilir
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                // Log kaydı oluşturulur
                Logs log = new Logs();
                log.Action = "Update";
                log.Description = "İşlem güncellendi. ID: " + transaction.Id +
                                  " | Tür: " + transaction.Type +
                                  " | Tutar: " + transaction.Amount;
                log.LogDate = System.DateTime.Now;

                // Log veritabanına eklenir
                db.Logs.Add(log);
                db.SaveChanges();

                // Başarılı güncelleme sonrası listeye dönülür
                return RedirectToAction("Index");
            }

            // Model geçersizse form tekrar gösterilir
            return View(transaction);
        }

        // Silme onay sayfası
        public ActionResult Delete(int? id)
        {
            // Id boşsa 400 döndürülür
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Silinecek kayıt bulunur
            Transactions transaction = db.Transactions.Find(id);

            // Kayıt bulunamazsa 404 döndürülür
            if (transaction == null)
                return HttpNotFound();

            // Silme onay ekranına veri gönderilir
            return View(transaction);
        }

        // Silme işlemi POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // İlgili işlem bulunur
            Transactions transaction = db.Transactions.Find(id);

            if (transaction != null)
            {
                // Silme işleminden önce log açıklaması hazırlanır
                string aciklama = "İşlem silindi. ID: " + transaction.Id +
                                  " | Tür: " + transaction.Type +
                                  " | Tutar: " + transaction.Amount;

                // Kayıt veritabanından silinir
                db.Transactions.Remove(transaction);
                db.SaveChanges();

                // Silme log'u oluşturulur
                Logs log = new Logs();
                log.Action = "Delete";
                log.Description = aciklama;
                log.LogDate = System.DateTime.Now;

                // Log veritabanına eklenir
                db.Logs.Add(log);
                db.SaveChanges();
            }

            // İşlem sonrası liste sayfasına dönülür
            return RedirectToAction("Index");
        }

        // Controller dispose edilirken veritabanı bağlantısı kapatılır
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