using System;
using System.Linq;
using System.Windows;
using TaskManager.Data;
using TaskManager.Models;
using System.Windows.Controls;

namespace TaskManager
{
    public partial class TaskForm : Window
    {
        private readonly TaskRepository _repository;
        private readonly TaskItem _task;

        public TaskForm(TaskRepository repository, TaskItem? task = null)
        {
            InitializeComponent();
            _repository = repository;
            _task = task ?? new TaskItem 
            { 
                Title = string.Empty,
                Status = "To Do",
                CreatedAt = DateTime.Now
            };

            LoadUsers();
            if (task != null)
            {
                TitleBox.Text = task.Title;
                DescriptionBox.Text = task.Description;
                DueDatePicker.SelectedDate = task.DueDate;
                StatusCombo.SelectedItem = StatusCombo.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == task.Status);
                UserCombo.SelectedValue = task.UserId;
            }
            else
            {
                StatusCombo.SelectedIndex = 0;
            }
        }

        private void LoadUsers()
        {
            try
            {
                var users = _repository.GetUsers();
                UserCombo.ItemsSource = users;
                UserCombo.DisplayMemberPath = "Name";
                UserCombo.SelectedValuePath = "Id";
                if (users.Any())
                    UserCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TitleBox.Text))
                {
                    MessageBox.Show("Название задачи не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (UserCombo.SelectedValue == null)
                {
                    MessageBox.Show("Выберите пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _task.Title = TitleBox.Text;
                _task.Description = DescriptionBox.Text;
                _task.DueDate = DueDatePicker.SelectedDate;
                _task.Status = (StatusCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
                _task.UserId = (int)UserCombo.SelectedValue;

                if (_task.Id == 0)
                    _repository.AddTask(_task);
                else
                    _repository.UpdateTask(_task);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}