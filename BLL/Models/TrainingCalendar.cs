using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLL.Models.Interfaces;

namespace BLL.Models
{
    public class TrainingCalendar : ITrainingCalendar
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TrainingCalendar> _logger;

        public TrainingCalendar(ApplicationDbContext context, ILogger<TrainingCalendar> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("Створено екземпляр TrainingCalendar для роботи з тренуваннями.");
        }

        public async Task<List<DAL.Models.Training>> GetTrainingsByDateRangeAsync(DateTime startDate, DateTime endDate, int userId)
        {
            _logger.LogInformation("Розпочато виконання GetTrainingsByDateRangeAsync: userId={UserId}, startDate={StartDate}, endDate={EndDate}.", userId, startDate, endDate);

            // Перевірка вхідних параметрів
            if (startDate == default)
            {
                _logger.LogWarning("Початкова дата не може бути порожньою: startDate={StartDate}.", startDate);
                throw new ArgumentException("Початкова дата не може бути порожньою.");
            }

            if (endDate == default)
            {
                _logger.LogWarning("Кінцева дата не може бути порожньою: endDate={EndDate}.", endDate);
                throw new ArgumentException("Кінцева дата не може бути порожньою.");
            }

            if (endDate < startDate)
            {
                _logger.LogWarning("Кінцева дата не може бути раніше початкової: startDate={StartDate}, endDate={EndDate}.", startDate, endDate);
                throw new ArgumentException("Кінцева дата не може бути раніше початкової.");
            }

            if (userId <= 0)
            {
                _logger.LogWarning("Невірний ідентифікатор користувача: userId={UserId}.", userId);
                throw new ArgumentException("Ідентифікатор користувача повинен бути більше 0.");
            }

            try
            {
                // Перевірка підключення до бази
                _logger.LogInformation("Перевірка підключення до бази даних для userId={UserId}...", userId);
                bool canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Не вдалося підключитися до бази даних для userId={UserId}.", userId);
                    return new List<DAL.Models.Training>();
                }
                _logger.LogInformation("Підключення до бази даних успішне для userId={UserId}.", userId);

                // Отримуємо тренування за діапазоном дат
                _logger.LogInformation("Виконуємо запит до бази даних для отримання тренувань у діапазоні від {StartDate} до {EndDate} для userId={UserId}...", startDate, endDate, userId);
                var trainings = await _context.Trainings
                    .Where(t => t.UserId == userId &&
                                t.Date.HasValue &&
                                t.Date.Value >= startDate &&
                                t.Date.Value <= endDate)
                    .Include(t => t.Exercises)
                    .OrderBy(t => t.Date)
                    .ToListAsync();

                // Логуємо результат запиту
                if (!trainings.Any())
                {
                    _logger.LogInformation("Тренування для користувача {UserId} у діапазоні від {StartDate} до {EndDate} не знайдені.", userId, startDate, endDate);
                }
                else
                {
                    _logger.LogInformation("Знайдено {Count} тренувань для користувача {UserId} у діапазоні від {StartDate} до {EndDate}: {Trainings}",
                        trainings.Count, userId, startDate, endDate,
                        string.Join("; ", trainings.Select(t => $"Date={t.Date}, Name={t.Name}")));
                }

                _logger.LogInformation("GetTrainingsByDateRangeAsync виконано успішно для userId={UserId}.", userId);
                return trainings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні тренувань для користувача {UserId} у діапазоні від {StartDate} до {EndDate}. Повне повідомлення: {Message}, StackTrace: {StackTrace}", userId, startDate, endDate, ex.Message, ex.StackTrace);
                return new List<DAL.Models.Training>();
            }
        }

        public async Task<Dictionary<int, (int CurrentYearCount, int PreviousYearCount)>> GetMonthlyTrainingCountsAsync(int userId, int currentYear)
        {
            _logger.LogInformation("Розпочато виконання GetMonthlyTrainingCountsAsync: userId={UserId}, currentYear={CurrentYear}.", userId, currentYear);

            // Перевірка вхідних параметрів
            if (userId <= 0)
            {
                _logger.LogWarning("Невірний ідентифікатор користувача: userId={UserId}.", userId);
                throw new ArgumentException("Ідентифікатор користувача повинен бути більше 0.");
            }

            if (currentYear < 2000 || currentYear > DateTime.UtcNow.Year)
            {
                _logger.LogWarning("Невірний рік: currentYear={CurrentYear}.", currentYear);
                throw new ArgumentException("Рік повинен бути у допустимому діапазоні.");
            }

            try
            {
                // Перевірка підключення до бази даних
                _logger.LogInformation("Перевірка підключення до бази даних для userId={UserId}, currentYear={CurrentYear}...", userId, currentYear);
                bool canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Не вдалося підключитися до бази даних під час отримання даних для гістограми: userId={UserId}, currentYear={CurrentYear}.", userId, currentYear);
                    return new Dictionary<int, (int, int)>();
                }
                _logger.LogInformation("Підключення до бази даних успішне для userId={UserId}, currentYear={CurrentYear}.", userId, currentYear);

                int previousYear = currentYear - 1;
                _logger.LogInformation("Розпочинаємо обробку даних для гістограми: userId={UserId}, currentYear={CurrentYear}, previousYear={PreviousYear}.", userId, currentYear, previousYear);

                // Отримуємо тренування за два роки
                _logger.LogInformation("Виконуємо запит до бази даних для отримання тренувань за два роки...");
                var trainings = await _context.Trainings
                    .Where(t => t.UserId == userId &&
                                t.Date.HasValue &&
                                (t.Date.Value.Year == currentYear || t.Date.Value.Year == previousYear))
                    .ToListAsync();

                // Логуємо кількість отриманих тренувань
                if (!trainings.Any())
                {
                    _logger.LogInformation("Тренування для користувача {UserId} за роки {CurrentYear} і {PreviousYear} не знайдені.", userId, currentYear, previousYear);
                }
                else
                {
                    _logger.LogInformation("Знайдено {Count} тренувань для користувача {UserId} за роки {CurrentYear} і {PreviousYear}: {Trainings}",
                        trainings.Count, userId, currentYear, previousYear,
                        string.Join("; ", trainings.Select(t => $"Date={t.Date}, Name={t.Name}")));
                }

                // Групуємо тренування по місяцях для поточного року
                _logger.LogInformation("Групуємо тренування за місяцями для поточного року {CurrentYear}...", currentYear);
                var currentYearCounts = trainings
                    .Where(t => t.Date.HasValue && t.Date.Value.ToUniversalTime().Year == currentYear)
                    .GroupBy(t => t.Date.Value.ToUniversalTime().Month)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count());

                // Логуємо результат групування
                if (!currentYearCounts.Any())
                {
                    _logger.LogInformation("Тренування для поточного року {CurrentYear} для користувача {UserId} не знайдені.", currentYear, userId);
                }
                else
                {
                    _logger.LogInformation("Результати групування для поточного року {CurrentYear}: {Counts}",
                        currentYear,
                        string.Join("; ", currentYearCounts.Select(kvp => $"Month={kvp.Key}, Count={kvp.Value}")));
                }

                // Групуємо тренування по місяцях для попереднього року
                _logger.LogInformation("Групуємо тренування за місяцями для попереднього року {PreviousYear}...", previousYear);
                var previousYearCounts = trainings
                    .Where(t => t.Date.Value.Year == previousYear)
                    .GroupBy(t => t.Date.Value.Month)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count());

                // Логуємо результат групування для попереднього року
                if (!previousYearCounts.Any())
                {
                    _logger.LogInformation("Тренування для попереднього року {PreviousYear} для користувача {UserId} не знайдені.", previousYear, userId);
                }
                else
                {
                    _logger.LogInformation("Результати групування для попереднього року {PreviousYear}: {Counts}",
                        previousYear,
                        string.Join("; ", previousYearCounts.Select(kvp => $"Month={kvp.Key}, Count={kvp.Value}")));
                }

                // Формуємо результат
                _logger.LogInformation("Формуємо кінцевий результат для гістограми...");
                var result = new Dictionary<int, (int CurrentYearCount, int PreviousYearCount)>();
                for (int month = 1; month <= 12; month++)
                {
                    int currentCount = currentYearCounts.ContainsKey(month) ? currentYearCounts[month] : 0;
                    int previousCount = previousYearCounts.ContainsKey(month) ? previousYearCounts[month] : 0;
                    result[month] = (currentCount, previousCount);
                    _logger.LogDebug("Місяць {Month}: {CurrentYear}={CurrentCount}, {PreviousYear}={PreviousCount}", month, currentYear, currentCount, previousYear, previousCount);
                }

                // Логуємо кінцевий результат
                _logger.LogInformation("Кінцевий результат для гістограми: {Result}",
                    string.Join("; ", result.Select(kvp => $"Month={kvp.Key}, {currentYear}={kvp.Value.CurrentYearCount}, {previousYear}={kvp.Value.PreviousYearCount}")));

                _logger.LogInformation("Дані для гістограми успішно отримані: userId={UserId}, currentYear={CurrentYear}, previousYear={PreviousYear}.", userId, currentYear, previousYear);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні даних для гістограми: userId={UserId}, currentYear={CurrentYear}. Повне повідомлення: {Message}, StackTrace: {StackTrace}", userId, currentYear, ex.Message, ex.StackTrace);
                return new Dictionary<int, (int, int)>();
            }
        }
    }
}