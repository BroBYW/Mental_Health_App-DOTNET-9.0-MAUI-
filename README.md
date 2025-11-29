# MindTrack - Mental Health Companion App

**MindTrack** is a cross-platform mobile application built with **.NET 9 MAUI** designed to support mental well-being. It allows users to track their daily moods, maintain a personal journal with photo attachments, find nearby mental health clinics using geolocation, and chat with an AI-powered empathetic companion.

## 📱 Features

* **Mood Tracking:** Log daily moods (Rad, Good, Meh, Bad, Awful) and view trends on a dashboard chart.
* **Journaling:** Write daily entries and attach photos. Data is synced between a local SQLite database and the Cloud.
* **AI Chat Companion:** Chat with an empathetic bot powered by OpenAI (GPT-3.5 Turbo) for support and conversation.
* **Find Help:** Locate nearby mental health clinics and specialists using Google Maps integration.
* **Cloud Sync:** Secure authentication and data synchronization using Firebase (Auth, Realtime DB, and Storage).
* **Daily Inspirations:** View rotating motivational quotes on the dashboard.

## 🛠 Tech Stack

* **Framework:** .NET 9.0 MAUI (Multi-platform App UI)
* **Language:** C#
* **Architecture:** MVVM (Model-View-ViewModel) using `CommunityToolkit.Mvvm`
* **Database:**
    * Local: `sqlite-net-pcl`
    * Cloud: Firebase Realtime Database
* **Authentication:** Firebase Authentication
* **Storage:** Firebase Storage (for profile and journal images)
* **AI:** OpenAI API
* **Maps:** Microsoft.Maui.Controls.Maps (Google Maps)

## 🚀 Getting Started

### Prerequisites

* [Visual Studio 2022](https://visualstudio.microsoft.com/) (v17.12 or later) with the **.NET Multi-platform App UI development** workload installed.
* .NET 9.0 SDK.
* An Android Emulator or physical device for testing.

### ⚙️ Configuration & API Keys

For security reasons, API keys are **not** committed to the repository. You must create a `Secrets.cs` file to run the application.

#### 1. Create the Secrets File
1.  Navigate to the project root folder (where `MauiProgram.cs` is located).
2.  Create a new file named **`Secrets.cs`**.
3.  Copy and paste the following code into the file:

```csharp
namespace PROJECT
{
    public static class Secrets
    {
        // 1. Firebase Web API Key (Project Settings -> General -> Web API Key)
        public const string FirebaseApiKey = "REPLACE_WITH_YOUR_FIREBASE_API_KEY";

        // 2. Google Maps API Key (Enabled for Maps SDK for Android/iOS)
        public const string GoogleMapsApiKey = "REPLACE_WITH_YOUR_GOOGLE_MAPS_KEY";

        // 3. OpenAI API Key ([https://platform.openai.com/api-keys](https://platform.openai.com/api-keys))
        public const string OpenAiApiKey = "REPLACE_WITH_YOUR_OPENAI_SK_KEY";
    }
}