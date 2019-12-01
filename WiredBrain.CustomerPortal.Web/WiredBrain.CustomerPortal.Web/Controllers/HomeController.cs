using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using WiredBrain.CustomerPortal.Web.Models;
using WiredBrain.CustomerPortal.Web.Repositories;
using WiredBrain.Logging;

namespace WiredBrain.CustomerPortal.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICustomerRepository repo;
        private readonly IConfiguration config;

        public HomeController(ICustomerRepository repo, IConfiguration config)
        {
            this.repo = repo;
            this.config = config;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Welcome()
        {
            Log.Information("Landed on welcome page - should be authenticated.");
            ViewBag.Title = "Enter loyalty number";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Welcome(string loyaltyNumber)
        {
            if (!int.TryParse(loyaltyNumber, out var loyaltyNum))
            {
                SecurityLog.Warning("Invalid input from user!  {UserInput}", loyaltyNumber);
                //Log.Warning("Invalid input from user!  {UserInput}", loyaltyNumber);
                throw new Exception("Invalid input detected!!!");
            }
            var customer = await repo.GetCustomerByLoyaltyNumber(loyaltyNum);
            if (customer == null)
            {
                ModelState.AddModelError(string.Empty, "Unknown loyalty number");
                return View();
            }
            return RedirectToAction("LoyaltyOverview", new { loyaltyNumber });
        }

        public async Task<IActionResult> LoyaltyOverview(int loyaltyNumber)
        {
            ViewBag.Title = "Your points";
            var userLoyaltyNumber = int.Parse(User.Claims.FirstOrDefault(a => a.Type == "loyalty")?.Value ?? "0");
            if (userLoyaltyNumber != loyaltyNumber)
            {
                SecurityLog.Warning("Unauthorized access attempted on {LoyaltyNum}", loyaltyNumber);
                //Log.Warning("Unauthorized access attempted on {LoyaltyNum}", loyaltyNumber);
                throw new Exception($"Unauthorized to see loyalty number {loyaltyNumber}!!");
            }
            var customer = await repo.GetCustomerByLoyaltyNumber(loyaltyNumber);
            
            var pointsNeeded = int.Parse(config["CustomerPortalSettings:PointsNeeded"]);

            var loyaltyModel = LoyaltyModel.FromCustomer(customer, pointsNeeded);
            return View(loyaltyModel);
        }

        public async Task<IActionResult> EditFavorite(int loyaltyNumber)
        {
            ViewBag.Title = "Edit favorite";

            var customer = await repo.GetCustomerByLoyaltyNumber(loyaltyNumber);
            return View(new EditFavoriteModel
            {
                LoyaltyNumber = customer.LoyaltyNumber,
                Favorite = customer.FavoriteDrink
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditFavorite(EditFavoriteModel model)
        {
            await repo.SetFavorite(model);
            return RedirectToAction("LoyaltyOverview", new { loyaltyNumber = model.LoyaltyNumber });
        }

        [HttpGet]
        public async Task<IActionResult> BadApiCall()
        {
            using (var httpClient = new HttpClient(new StandardHttpMessageHandler(HttpContext)))
            {

                var response = await httpClient.GetAsync("https://localhost:44354/weatherforecast");

                var stringResponse = await response.Content.ReadAsStringAsync();
                var items = JsonConvert
                    .DeserializeObject<List<WeatherForecast>>(stringResponse); // this might be worth exception handling

                return View(new BadApiViewModel {Forecasts = items});
            }
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            var errorModel = new ErrorModel();

            errorModel.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var exceptionPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var ex = exceptionPathFeature?.Error;
            if (ex?.Data.Contains("API Route") == true)
            {
                errorModel.ApiRoute = ex.Data["API Route"]?.ToString();
                errorModel.ApiStatus = ex.Data["API Status"]?.ToString();
                errorModel.ApiErrorId = ex.Data["API ErrorId"]?.ToString();
                errorModel.ApiTitle = ex.Data["API Title"]?.ToString();
                errorModel.ApiDetail = ex.Data["API Detail"]?.ToString();
            }

            return View(errorModel);
        }
    }
}
