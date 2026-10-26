using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using pr_7.Models;

namespace pr_7.Controllers
{
    public class FeedbackController : Controller
    {
        // Static in-memory list to store feedback data for the practical
        private static List<Feedback> feedbackList = new List<Feedback>
        {
            new Feedback
            {
                Id = 1,
                Name = "Aarav Patel",
                Email = "aarav@gmail.com",
                ContactNo = "9876543210",
                Category = "Teaching",
                Rating = 5,
                Comments = "Teaching methodology for .NET MVC is clear and practical.",
                ContactMethod = "Email",
                WouldRecommend = true,
                SubmittedDate = DateTime.Now.AddDays(-2)
            },
            new Feedback
            {
                Id = 2,
                Name = "Priya Sharma",
                Email = "priya@gmail.com",
                ContactNo = "9823456781",
                Category = "Curriculum",
                Rating = 4,
                Comments = "Practical exercises help understand MVC architecture properly.",
                ContactMethod = "Email",
                WouldRecommend = true,
                SubmittedDate = DateTime.Now.AddDays(-1)
            }
        };

        private static int nextId = 3;

        // Helper method to populate DropDownList categories
        private SelectList GetCategoryList(string? selectedValue = null)
        {
            var categories = new List<string> { "Teaching", "Curriculum", "Lab Infrastructure", "General" };
            return new SelectList(categories, selectedValue);
        }

        // 1. GET: Feedback/Index (Displays list and summary partial view)
        public IActionResult Index()
        {
            return View(feedbackList);
        }

        // 2. GET: Feedback/Create (Displays form using HTML Helpers)
        public IActionResult Create()
        {
            ViewBag.Categories = GetCategoryList();
            return View(new Feedback());
        }

        // 3. POST: Feedback/Create (Validates and saves feedback)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Feedback feedback)
        {
            // Server-side validation check
            if (ModelState.IsValid)
            {
                feedback.Id = nextId++;
                feedback.SubmittedDate = DateTime.Now;
                feedbackList.Add(feedback);

                TempData["Message"] = "Feedback submitted successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If validation failed, repopulate dropdown and return form
            ViewBag.Categories = GetCategoryList(feedback.Category);
            return View(feedback);
        }

        // 4. GET: Feedback/Details/1
        public IActionResult Details(int id)
        {
            var item = feedbackList.FirstOrDefault(f => f.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // 5. GET or POST: Feedback/Delete/1
        public IActionResult Delete(int id)
        {
            var item = feedbackList.FirstOrDefault(f => f.Id == id);
            if (item != null)
            {
                feedbackList.Remove(item);
                TempData["Message"] = "Feedback deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
