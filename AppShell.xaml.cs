namespace PROJECT
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("moodEntry", typeof(Pages.MoodEntryPage));
            Routing.RegisterRoute("login", typeof(Pages.LoginPage));
            Routing.RegisterRoute("register", typeof(Pages.RegisterPage));
            // Register Edit Profile Route
            Routing.RegisterRoute("editProfile", typeof(Pages.EditProfilePage));
            Routing.RegisterRoute("policies", typeof(Pages.AppPoliciesPage));
        }
    }
}