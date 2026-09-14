using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmartXDashboard
{
    public partial class LoginWindow : Window
    {
        private readonly System.Net.Http.HttpClient _httpClient = new()
        {
            BaseAddress = new System.Uri("https://localhost:5000/") 
        };
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void TabLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginForm.Visibility = Visibility.Visible;
            RegisterForm.Visibility = Visibility.Collapsed;
        }

        private void TabRegister_Click(object sender, RoutedEventArgs e)
        {
            LoginForm.Visibility = Visibility.Collapsed;
            RegisterForm.Visibility = Visibility.Visible;
        }

        private async void LoginSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginData = new { Username = LoginUsernameInput.Text, Password = LoginPasswordInput.Password };

                // Option B manual serialization (or PostAsJsonAsync if package is installed)
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(loginData);
                var content = new System.Net.Http.StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Access Denied: Invalid credentials.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Connection failed: Ensure the API server is running.\n\nDetails: {ex.Message}", "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SignupSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SignupEmailInput.Text) || string.IsNullOrWhiteSpace(SignupPasswordInput.Password))
            {
                MessageBox.Show("Please complete all registration fields.", "Provisioning Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OpenMainWindow();
        }

        private void OpenMainWindow()
        {
            MainWindow dashboard = new MainWindow();
            dashboard.Show();
            this.Close();
        }

        //Password Strenght
        private void SignupPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = SignupPasswordInput.Password;
            int score = EvaluatePasswordStrength(password);

            //Update Window based on strength score
            if (string.IsNullOrEmpty(password))
            {
                StrengthBar.Width = 0;
                StrengthText.Text = "None";
                StrengthText.Foreground = (Brush)new BrushConverter().ConvertFrom("#8A8A93");
            }
            else if (score <= 2)
            {
                StrengthBar.Width = 80;
                StrengthBar.Background = (Brush)new BrushConverter().ConvertFrom("#E50914"); // Red - password is weak
                StrengthText.Text = "Weak";
                StrengthText.Foreground = (Brush)new BrushConverter().ConvertFrom("#E50914");
            }
            else if (score == 3 || score == 4)
            {
                StrengthBar.Width = 180;
                StrengthBar.Background = (Brush)new BrushConverter().ConvertFrom("#FFC700"); // Yellow - medium password
                StrengthText.Text = "Medium";
                StrengthText.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFC700");
            }
            else
            {
                StrengthBar.Width = 270;
                StrengthBar.Background = (Brush)new BrushConverter().ConvertFrom("#34C759"); // Green - password is strong
                StrengthText.Text = "Strong";
                StrengthText.Foreground = (Brush)new BrushConverter().ConvertFrom("#34C759");
            }
        }

        private int EvaluatePasswordStrength(string password)
        {
            int score = 0;

            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (Regex.IsMatch(password, @"[a-z]") && Regex.IsMatch(password, @"[A-Z]")) score++;
            if (Regex.IsMatch(password, @"[0-9]")) score++;
            if (Regex.IsMatch(password, @"[!@#$%^&*(),.? logic:{""}:{}|<>]")) score++;

            return score;
        }
    }
}