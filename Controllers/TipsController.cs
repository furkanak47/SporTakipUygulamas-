using Microsoft.AspNetCore.Mvc;

namespace SporTakip.Controllers;

public class TipsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}