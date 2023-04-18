using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using MimeKit;
using NewKino.Models;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace NewKino.Controllers
{

    public class HomeController : Controller
    {
        private static SqliteConnection connection = new SqliteConnection("Data Source=kino1.db");
        private static SqliteCommand command;


        //каждый метод, представленный в контроллере является последним в цепоче URL
       
        public async Task<IActionResult> IndexAsync(string? id1)
        {
            try
            {
                ViewData["Message"] = TempData["Message"];
                connection.Open();
                if (id1 == "Game")
                {
                    List<Movie> movie = new List<Movie>();
                    command = new SqliteCommand("SELECT * FROM Game", connection);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                movie.Add(new Movie { href = reader.GetValue(1).ToString(), html_code = reader.GetValue(2).ToString() });
                            }
                        }
                    }

                    return View(movie);
                }
                else
                if (id1 == "Love")
                {
                    List<Movie> movie = new List<Movie>();
                    command = new SqliteCommand("SELECT * FROM Love", connection);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                movie.Add(new Movie { href = reader.GetValue(1).ToString(), html_code = reader.GetValue(2).ToString() });
                            }
                        }
                    }

                    return View(movie);
                }
                else if (id1 == "Motiv")
                {
                    List<Movie> movie = new List<Movie>();
                    command = new SqliteCommand("SELECT * FROM Motivation", connection);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                movie.Add(new Movie { href = reader.GetValue(1).ToString(), html_code = reader.GetValue(2).ToString() });
                            }
                        }
                    }

                    return View(movie);
                }
                else if (id1 == "Add")
                {
                    List<Movie> movie = new List<Movie>();
                    command = new SqliteCommand("SELECT * FROM AddTable", connection);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                movie.Add(new Movie { href = "Add", html_code = reader.GetValue(1).ToString() });
                            }
                        }
                    }

                    return View(movie);
                }
                else if (id1 == null)
                {
                    List<Movie> movie = new List<Movie>();
                    command = new SqliteCommand("SELECT * FROM Kino", connection);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                movie.Add(new Movie { href = reader.GetValue(1).ToString(), html_code = reader.GetValue(2).ToString() });
                            }
                        }
                    }

                    return View(movie); //View это представленеи из папки Home. IActionResult -это тип возвращаемого значения, который обязателен для View
                }
                else
                {
                    List<Movie> movie = new List<Movie>();
                    command = new SqliteCommand($"SELECT * FROM Kino Where description LIKE \'{"%" + id1.ToLower() + "%"}\'", connection);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                movie.Add(new Movie { href = reader.GetValue(1).ToString(), html_code = reader.GetValue(2).ToString() });
                            }
                        }
                    }
                    if (movie.Count == 0 && HttpContext.User.Identity.IsAuthenticated == true)
                    {
                        command = new SqliteCommand($"SELECT * FROM AddTable Where description LIKE \'{"%" + id1.ToLower() + "%"}\'", connection);
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                while (reader.Read())   // построчно считываем данные
                                {
                                    movie.Add(new Movie { href = "Add", html_code = reader.GetValue(1).ToString() });
                                }
                            }
                        }
                    }
                    return View(movie);
                }
            }
            catch (SystemException ex)
            {
                await Program.client.SendTextMessageAsync("1918705001", $"Ошибка:{ex.Message} Пользователь: {HttpContext.User.Identity?.Name} Время: {DateTime.Now}");
                return RedirectToAction("Index");
            }

        }



        public async Task<IActionResult> Register()
        {
          
            return View(); //View это представленеи из папки Home. IActionResult -это тип возвращаемого значения, который обязателен для View

        }
        public async Task<IActionResult> EntryTrue()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EntryTrue(string? frame,string? description)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                connection.Open();
                frame = frame.Replace("width=\"560\" height=\"315\"", "width=\"380\" height=\"260\"");
                SqliteCommand command = new SqliteCommand($"SELECT COUNT(*) FROM AddTable WHERE html_code=\'{frame}\' OR description=\'{description}\'", connection);
                //command.CommandText = $"SELECT COUNT(*) FROM Add WHERE html_code=\'{frame}\' OR description=\'{description}\'";
                int counter = Convert.ToInt16(command.ExecuteScalar());
                if (counter > 0)
                {
                    ViewData["Message"] = "Кино уже есть";
                }
                else
                if (frame.Contains("<iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/") == true && frame.Contains("title=\"YouTube video player\" frameborder=\"0\" allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share\" allowfullscreen></iframe>") == true && frame.Length == 259 && description != null && description.Length>100)
                {
                    
                    ViewData["Message"] = "Добавлено";
                    command.CommandText = $"INSERT INTO AddTable (html_code,description,user) VALUES (\'{frame}\',\'{description?.ToLower()}\','{HttpContext.User.Identity?.Name}')";
                    await Program.client.SendTextMessageAsync("1918705001", $"Пользователь:{HttpContext.User.Identity?.Name} Отправил видео со следующим описанием:{description}"); //отправка сообщенияв ТГ
                    command.ExecuteNonQuery();
                    Console.WriteLine(HttpContext.User.Identity?.Name);
                }
                else
                {
                    ViewData["Message"] = "Не знаю";
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> Entry()
        {
            return View(); //View это представленеи из папки Home. IActionResult -это тип возвращаемого значения, который обязателен для View
        }
        public async Task<IActionResult> AlmoreProjekt()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entry([Bind("Email","Password")] Users resp)
        {
            command = new SqliteCommand($"SELECT COUNT(*) FROM UsersSite Where Email = \'{resp.Email}\' AND Password='{resp.Password}'", connection);
            int counter = Convert.ToInt16(command.ExecuteScalar());
            if (counter > 0) // если есть данные
            {
                await Authenticate(resp.Email);
                await Program.client.SendTextMessageAsync("1918705001", $"Пользователь: {resp.Email} вошёл в систему в {DateTime.Now.ToString()}");
                return RedirectToAction("Index");

            }
            else
            {
                ViewData["Message"] = "Отказ";
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Name,Surname,Email")] Users resp)
        {
            try { 
            command = new SqliteCommand($"SELECT COUNT(*) FROM UsersSite Where Email = \'{resp.Email}\'", connection);
            int counter = Convert.ToInt16(command.ExecuteScalar());

                if (counter > 0) // если есть данные
                {
                    ViewData["Message"] = "Уже есть";
                    return View();
                }
                else if (resp.Name==null || resp.Surname== null || resp.Email== null)
                {
                    ViewData["Message"] = "Дурак";
                    return View();
                }
                else if (resp.Name.Length>100 || resp.Surname.Length > 100)
                {
                    ViewData["Message"] = "Много";
                    return View();
                }
                { //Отправка сообщения пользователю
                    string password = Guid.NewGuid().ToString();
                    TempData["Message"] = "Успешно";
                    await Program.client.SendTextMessageAsync("1918705001", $"Зарегестрирован новый пользователь:{resp.Email} Имя: {resp.Name} Фамилия: {resp.Surname}");

                    var emailMessage = new MimeMessage();

                    emailMessage.From.Add(new MailboxAddress("Администрация сайта", "hanazuky@yandex.ru"));
                    emailMessage.To.Add(new MailboxAddress("", resp.Email));
                    emailMessage.Subject = $"Здравствуй {resp.Name}.Рад видеть тебя на сайте Kino-Han";
                    emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = $"Это сообщение вам пришло, потому что вы зарегестрировались на сайте Kino-Han.\n\r Хозяин очень рад, что вы проявили внимание к его продукту. Возможно, он даже вас найдёт😁.\n\r Ваш логин: {resp.Email}\n\rПароль:{password}"
                    };

                    using (var client = new SmtpClient())
                    {
                        await client.ConnectAsync("smtp.yandex.ru", 25, false);
                        await client.AuthenticateAsync("hanazuky@yandex.ru", "Hanazuky123");
                        await client.SendAsync(emailMessage);

                        await client.DisconnectAsync(true);
                    }
                    command = new SqliteCommand($"INSERT INTO UsersSite (Name,Surname,Email,Password) VALUES (\'{resp.Name}\',\'{resp.Surname}\',\'{resp.Email}\',\'{password}\')", connection);
                    command.ExecuteNonQuery();

                    return RedirectToAction("Index"); //перенапраправление на главную страницу
                }
            }
            catch (SystemException ex)
            {
                await Program.client.SendTextMessageAsync("1918705001", $"Ошибка:{ex.Message} Пользователь: {HttpContext.User.Identity?.Name} Время: {DateTime.Now}");
                return RedirectToAction("Index");
            }

        }

        public IActionResult AboutDeveloper()
        {
            return View();  //View это представленеи из папки Home 
        }
        public string Welcome(string name, int numTimes = 1)
        {
            return HtmlEncoder.Default.Encode($"Hello {name}, NumTimes is: {numTimes}");
            //? указывается в качестве знака разделителя, после которого указывается строка запроса
            //& амперсентом отделяется пара-ключ значение
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }

}