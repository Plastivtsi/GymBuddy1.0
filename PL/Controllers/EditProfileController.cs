using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Service;
using Serilog;


namespace PL.Controllers
{
    public class EditProfileController : Controller
    {
        private readonly IUserService _userService;

        public EditProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = this.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // ���� ���������� �� �������������
            }

            var user = await _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(); // ���� ����������� �� ��������
            }

            return View(user); // ��������� ������ ����������� ��� �����������
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User model)
        {
            Log.Information("�������� ��������� ��� ����������� ID: {UserId}", model.Id);

            if (ModelState.IsValid)
            {
                _userService.UpdateUser(model);
                TempData["SuccessMessage"] = "���� ���������!";
                return RedirectToAction("Profile", "Index");
            }

            return View(model);
        }
    }
}

