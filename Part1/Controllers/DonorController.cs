using Microsoft.AspNetCore.Mvc;
using Part1.Models;

namespace Part1.Controllers
{
    public class DonorController : Controller
    {
        public static List<Donation> allDonations = new List<Donation>();
        public static List<Clothes> allClothes = new List<Clothes>();
        public static List<Food> allFood = new List<Food>();

        public IActionResult Donate() => View();

        [HttpPost]
        public IActionResult DonateMoney(Donation d)
        {
            d.Id = allDonations.Count + 1;
            d.DonationDate = DateTime.Now;
            allDonations.Add(d);
            return View("TaxCertificate", d);
        }

        [HttpPost]
        public IActionResult DonateClothes(Clothes c)
        {
            c.Id = allClothes.Count + 1;
            allClothes.Add(c);
            return View("ClothesConfirmation", c);
        }

        [HttpPost]
        public IActionResult DonateFood(Food f)
        {
            f.Id = allFood.Count + 1;
            allFood.Add(f);
            return View("FoodConfirmation", f);
        }

        public IActionResult TaxCertificate(Donation d) => View(d);
        public IActionResult ClothesConfirmation() => View();
        public IActionResult FoodConfirmation() => View();
    }
}