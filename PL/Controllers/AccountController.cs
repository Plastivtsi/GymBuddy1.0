using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
//using BLL.Models.Interfaces;
using DAL.Models;
using BLL.Models;
using BLL.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PL.Models;
using System.Web;


namespace PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;


        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string nickname, string email, string password)
        {
            // Перевіряємо, чи існує користувач із таким UserName
            var existingUser = await _userManager.FindByNameAsync(nickname);
            if (existingUser != null)
            {
                _logger.LogWarning("Спроба реєстрації з уже існуючим ім'ям користувача: {Nickname}", nickname);
                ViewBag.Error = "Користувач із таким ім'ям уже існує.";
                ModelState.AddModelError(string.Empty, "Користувач із таким ім'ям уже існує.");
                return View();
            }
            var user = new User { UserName = nickname, Email = email };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // Додаємо користувача до ролі "User"
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        _logger.LogError("Помилка додавання ролі 'User' для {Nickname}: {Description}", nickname, error.Description);
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }
                _logger.LogInformation("User {Nickname} created a new account with role 'User'.", nickname); await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                _logger.LogError("Помилка при створенні користувача {Nickname}: {Description}", nickname, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string loginUsername, string loginPassword)
        {
            var result = await _signInManager.PasswordSignInAsync(loginUsername, loginPassword, isPersistent: false, lockoutOnFailure: false);
            var user = await _userManager.FindByNameAsync(loginUsername);

            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Blocked"))
                {
                    ViewBag.Error = $"Ваш акаунт було заблоковано адміністратором. Причина: {user.BlockedReason}";
                    return View();
                }
            }

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} logged in.", loginUsername);

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("HomeAdmin", "Home"); // Перенаправлення на Home/HomeAdmin
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (user != null)
                {
                    var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginPassword);
                    if (!isPasswordCorrect)
                    {
                        ModelState.AddModelError(string.Empty, "Неправильний пароль.");
                        ViewBag.Error = $"Неправильний пароль";
                        _logger.LogWarning("Неправильний пароль для користувача {Username}.", loginUsername);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Користувача з таким ім'ям не знайдено.");
                    _logger.LogWarning("Спроба входу для неіснуючого користувача {Username}.", loginUsername);
                }
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> AssignRole(int userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null && !await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
                return Ok($"Role '{role}' assigned to user {user.UserName}");
            }
            return BadRequest("User not found or already in role");
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                // дописати коли буде підтвердження емейлу 

                if (user == null )
                {
                    // Не повідомляємо, чи існує користувач, для безпеки
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                _logger.LogInformation("Згенеровано токен для користувача {UserId}: {Token}", user.Id, token);

                var encodedToken = HttpUtility.UrlEncode(token);
                _logger.LogInformation("Закодований токен: {EncodedToken}", encodedToken);

               // var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { userId = user.Id, token = HttpUtility.UrlEncode(token) },
                    protocol: Request.Scheme);

                var encodedCallbackUrl = HttpUtility.HtmlEncode(callbackUrl); // Екрануємо URL
                var message = $@"<p>Будь ласка, скиньте ваш пароль, перейшовши за посиланням:</p>
                               <p><a href='{encodedCallbackUrl}'>Скинути пароль</a></p>";

                await _emailService.SendEmailAsync(
                    model.Email,
                    "Скидання паролю",
                    message);

                _logger.LogInformation("Надіслано email для скидання паролю для {Email}", model.Email);
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel {
                Token = HttpUtility.UrlDecode(token),
                UserId = userId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId); // Пошук за UserIdif (user == null)
            if (user == null)
            {
                // Не повідомляємо, чи існує користувач
                return RedirectToAction("ResetPasswordConfirmation");
            }
            _logger.LogInformation("Спроба скидання паролю для користувача {UserId}. Токен: {Token}", model.UserId, model.Token);

            var decodedToken = HttpUtility.UrlDecode(model.Token);
            _logger.LogInformation("Декодований токен: {DecodedToken}", decodedToken);

            // Перевірка валідності токена
            var isValidToken = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                decodedToken);
            _logger.LogInformation("Токен валідний: {IsValid}", isValidToken);

            if (!isValidToken)
            {
                ModelState.AddModelError(string.Empty, "Невалідний токен скидання паролю.");
                _logger.LogError("Токен не пройшов перевірку для користувача {UserId}", model.UserId);
                return View(model);
            }

            // Явно змінюємо пароль
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                foreach (var error in removePasswordResult.Errors)
                {
                    _logger.LogError("Помилка видалення старого паролю для користувача {UserId}: Код: {Code}, Опис: {Description}", model.UserId, error.Code, error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.Password);
            if (addPasswordResult.Succeeded)
            {
                _logger.LogInformation("Пароль успішно скинуто для користувача з ID {UserId}", model.UserId);
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in addPasswordResult.Errors)
            {
                _logger.LogError("Помилка встановлення нового паролю для користувача {UserId}: Код: {Code}, Опис: {Description}", model.UserId, error.Code, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

    }
}
