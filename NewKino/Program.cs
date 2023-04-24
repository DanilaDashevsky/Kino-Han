using AngleSharp;
using Microsoft.Data.Sqlite;
using MimeKit;
using Newtonsoft.Json.Linq;
using MailKit.Net.Smtp;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using Microsoft.AspNetCore.Authentication.Cookies;
using Telegram.Bot.Types.Enums;
using static Org.BouncyCastle.Math.EC.ECCurve;
using AngleSharp.Dom;

internal class Program
{
    public static SqliteConnection connection;
    public static SqliteCommand command;
    public static string muvie;
    public static YoutubeClient youtube = new YoutubeClient();
    public static string token = "";
    public static TelegramBotClient client = new TelegramBotClient(token);
    private static async Task Main(string[] args)
    {
        connection = new SqliteConnection("Data Source=kino1.db");
        connection.Open();
        client.StartReceiving();
        client.OnMessage += OnMessageHandler;


        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthentication(
           CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
       var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
        app.UseAuthentication();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }



    private static async void OnMessageHandler(object? sender, MessageEventArgs e)
    {
        try
        {

            var msg = e.Message;
            Username1 = msg.Chat.Username;
            Firstname1 = msg.Chat.FirstName;
            Lastname1 = msg.Chat.LastName;
            if (msg.Text != null)
            {
                if (counter_question > 0 && msg.Text != "")
                {
                    counter_question = 0;
                    if (msg.Text != "😎Видео с сайта разработчика😎" && msg.Text != "😍Связаться с разработчиком😍" && msg.Text != "😎Мотивационные😎" && msg.Text != "🎥Узнать информацию о видео с YouTube🎞")
                    {
                        await client.SendTextMessageAsync("1918705001", $"ЧатID {msg.Chat.Id} Cообщение:{msg.Text} Username: {msg.Chat.Username} Имя: {msg.Chat.FirstName} Фамилия: {msg.Chat.LastName} Время:{DateTime.Now.ToString()}");
                        await client.SendTextMessageAsync(msg.Chat.Id, "Хорошо, ожидайте. Администратор ответит вам в личном сообщении в ближайшее время!");
                    }
                    else
                    {
                        await client.SendTextMessageAsync(msg.Chat.Id, "Простите, нельзя использовать команды из меню, чтобы написать разработчику");
                    }
                }

                if ((table == "Love" || table == "Game" || table == "Motivation") && msg.Text != "")
                {
                    command = new SqliteCommand($"SELECT * FROM {table} Where description = \'{msg.Text}\'", connection);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            reader.Read();
                            var video = await youtube.Videos.GetAsync(reader.GetValue(1).ToString());
                            await client.SendTextMessageAsync(msg.Chat.Id, video.Url);
                        }
                    }
                }

                
                
                switch (msg.Text)
                {
                    case "/start": { await client.SendTextMessageAsync(msg.Chat.Id, $"О,{msg.Chat.FirstName} привет😍!", replyMarkup: await GetMenu()); await client.SendTextMessageAsync(msg.Chat.Id, $"Рад видеть тебя в своём чат-боте по продаже электроники!", replyMarkup: await GetSite()); break; }
                    case "😎Видео с сайта разработчика😎": { await client.SendTextMessageAsync(msg.Chat.Id, ":)", replyMarkup: await GetMenuDeveloper()); break; }
                    case "⬅Назад⬅": { table = ""; counter_question = 0; await client.SendTextMessageAsync(msg.Chat.Id, ":)", replyMarkup: await GetMenu()); break; }
                    case "🕹Играть🕹": { await client.SendTextMessageAsync(msg.Chat.Id, "Отлично, давай поиграем.😏 Напиши что-нибудь, а я попробую найти картинку по твоему запросу", replyMarkup: await GetMenuPalyYoutube()); table = "play"; break; }
                    case "😍Связаться с разработчиком😍": { await client.SendTextMessageAsync(msg.Chat.Id, "Отправте ваш вопрос:", replyMarkup: await GetMenu()); ++counter_question; break; }
                    case "😎Мотивационные😎": { table = "Motivation"; await client.SendTextMessageAsync(msg.Chat.Id, "Отличный выбор💪", replyMarkup: await GetMenuSite("Motivation")); break; }
                    case "😍Про любовь😍": { table = "Love"; await client.SendTextMessageAsync(msg.Chat.Id, "Отличный выбор❤", replyMarkup: await GetMenuSite("Love")); break; }
                    case "😍Игры😍": { table = "Game"; await client.SendTextMessageAsync(msg.Chat.Id, "Отличный выбор🎮", replyMarkup: await GetMenuSite("Game")); break; }
                    case "🎥Узнать информацию о видео с YouTube🎞": { table = "YouTube"; await client.SendTextMessageAsync(msg.Chat.Id, "Хорошо, скинь мне ссылку на видео:", replyMarkup: await GetMenuPalyYoutube()); break; }
                }
                if (table == "play" && msg.Text != null && msg.Text != "🕹Играть🕹" && msg.Text != "⬅Назад⬅")
                {


                    await client.SendTextMessageAsync("1918705001", $"Этот человек отправил след.запрос во вкладке Играть чатID: {msg.Chat.Id} UserName:{msg.Chat.Username} Имя: {msg.Chat.FirstName} Фамилия: {msg.Chat.LastName} Text:{msg.Text}");
                    var config = Configuration.Default.WithDefaultLoader();
                    string request = msg.Text;
                    Random rnd = new Random();
                    List<string> list_ref = new List<string>();
                    var address = "https://yandex.ru/images/search?text=" + request.Replace(" ", "%20");
                    var document = await BrowsingContext.New(config).OpenAsync(address);
                    foreach (var element in document.QuerySelectorAll("img"))
                    {
                        if (element.ClassName == "serp-item__thumb justifier__thumb")
                        {
                            list_ref.Add("https:" + element.GetAttribute("src").ToString());
                        }
                    }

                    if (list_ref.Count >= 1)
                        await client.SendPhotoAsync(msg.Chat.Id, list_ref[rnd.Next(0, list_ref.Count - 1)]);
                    else
                        await client.SendTextMessageAsync(msg.Chat.Id, "Извините, по вашему запросу ничего не найдено");
                }
                if (table == "YouTube" && msg.Text.Contains("youtu"))
                {
                    var video = await youtube.Videos.GetAsync(msg.Text);
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(msg.Text);
                    var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

                    await client.SendTextMessageAsync(msg.Chat.Id, "Автор: " + video.Author.ToString());
                    await client.SendTextMessageAsync(msg.Chat.Id, "Продолжительность: " + video.Duration.ToString());
                    await client.SendTextMessageAsync(msg.Chat.Id, "Размер: " + streamInfo.Size.ToString());
                    await client.SendTextMessageAsync(msg.Chat.Id, "Ссылка для прямого просмотра без рекламы: " + streamInfo.Url.ToString());
                }
                else if (table == "YouTube" && msg.Text != "" && msg.Text != "🎥Узнать информацию о видео с YouTube🎞")
                {
                    await client.SendTextMessageAsync(msg.Chat.Id, "Это ссылка не с Ютуба.");
                }

            }
        }
        catch (SystemException ex)
        {
            await client.SendTextMessageAsync("1918705001", $"{Username1} {Firstname1} {Lastname1} Ошибка{ex.Message}");
        }
    }
    private static async Task<IReplyMarkup> GetMenu()
    {
        return new ReplyKeyboardMarkup
        {
            Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton> { new KeyboardButton { Text = "😎Видео с сайта разработчика😎" }, new KeyboardButton { Text = "😍Связаться с разработчиком😍" } },
                new List<KeyboardButton> { new KeyboardButton { Text = "🕹Играть🕹" }, new KeyboardButton { Text = "🎥Узнать информацию о видео с YouTube🎞" } }
            }
        };
    }
    private static async Task<IReplyMarkup> GetMenuDeveloper()
    {
        return new ReplyKeyboardMarkup
        {
            Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton> { new KeyboardButton { Text = "😎Мотивационные😎" }, new KeyboardButton { Text = "😍Про любовь😍" }, new KeyboardButton { Text = "😍Игры😍" }  },
                new List<KeyboardButton> { new KeyboardButton { Text = "⬅Назад⬅" } }
            }
        };
    }
    private static async Task<IReplyMarkup> GetMenuPalyYoutube()
    {
        return new ReplyKeyboardMarkup
        {
            Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton> { new KeyboardButton { Text = "⬅Назад⬅" } }
            }
        };
    }
    private static async Task<IReplyMarkup> GetMenuSite(string table)
    {
        ReplyKeyboardMarkup Keyboard1 = new ReplyKeyboardMarkup();
        List<List<KeyboardButton>> list = new List<List<KeyboardButton>>();
        command = new SqliteCommand($"SELECT * FROM '{table}'", connection);
        list.Add(new List<KeyboardButton> { new KeyboardButton { Text = "⬅Назад⬅" } });
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            if (reader.HasRows) // если есть данные
            {
                while (reader.Read())   // построчно считываем данные
                {
                    list.Add(new List<KeyboardButton> { new KeyboardButton { Text = reader.GetValue(3).ToString() } });
                }
            }

        }
        Keyboard1.Keyboard = list;
        return Keyboard1;
    }

    public static async Task WriteUser(string? Username1, string? Firstname1, string? Lastname1)
    {
        int u = 0;
        command = new SqliteCommand($"SELECT * FROM Users Where Username = \'{Username1}\'", connection);
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            if (reader.HasRows == false) // если есть данные
            {
                u++;
            }
        }
        if (u > 0)
        {
            if (Lastname1 == null)
                Lastname1 = "none";
            if (Username1 == "")
                Username1 = "none";
            command.CommandText = $"INSERT INTO Users (Username,Firstname,Lastname) values (\'{Username1}\',\'{Firstname1}\',\'{Lastname1}\')";
            await client.SendTextMessageAsync("1918705001", $" Вот этот человек начал пользоваться вашим ботом UserName:{Username1} Имя: {Firstname1} Фамилия: {Lastname1}");
            command.ExecuteNonQuery();
        }
    }
    public static async Task<InlineKeyboardMarkup> GetSite()
    {
        InlineKeyboardButton urlButton = new InlineKeyboardButton();

        urlButton.Text = "Перейти на сайт разработчика";
        urlButton.Url = "http://hanazuky1-001-site1.ctempurl.com/";

        InlineKeyboardButton[] buttons = new InlineKeyboardButton[] { urlButton };

        InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);

        return inline;
    }

    public static int counter_question;
    public static int counter_play;
    public static string Username1;
    public static string Firstname1;
    public static string Lastname1;
    public static string table;
}