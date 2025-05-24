using System.Windows;

namespace TaskManager
{
    public partial class AddUserWindow : Window
    {
        public string UserName { get; private set; } = "";

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            UserName = UserNameBox.Text.Trim();
            if (string.IsNullOrEmpty(UserName))
            {
                MessageBox.Show("Имя пользователя не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
} 