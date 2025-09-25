using Firebase.Database;
using Firebase.Database.Query;
using Telegram.Bot.Types;

namespace BirthdayBuzz_Bot_NETCore
{
    public class FirebaseHelper
    {
        private readonly FirebaseClient _client;

        public FirebaseHelper()
        {
            string strFirebase_DB_URL = Environment.GetEnvironmentVariable("Firebase__DB_URL");
            string strFirebase_DB_SecretKey = Environment.GetEnvironmentVariable("Firebase__DB_SecretKey");

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            _client = new FirebaseClient(
                strFirebase_DB_URL,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(strFirebase_DB_SecretKey)
                });
        }

        #region Birthday Data
        public async Task<List<BirthdayEntry>> GetAllBirthdayDataAsync()
        {
            var firebaseData = await _client.Child("BirthdayData").OnceAsync<BirthdayEntry>();
            return firebaseData.Select(x => x.Object).ToList();
        }

        public async Task<string> AddBirthdayDataAsync(string strName, DateTime dtBirthdayDate)
        {
            var entry = new BirthdayEntry
            {
                strName = strName,
                dtBirthdayDate = dtBirthdayDate
            };

            var result = await _client.Child("BirthdayData").PostAsync(entry);
            return result.Key;
        }
        #endregion

        #region Telegram Subcriber Data
        public async Task<List<TelegramSubscriber_Entry>> GetTelegramSubscriberDataAsync()
        {
            var firebaseData = await _client.Child("TelegramSubscriberData").OnceAsync<TelegramSubscriber_Entry>();
            return firebaseData.Select(x => x.Object).ToList();
        }

        // Check Subscriber berdaftar atau tidak
        public async Task<bool> IsSubscribed(long longTeleChatID)
        {
            var all = await GetTelegramSubscriberDataAsync();
            return all.Any(x => x.longTeleChatID == longTeleChatID);
        }

        // Store New Subscriber Data: Tele Chat ID, Nama, No. WhatsApp, Status Subscriber
        public async Task AddTelegramSubscriberDataAsync(long longTeleChatID, string strName, string strWhatsApp_PhoneNumber, string strSubscriberStatus)
        {
            var entry = new TelegramSubscriber_Entry
            {
                longTeleChatID = longTeleChatID,
                strName = strName,
                strWhatsApp_PhoneNumber = strWhatsApp_PhoneNumber,
                strSubscriberStatus = strSubscriberStatus
            };

            await _client
                .Child("TelegramSubscriberData")
                .Child(longTeleChatID.ToString())
                .PutAsync(entry);
        }

        // Get Subscriber Status to Update from Pending to Approved or Declined
        public async Task<string> GetUserStatusAsync(long longTeleChatID)
        {
            try
            {
                var user = await _client
                    .Child("TelegramSubscriberData")
                    .Child(longTeleChatID.ToString())
                    .OnceSingleAsync<TelegramSubscriber_Entry>();

                return user?.strSubscriberStatus;
            }
            catch
            {
                return null; // not found
            }
        }

        // Update Subscriber Status when Admin Bot Approved or Declined
        public async Task UpdateUserStatusAsync(long longTeleChatID, string strSubscriberStatus)
        {
            await _client
                .Child("TelegramSubscriberData")
                .Child(longTeleChatID.ToString())
                .Child("strSubscriberStatus")
                .PutAsync($"\"{strSubscriberStatus}\"");
        }
        #endregion
    }
}