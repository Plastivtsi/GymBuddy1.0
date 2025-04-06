using Microsoft.AspNetCore.Mvc;
using DAL.Models;
using BLL.Interfaces;
using Serilog;
using System.Security.Claims;

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
        public IActionResult Edit()
        {
            // �������� ID ����������� � Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // ���� ���������� �� �������������
            }

            // ���������� �� ��������� ����������
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(); // ���� ����������� �� ��������
            }

            return View(user); // ��������� ����������� ��� �����������
        }

        [HttpPost]
        public IActionResult Edit(User model)
        {
            Log.Information("�������� ��������� ��� ����������� ID: {UserId}", model.Id);

            if (ModelState.IsValid)
            {
                _userService.UpdateUser(model); // ��������� ��� �����������
                TempData["SuccessMessage"] = "���� ���������!";
                return RedirectToAction("Profile", "Index"); // ��������������� �� ������� �������
            }

            return View(model); // ���� ������ �� ������, ��������� ����� �� �����
        }
    }
}
