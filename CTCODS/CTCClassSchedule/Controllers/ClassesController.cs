using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTCClassSchedule.Controllers
{
    public class ClassesController : Controller
    {
        //
        // GET: /Classes/

			public ActionResult Index(string letter)
			{
				ViewBag.letter = letter;
				return View();
			}

			public ActionResult All(string Subject)
			{
				return View();
			}


			public ActionResult Subject(string Subject)
			{
				ViewBag.Subject = Subject;
				return View();
			}

			public ActionResult YearQuarter(string YearQuarter, string flex, string time, string days, string avail, string letter)
			{
				ViewBag.letter = letter;
				ViewBag.YearQuarter = YearQuarter;
				ViewBag.flex = flex;
				return View();
			}

			public ActionResult YearQuarterSubject(String YearQuarter, string Subject, string flex, string time, string days, string avail)
			{
				ViewBag.YearQuarter = YearQuarter;
				ViewBag.Subject = Subject;
				return View();
			}


			public ActionResult ClassDetails()
			{
				return View();
			}



        //
        // GET: /Classes/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Classes/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Classes/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Classes/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Classes/Edit/5

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

        //
        // GET: /Classes/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Classes/Delete/5

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
    }
}
