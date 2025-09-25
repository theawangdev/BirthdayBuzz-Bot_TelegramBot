using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBuzz_Bot_NETCore
{
    internal class Program
    {
        private static string _strAppName = "BirthdayBuzz-Bot";
        public static string strAppName
        {
            get { return _strAppName; }
        }

        // Telegram Bot EV
        private static readonly string strTelegram_Bot_Token = Environment.GetEnvironmentVariable("Telegram__Bot_Token");
        private static TelegramBotClient _TelegramBotClient = new TelegramBotClient(strTelegram_Bot_Token);
        private static readonly string strBot_Owner_ID = Environment.GetEnvironmentVariable("Telegram__Bot_Owner_ID");
        private static readonly long longBot_Owner_ID = long.Parse(strBot_Owner_ID);

        // Firebase EV
        private static FirebaseHelper _FirebaseHelper = new FirebaseHelper();

        // Convert TimeZone to Malaysia Time Zone
        private static readonly TimeZoneInfo _MalaysiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kuala_Lumpur");
        private static DateTime dtMalaysia => TimeZoneInfo.ConvertTime(DateTime.UtcNow, _MalaysiaTimeZone);

        static async Task Main()
        {
            try
            {
                // Run ScheduleDailyCheckBirthdays immediately after startup or when Bot is trigger
                _ = ScheduleDailyCheckBirthdays();

                _TelegramBotClient.OnMessage += Bot_OnMessage;
                _TelegramBotClient.OnCallbackQuery += Bot_OnCallbackQuery;
                _TelegramBotClient.StartReceiving();
                
                // Kepp App alive
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{dtMalaysia}]: [STARTUP ERROR]: {ex.Message}");
                await _TelegramBotClient.SendTextMessageAsync(
                    chatId: longBot_Owner_ID,
                    text: $"<b>[{dtMalaysia}]:</b> [STARTUP ERROR]: {ex.Message}",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                );
            }
        }

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            var varTeleChatID = e.Message.Chat.Id;
            var varText = e.Message.Text?.Trim();

            // Get Subscriber strSubscriberStatus from Firebase: Pending, Approved or Declined
            string status = await _FirebaseHelper.GetUserStatusAsync(varTeleChatID);

            if (varText == "/start")
            {
                await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "𝗪𝗲𝗹𝗸𝗲𝗺 𝗞𝗲𝗿𝗮𝗯𝗮𝘁! (づ ◕‿◕ )づ");
                await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "Daftar dulu baru boleh guna Bot ni.");
                await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "Guna /daftar [Nama] [60nomborTeponWasap].\n\nContoh: /daftar Sarip Dol 60123456789.");
            }

            if (varText != null && !varText.StartsWith("/daftar"))
            {
                if (string.IsNullOrEmpty(status) || status == "Pending" || status == "Declined")
                {
                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID,
                        "⚠️ ID anda belum di approve. Hanya boleh guna /daftar sehingga Admin approve.");
                    return;
                }
            }

            // Continue existing command handling
            if (varText.StartsWith("/daftar"))
            {
                await RegisterNewSubscriber(sender, e);
            }

            // Check if User berdaftar
            else if (!await _FirebaseHelper.IsSubscribed(varTeleChatID))
            {
                await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "❌ User tak berdaftar.");
                await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "Daftar dulu baru boleh tengok Birthday List.");
                await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "Guna /daftar [Nama] [60nomborTeponWasap].\n\nContoh: /daftar Sarip Dol 60123456789.");
            }
            
            // Command tengok list Birthday Data
            else if (varText == "/list")
            {
                await ListBirthdays(sender, e);
            }
            
            // Command tambah Birthday Data
            else if (varText?.StartsWith("/tambah") == true && varTeleChatID == longBot_Owner_ID)
            {
                await AddBirthdays(sender, e);
            }
            
            // Command semak hari kelahiran
            else if (varText == "/check" && varTeleChatID == longBot_Owner_ID)
            {
                Console.WriteLine($"[{dtMalaysia}]: Admin Bot run /check command.");

                await ManualCheckBirthdays();
            }
            
            // Selain command yang ada
            else
            {
                if (varTeleChatID == longBot_Owner_ID)
                {
                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "Salah command boss. 🤖" +
                        "\n\n📌 𝗦𝗘𝗡𝗔𝗥𝗔𝗜 𝗖𝗢𝗠𝗠𝗔𝗡𝗗 𝗕𝗢𝗧" +
                        "\n\n➕ /tambah: Tambah Birthday Data" +
                        "\n📋 /list: Tengok list Birthday Data" +
                        "\n🔎 /check: Semak hari kelahiran");
                }
                else
                {
                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "Salah command. 🤖" +
                        "\n\n📌 𝗦𝗘𝗡𝗔𝗥𝗔𝗜 𝗖𝗢𝗠𝗠𝗔𝗡𝗗 𝗕𝗢𝗧" +
                        "\n\n📝 /daftar: Daftar pengguna baru" +
                        "\n📋 /list: Tengok list Birthday");
                }  
            }
        }

        private static async Task ScheduleDailyCheckBirthdays()
        {
            var AppAssembly = Assembly.GetExecutingAssembly();
            var AppVersion = FileVersionInfo.GetVersionInfo(AppAssembly.Location).FileVersion;

            Console.WriteLine($"[{dtMalaysia}]: {strAppName} v{AppVersion} is running...");
            await _TelegramBotClient.SendTextMessageAsync(
                    chatId: longBot_Owner_ID,
                    text: $"<b>[{dtMalaysia}]:</b> {strAppName} v{AppVersion} is running...",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );

            Console.WriteLine($"[{dtMalaysia}]: Bot buat semakan hari kelahiran...");
            await _TelegramBotClient.SendTextMessageAsync(
                    chatId: longBot_Owner_ID,
                    text: $"<b>[{dtMalaysia}]:</b> Bot buat semakan hari kelahiran...",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );

            while (true)
            {
                var varDTNow = dtMalaysia;

                // Declare var to run at 00:00 (24 hour format)
                var varMidnightTonight = varDTNow.Date.AddDays(1);

                // Declare var to run at 12:00 (24 hour format)
                var varNoonToday = varDTNow.Date.AddHours(12);

                // Declare var to run at Midnight or Noon
                DateTime dtNextRun;

                // Run SendDailyBirthdays at 12:00 Noon
                if (varDTNow < varNoonToday)
                {
                    dtNextRun = varNoonToday;
                }

                // Run SendDailyBirthdays at 00:00 Midnight
                else if (varDTNow < varMidnightTonight)
                {
                    dtNextRun = varMidnightTonight;
                }

                // Just in case past 00:00 Midnight, run at 12:00 Noon
                else
                {
                    dtNextRun = varDTNow.Date.AddDays(1).AddHours(12);
                }

                var varTimeUntilNextRun = dtNextRun - varDTNow;

                int intHours = (int)varTimeUntilNextRun.TotalHours;
                int intMinutes = varTimeUntilNextRun.Minutes;

                // Run SendDailyBirthdays Daily at 00:00 Midnight or 12:00 Noon
                await SendDailyBirthdays();

                Console.WriteLine($"[{dtMalaysia}]: Bot buat semakan hari kelahiran lagi pada: {dtNextRun:dd/MM/yyyy HH:mm tt} ({intHours} jam {intMinutes} minit) dari sekarang.");
                await _TelegramBotClient.SendTextMessageAsync(
                    chatId: longBot_Owner_ID,
                    text: $"<b>[{dtMalaysia}]:</b> ⏳ Bot buat semakan hari kelahiran lagi pada:" +
                    $"\n{dtNextRun:dd/MM/yyyy HH:mm tt} ({intHours} jam {intMinutes} minit) dari sekarang.",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                );

                await Task.Delay(varTimeUntilNextRun);
            }
        }

        // Method SendDailyBirthdays when startup
        private static async Task SendDailyBirthdays()
        {
            var today = dtMalaysia.Date;
            var birthdays = await _FirebaseHelper.GetAllBirthdayDataAsync();
            var subs = await _FirebaseHelper.GetTelegramSubscriberDataAsync();

            var matches = birthdays
                .Where(b => b.dtBirthdayDate.Month == today.Month &&
                            b.dtBirthdayDate.Day == today.Day)
                .ToList();

            if (!matches.Any())
            {
                Console.WriteLine($"[{dtMalaysia}]: Takde birthday sesiapa harini.");
                await _TelegramBotClient.SendTextMessageAsync(
                    chatId: longBot_Owner_ID,
                    text: $"<b>[{dtMalaysia}]:</b> 🙅🏻‍♂️ Takde birthday sesiapa harini.",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                );

                return;
            }

            foreach (var sub in subs)
            {
                foreach (var b in matches)
                {
                    string strBirthdayInfo = $"🎉 𝗜𝗡𝗙𝗢! 𝗜𝗡𝗙𝗢! Harini birthday <b>{b.strName}</b> laaahh! (<b>{b.dtBirthdayDate:dd/MM}</b>) 🥳 Pergi wish cepat! 💌";

                    string strBirthdayWish = $"🎉 𝑯𝒂𝒑𝒑𝒚 𝑩𝒊𝒓𝒕𝒉𝒅𝒂𝒚 {b.strName} 🎂" +
                        $"\nBarakallahu fi umrik 🤲🏻✨" +
                        $"\n." +
                        $"\n🌺 Dipanjangkan umur penuh keberkatan" +
                        $"\n🍃 Dilimpahi rezeki seluas lautan" +
                        $"\n🕌 Dikuatkan keimanan & taqwa" +
                        $"\n💪🏻 Diberi kesihatan yang baik & berpanjangan" +
                        $"\n🏅 Dikurniakan kejayaan dan kebahagiaan dunia & akhirat" +
                        $"\n." +
                        $"\n." +
                        $"\nAllahumma aamiin. 🤲🏻💌" +
                        $"\n🎁🎈🍰🧁🎊";

                    // Send Birthday Info
                    await _TelegramBotClient.SendTextMessageAsync(
                    chatId: sub.longTeleChatID,
                    text: strBirthdayInfo,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );

                    // Send Birthday Wish
                    await _TelegramBotClient.SendTextMessageAsync(sub.longTeleChatID, strBirthdayWish);
                }
            }

            Console.WriteLine($"[{dtMalaysia}]: Harini ada birthday seseorang! Ingatan hari kelahiran telah di broadcast.");
            await _TelegramBotClient.SendTextMessageAsync(
                    chatId: longBot_Owner_ID,
                    text: $"<b>[{dtMalaysia}]:</b> 🎉 Harini ada birthday seseorang! Ingatan hari kelahiran telah di broadcast.",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );
        }

        // Method RegisterNewSubscriber when command /daftar is call
        private static async Task RegisterNewSubscriber(object sender, MessageEventArgs e)
        {
            var varTeleChatID = e.Message.Chat.Id;
            var varText = e.Message.Text?.Trim();

            var parts = varText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Proses /daftar kena ada sekurangnya 3 patah perkataan & diakhiri dengan nombor WhatsApp bermula 60
            if (parts.Length >= 3 && parts[^1].StartsWith("60"))
            {
                string strWhatsApp_PhoneNumber = parts[^1];

                // Join all between /daftar & strWhatsApp_PhoneNumber
                string strName = string.Join(" ", parts.Skip(1).Take(parts.Length - 2));

                // Check if Subscriber already registered
                if (await _FirebaseHelper.IsSubscribed(varTeleChatID))
                {
                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "✅ ID sudah berdaftar sebelum ni." +
                        "\n\n📌 𝗦𝗘𝗡𝗔𝗥𝗔𝗜 𝗖𝗢𝗠𝗠𝗔𝗡𝗗 𝗕𝗢𝗧" +
                        "\n\n📝 /daftar: Daftar pengguna baru" +
                        "\n📋 /list: Tengok list Birthday Data");
                }
                else
                {
                    // Store New Subscriber Data with Pending strSubscriberStatus until Admin Bot approve
                    await _FirebaseHelper.AddTelegramSubscriberDataAsync(varTeleChatID, strName, strWhatsApp_PhoneNumber, "Pending");

                    // Inform New Subscriber registration has been submitted
                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID,
                        "📤 Pendaftaran telah dihantar." +
                        "\n\nSila tunggu Admin approve.");

                    Console.WriteLine($"[{dtMalaysia}]: Pendaftaran baru diterima, menunggu Admin Bot Approve / Decline. - Nama: {strName}, No. WhatsApp: {strWhatsApp_PhoneNumber}, Tele Chat ID: {varTeleChatID}");

                    // Inform Admin Bot for New Subscriber registration
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("✅ Approve", $"approve:{varTeleChatID}"),
                            InlineKeyboardButton.WithCallbackData("❌ Decline", $"decline:{varTeleChatID}")
                        }
                    });

                    await _TelegramBotClient.SendTextMessageAsync(longBot_Owner_ID,
                        $"📥 *Pendaftaran Subscriber Baru*\n\n" +
                        $"👤 Nama: {strName}\n" +
                        $"📱 No. WhatsApp: {strWhatsApp_PhoneNumber}\n" +
                        $"🏷 Tele Chat ID: {varTeleChatID}\n\n" +
                        $"Sila pilih tindakan:",
                        ParseMode.Markdown,
                        replyMarkup: inlineKeyboard);
                }
            }
            else
            {
                await _TelegramBotClient.SendTextMessageAsync(varTeleChatID,
                    "❌ Format salah. Buat macam ni:" +
                    "\n\n/daftar Ali Rudin 60123456789");
            }
        }

        private static async void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            var callbackData = e.CallbackQuery.Data;

            if (callbackData.StartsWith("approve:") || callbackData.StartsWith("decline:"))
            {
                var parts = callbackData.Split(':');
                var action = parts[0]; // approve / decline
                var varTeleChatID = long.Parse(parts[1]);

                if (action == "approve")
                {
                    await _FirebaseHelper.UpdateUserStatusAsync(varTeleChatID, "Approved");

                    // Inform New Subsriber
                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "✅ ID anda telah di approved." +
                        $"\n\nWelkem to {strAppName}! 🎉");

                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "📢 𝗣𝗘𝗡𝗧𝗜𝗡𝗚!" +
                        $"\nSila pastikan \"𝗨𝗻𝗺𝘂𝘁𝗲 𝗡𝗼𝘁𝗶𝗳𝗶𝗰𝗮𝘁𝗶𝗼𝗻𝘀\" Bot ni untuk berfungsi sepenuhnya." +
                        $"\n\n👉 Tekan nama Bot kat atas > tekan butang \"𝗨𝗻𝗺𝘂𝘁𝗲\" 🔕.");

                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "📌 𝗦𝗘𝗡𝗔𝗥𝗔𝗜 𝗖𝗢𝗠𝗠𝗔𝗡𝗗 𝗕𝗢𝗧" +
                        "\n\n📝 /daftar: Daftar pengguna baru" +
                        "\n📋 /list: Tengok list Birthday Data");

                    // Inform Admin Bot
                    Console.WriteLine($"[{dtMalaysia}]: Admin Bot approved Subscriber baru.");
                    await _TelegramBotClient.SendTextMessageAsync(
                        chatId: longBot_Owner_ID,
                        text: $"<b>[{dtMalaysia}]:</b> Pendaftaran Subscriber baru di approved. ✅",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );

                    // Remove inline keyboard
                    await _TelegramBotClient.EditMessageReplyMarkupAsync(
                        chatId: longBot_Owner_ID,
                        messageId: e.CallbackQuery.Message.MessageId,
                        replyMarkup: null
                    );
                }
                else if (action == "decline")
                {
                    await _FirebaseHelper.UpdateUserStatusAsync(varTeleChatID, "Declined");

                    // Inform New Subscribers
                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "❌ ID anda telah di rejected. Hubungi admin untuk maklumat lanjut.");

                    // Inform Admin Bot
                    Console.WriteLine($"[{dtMalaysia}]: Admin Bot rejected Subscriber baru.");
                    await _TelegramBotClient.SendTextMessageAsync(
                        chatId: longBot_Owner_ID,
                        text: $"<b>[{dtMalaysia}]:</b> Pendaftaran Subscriber baru di rejected. ❌",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );

                    // Remove inline keyboard
                    await _TelegramBotClient.EditMessageReplyMarkupAsync(
                        chatId: longBot_Owner_ID,
                        messageId: e.CallbackQuery.Message.MessageId,
                        replyMarkup: null
                    );
                }
            }
        }

        // Method AddBirthdays when command /tambah is call
        private static async Task AddBirthdays(object sender, MessageEventArgs e)
        {
            var varTeleChatID = e.Message.Chat.Id;
            var varText = e.Message.Text?.Trim();
            var parts = varText.Split(' ');

            if (parts.Length >= 3)
            {
                string strDate = parts[^1];
                string strName = string.Join(" ", parts.Skip(1).Take(parts.Length - 2));

                if (!DateTime.TryParseExact(strDate, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthdayDate))
                {
                    await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "❌ Salah format." +
                        "\n\nGuna /tambah [Nama Nama] dd/MM");
                    return;
                }

                birthdayDate = birthdayDate.Date;

                try
                {
                    await _FirebaseHelper.AddBirthdayDataAsync(strName, birthdayDate);

                    Console.WriteLine($"[{dtMalaysia}]: Birthday Data baru berjaya ditambah - Nama: {strName}, Tarikh: {birthdayDate:dd/MM}.");
                    await _TelegramBotClient.SendTextMessageAsync(
                        chatId: longBot_Owner_ID,
                        text: $"<b>[{dtMalaysia}]:</b> Birthday Data baru berjaya ditambah. ✅" +
                        $"\n\n👤 Nama: {strName}" +
                        $"\n📅 Tarikh: {birthdayDate:dd/MM}",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );

                    await ManualCheckBirthdays();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{dtMalaysia}]: Birthday Data baru gagal ditambah - Nama: {strName}, Tarikh: {birthdayDate:dd/MM}: {ex.Message}");
                    await _TelegramBotClient.SendTextMessageAsync(
                        chatId: longBot_Owner_ID,
                        text: $"<b>[{dtMalaysia}]:</b> Birthday Data baru gagal ditambah. ❌" +
                        $"\n\n👤 Nama: {strName}" +
                        $"\n📅 Tarikh: {birthdayDate:dd/MM}" +
                        $"\n\n[ERROR DESC]: {ex.Message}",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );
                }
            }
            else
            {
                await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "❌ Salah format." +
                        "\n\nGuna /tambah [Nama Nama] dd/MM");
            }
        }

        // Method ListBirthdays when command /list is call
        private static async Task ListBirthdays(object sender, MessageEventArgs e)
        {
            var varTeleChatID = e.Message.Chat.Id;

            var entries = await _FirebaseHelper.GetAllBirthdayDataAsync();

            if (entries.Count == 0)
            {
                await _TelegramBotClient.SendTextMessageAsync(varTeleChatID, "🙅🏻‍♂️ Takde birthday list lagi. Admin belum update.");
            }
            else
            {
                // Grouping Birthday Data by Month (1 - 12) and Day (1 - 31)
                var grouped = entries
                    .GroupBy(e => e.dtBirthdayDate.Month)
                    .OrderBy(g => g.Key);

                string result = "";

                foreach (var group in grouped)
                {
                    string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Key).ToUpper();
                    result += $"📅 <b>{monthName}</b>\n";

                    foreach (var entry in group.OrderBy(e => e.dtBirthdayDate.Day))
                    {
                        result += $"   🎂 {entry.strName} ({entry.dtBirthdayDate:dd})\n";
                    }

                    result += "\n";
                }

                await _TelegramBotClient.SendTextMessageAsync(
                    chatId: varTeleChatID,
                    text: result.Trim(),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                );
            }
        }

        // Method ManualCheckBirthdays when command /check is call
        private static async Task ManualCheckBirthdays()
        {
            Console.WriteLine($"[{dtMalaysia}]: Bot buat semakan hari kelahiran...");
            await _TelegramBotClient.SendTextMessageAsync(
                chatId: longBot_Owner_ID,
                text: $"<b>[{dtMalaysia}]:</b> Bot buat semakan hari kelahiran...",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );

            var today = DateTime.Today;
            var birthdays = await _FirebaseHelper.GetAllBirthdayDataAsync();

            var matches = birthdays
                .Where(b => b.dtBirthdayDate.Month == today.Month &&
                            b.dtBirthdayDate.Day == today.Day)
                .ToList();

            if (!matches.Any())
            {
                Console.WriteLine($"[{dtMalaysia}]: Takde birthday sesiapa harini.");
                await _TelegramBotClient.SendTextMessageAsync(
                    chatId: longBot_Owner_ID,
                    text: $"<b>[{dtMalaysia}]:</b> 🙅🏻‍♂️ Takde birthday sesiapa harini.",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                );
            }
            else
            {
                string list = string.Join("\n", matches.Select(b =>
                    $"🎂 {b.strName} ({b.dtBirthdayDate:dd/MM})"));

                await _TelegramBotClient.SendTextMessageAsync(
                    chatId: longBot_Owner_ID,
                    text: $"🥳 Harini ada birthday <b>{matches.Count}</b> orang:" +
                        $"\n\n{list}",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );

                // Broadcast to all subscribers
                await SendDailyBirthdays();
            }
        }
    }
}