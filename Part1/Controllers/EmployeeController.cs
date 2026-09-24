using Microsoft.AspNetCore.Mvc;
using Part1.Models;
using System.Linq;

namespace Part1.Controllers
{
    public class EmployeeController : Controller
    {
        public static List<ProjectUpdate> allUpdates = new List<ProjectUpdate>();

        public IActionResult Dashboard()
        {
            ViewBag.Updates = allUpdates.OrderByDescending(u => u.DatePosted).ToList();
            ViewBag.TotalMoneyDonations = DonorController.allDonations.Count;
            ViewBag.TotalClothesDonations = DonorController.allClothes.Count;
            ViewBag.TotalFoodDonations = DonorController.allFood.Count;
            ViewBag.TotalVolunteers = VolunteerController.allVolunteers.Count;

            // For latest donations
            ViewBag.LatestDonations = DonorController.allDonations
                .OrderByDescending(d => d.DonationDate)
                .Take(5)
                .ToList();

            ViewBag.TotalDonationsCount = DonorController.allDonations.Count
                + DonorController.allClothes.Count
                + DonorController.allFood.Count;

            return View();
        }

        [HttpPost]
        public IActionResult PostUpdate(ProjectUpdate update)
        {
            if (ModelState.IsValid)
            {
                update.Id = allUpdates.Count + 1;
                update.DatePosted = DateTime.Now;
                update.PostedBy = "Employee";
                allUpdates.Add(update);
                TempData["Success"] = "Update posted successfully!";
                return RedirectToAction("Dashboard");
            }
            return View("Dashboard");
        }

        public IActionResult ViewVolunteers()
        {
            return RedirectToAction("ViewVolunteers", "Volunteer");
        }
    }
}