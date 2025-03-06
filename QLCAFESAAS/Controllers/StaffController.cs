using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QLCAFESAAS.Models.Repository;

namespace QLCAFESAAS.Controllers
{
    public class StaffController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly string _staffPermission = "staff";

        public StaffController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }


        public IActionResult Index()
        {
           if(!CheckAccess(_staffPermission))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return View();
        }


        private bool CheckAccess(string Permission)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            if (User.FindFirst(ClaimTypes.Role)?.Value != Permission)
            {
                return false;
            }

            return true;
        }

        public IActionResult NV()
        {
            return View();
        }
    }
}
