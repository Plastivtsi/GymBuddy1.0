using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using BLL.Models.Interfaces;

namespace BLL.Models
{
    public class Autorization : ICreateUser
    {
        private static int currentUserId;
        public static int CurrentUserId { get => currentUserId; set => currentUserId = value; }
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Autorization> _logger;

        public Autorization(ApplicationDbContext context, ILogger<Autorization> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public virtual async Task<DAL.Models.User> CreateNewUser(string nickname, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(nickname) ||
                   string.IsNullOrWhiteSpace(email) ||
                   string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Всі поля повинні бути заповненими");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Name == nickname);
            if (userExists)
            {
                _logger.LogWarning("Користувач {Nickname} вже існує", nickname);
                throw new Exception("Такий користувач вже існує");
            }

            var user = new DAL.Models.User
            {
                Name = nickname,
                Email = email,
                Password = password,
                Weight = 50,
                Height = 150,
                Role = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Користувач {Nickname} успішно створений.", nickname);
            return user;
            
        }

        public bool Login(string nickname, string password)
        {
            if (string.IsNullOrWhiteSpace(nickname) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Обидва поля повинні бути заповнені.");
            }

            var user = _context.Users.FirstOrDefault(u => u.Name == nickname);
            if (user == null)
            {
                _logger.LogWarning("Користувач {Nickname} не знайдений.", nickname);
                throw new InvalidOperationException("Користувача не знайдено.");
            }

            if (user.Password != password)
            {
                _logger.LogWarning("Невірний пароль для користувача {Nickname}.", nickname);
                throw new InvalidOperationException("Невірний пароль.");
            }

            CurrentUserId = user.Id;
            _logger.LogInformation("Користувач {Nickname} успішно увійшов у систему.", nickname);
            return true;
        }
    }
}   
