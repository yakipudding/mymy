using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Mymy.DAL;
using Mymy.Models;

namespace Mymy.Controllers
{
    public class MemosController : Controller
    {
        private MymyContext db = new MymyContext();

        // GET: Memos
        public ActionResult Index()
        {
            return View(db.Memos.ToList());
        }

        // GET: Memos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Memo memo = db.Memos.Find(id);
            if (memo == null)
            {
                return HttpNotFound();
            }
            return View(memo);
        }

        // GET: Memos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Memos/Create
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MemoId,ProjectId,Category,Title,Content")] Memo memo)
        {
            if (ModelState.IsValid)
            {
                db.Memos.Add(memo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(memo);
        }

        // GET: Memos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Memo memo = db.Memos.Find(id);
            if (memo == null)
            {
                return HttpNotFound();
            }
            return View(memo);
        }

        // POST: Memos/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MemoId,ProjectId,Category,Title,Content")] Memo memo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(memo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(memo);
        }

        // GET: Memos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Memo memo = db.Memos.Find(id);
            if (memo == null)
            {
                return HttpNotFound();
            }
            return View(memo);
        }

        // POST: Memos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Memo memo = db.Memos.Find(id);
            db.Memos.Remove(memo);
            db.SaveChanges();
            return RedirectToAction("Index");
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
}
