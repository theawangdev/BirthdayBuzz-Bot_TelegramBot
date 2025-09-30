![Build](https://img.shields.io/badge/build-passing-brightgreen)
![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/theawangdev/BirthdayBuzz-Bot_TelegramBot)


<h1>🎉 BirthdayBuzz Bot: Telegram Bot 🎂</h1>

A private Telegram Bot built with C# .NET Core, integrate with Firebase as Database and deploy & host on Railway to send daily birthday reminders (with birthday wish) to approved subscribers.

<h2>✨ Features</h2>

🔒 Private access — only admin-approved users can use the Bot.
<br>👤 Admin-controlled new subscriber approval.

📋 Commands:
<br>/daftar → New subscriber need to register before can use the Bot.
<br>/list → View all birthdays data (for approved subscribers).
<br>/tambah → To add new birthday data (Admin).
<br>/check → Trigger Bot to manual check whos birthday today (Admin).

⏰ Daily scheduled birthday notifications with birthday wish.
<br>📡 Firebase integration for storing birthdays & subscribers data.
<br>🚀 Deployment and Hosting on Railway, read code from GitHub (Free Tier).

<h2>🛠 Tech Stack</h2>
• C# .NET Core
<br>• Telegram.Bot API
<br>• Firebase (Realtime Database)
<br>• Railway (Deploy & Host)

<h2>🚀 Getting Started</h2>

1. Clone the repo:
<div style="max-width:720px;font-family:system-ui,Segoe UI,Arial,sans-serif;">
  <div style="border:1px solid #e1e4e8;overflow:hidden;">
      <pre style="font-family:SFMono-Regular,Menlo,Monaco,monospace;white-space:pre-wrap;">
        git clone https://github.com/theawangdev/BirthdayBuzz-Bot_TelegramBot.git
        cd BirthdayBuzz-Bot_TelegramBot
      </pre>
  </div>
</div>

2. Add your Telegram and Firebase credentials as Environment Variables or inside <b>launchSettings.json</b> file:
<div style="max-width:720px;font-family:system-ui,Segoe UI,Arial,sans-serif;">
  <div style="border:1px solid #e1e4e8;overflow:hidden;">
      <pre style="font-family:SFMono-Regular,Menlo,Monaco,monospace;white-space:pre-wrap;">
        Firebase__DB_URL: "Get from: <a href="https://console.firebase.google.com/u/0/">Firebase Console</a>"
        Firebase__DB_SecretKey: "Get from: <a href="https://console.firebase.google.com/u/0/">Firebase Console</a>"
        Telegram__Bot_Token: "Get from: <a href="https://telegram.me/BotFather">BotFather</a>"
        Telegram__Bot_Owner_ID: "Get from: <a href="https://telegram.me/userinfobot">UserInfoBot</a>"
      </pre>
  </div>
</div>

3. Build & run:
<div style="max-width:720px;font-family:system-ui,Segoe UI,Arial,sans-serif;">
  <div style="border:1px solid #e1e4e8;overflow:hidden;">
      <pre style="font-family:SFMono-Regular,Menlo,Monaco,monospace;white-space:pre-wrap;">
        dotnet run
      </pre>
  </div>
</div>

<h2>📦 Deployment and Hosting</h2>
1. Create a private repository in GitHub and push the code.
<br>2. Go to <a href="https://railway.com/">Railway</a> to deploy from GitHub repo (via Dockerfile).
<br>3. Configure the Environment Variables on Railway.
