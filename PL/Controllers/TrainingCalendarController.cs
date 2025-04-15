using BLL.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using PL.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace PL.Controllers
{
    public class TrainingCalendarController : Controller
    {
        private readonly ITrainingCalendar _trainingCalendar;
        private readonly ILogger<TrainingCalendarController> _logger;
        private readonly UserManager<User> _userManager;

        public TrainingCalendarController(ITrainingCalendar trainingCalendar, ILogger<TrainingCalendarController> logger, UserManager<User> userManager)
        {
            _trainingCalendar = trainingCalendar ?? throw new ArgumentNullException(nameof(trainingCalendar));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetTrainingsByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate == default)
                {
                    _logger.LogWarning("Початкова дата не вказана: startDate={StartDate}", startDate);
                    ViewBag.ErrorMessage = "Початкова дата не вказана.";
                    return View("Index");
                }

                if (endDate == default)
                {
                    _logger.LogWarning("Кінцева дата не вказана: endDate={EndDate}", endDate);
                    ViewBag.ErrorMessage = "Кінцева дата не вказана.";
                    return View("Index");
                }

                if (endDate < startDate)
                {
                    _logger.LogWarning("Кінцева дата не може бути раніше початкової: startDate={StartDate}, endDate={EndDate}", startDate, endDate);
                    ViewBag.ErrorMessage = "Кінцева дата не може бути раніше початкової.";
                    return View("Index");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Користувач не авторизований. Перенаправлення на сторінку входу.");
                    return RedirectToAction("Login", "Account");
                }

                int userId = user.Id;
                if (userId <= 0)
                {
                    _logger.LogWarning("Невірний ідентифікатор користувача: userId={UserId}. Перенаправлення на сторінку входу.", userId);
                    return RedirectToAction("Login", "Account");
                }

                // Конвертуємо дати в UTC
                DateTime startDateUtc = startDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc)
                    : startDate.ToUniversalTime();
                DateTime endDateUtc = endDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc)
                    : endDate.ToUniversalTime();

                _logger.LogInformation("Виклик GetTrainingsByDateRangeAsync: userId={UserId}, startDate={StartDate}, endDate={EndDate}", userId, startDateUtc, endDateUtc);
                var trainings = await _trainingCalendar.GetTrainingsByDateRangeAsync(startDateUtc, endDateUtc, userId);

                // Отримуємо дані для гістограми (original dictionary)
                int currentYear = DateTime.UtcNow.Year;
                var rawCounts = await _trainingCalendar.GetMonthlyTrainingCountsAsync(userId, currentYear);

                // 🔁 Перетворюємо ValueTuple в клас
                var monthlyCounts = rawCounts.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new MonthlyTrainingCount
                    {
                        CurrentYearCount = kvp.Value.Item1,
                        PreviousYearCount = kvp.Value.Item2
                    });

                // ViewModel
                var viewModel = new TrainingCalendarViewModel
                {
                    Trainings = trainings,
                    MonthlyTrainingCounts = monthlyCounts,
                    CurrentYear = currentYear
                };

                _logger.LogInformation("Отримано {Count} тренувань", trainings.Count);

                return View("TrainingCalendar", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні тренувань: startDate={StartDate}, endDate={EndDate}. Повне повідомлення: {Message}, StackTrace: {StackTrace}", startDate, endDate, ex.Message, ex.StackTrace);
                ViewBag.ErrorMessage = "Виникла помилка при отриманні тренувань.";
                return View("Index");
            }
        }
    }
}
