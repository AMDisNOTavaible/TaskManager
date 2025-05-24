using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TaskManager.Data;
using TaskManager.Models;
using System.Collections.Generic;

namespace TaskManager
{
    public partial class MainWindow : Window
    {
        private readonly TaskRepository _repository;

        public MainWindow()
        {
            InitializeComponent();
            var context = new TaskManagerContext();
            _repository = new TaskRepository(context);
            LoadUsers();
            LoadTasks();
            UpdateStatistics();
        }

        private void LoadUsers()
        {
            try
            {
                var users = _repository.GetUsers();
                var allItem = new User { Id = -1, Name = "Все" };
                var userList = new List<User> { allItem };
                userList.AddRange(users);

                UserFilter.ItemsSource = userList;
                UserFilter.DisplayMemberPath = "Name";
                UserFilter.SelectedValuePath = "Id";
                UserFilter.SelectedIndex = 0;

                // Для удаления — только реальные пользователи
                DeleteUserCombo.ItemsSource = users;
                DeleteUserCombo.DisplayMemberPath = "Name";
                DeleteUserCombo.SelectedValuePath = "Id";
                if (users.Any())
                    DeleteUserCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTasks()
        {
            try
            {
                var search = SearchBox.Text;
                var status = (StatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
                var userId = UserFilter.SelectedValue as int?;

                // Если выбран "Все", userId будет -1
                if (userId == -1)
                    userId = null;

                var tasks = _repository.GetTasks(search, status, userId);
                TasksGrid.ItemsSource = tasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                var stats = _repository.GetTaskStatistics();
                int toDo = stats.ContainsKey("To Do") ? stats["To Do"] : 0;
                int inProgress = stats.ContainsKey("In Progress") ? stats["In Progress"] : 0;
                int done = stats.ContainsKey("Done") ? stats["Done"] : 0;

                StatisticsText.Text = $"Статистика: To Do: {toDo}, " +
                                      $"In Progress: {inProgress}, " +
                                      $"Done: {done}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadTasks();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTasks();
        }

        private void UserFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTasks();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var taskForm = new TaskForm(_repository);
            taskForm.ShowDialog();
            LoadTasks();
            UpdateStatistics();
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (TasksGrid.SelectedItem is TaskItem selectedTask)
            {
                var taskForm = new TaskForm(_repository, selectedTask);
                taskForm.ShowDialog();
                LoadTasks();
                UpdateStatistics();
            }
            else
            {
                MessageBox.Show("Выберите задачу для редактирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TasksGrid.SelectedItem is TaskItem selectedTask)
            {
                var result = MessageBox.Show($"Удалить задачу '{selectedTask.Title}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _repository.DeleteTask(selectedTask.Id);
                        LoadTasks();
                        UpdateStatistics();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
            UpdateStatistics();
        }

        private void TasksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработчик для активации кнопок редактирования/удаления
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow { Owner = this };
            if (addUserWindow.ShowDialog() == true)
            {
                string newName = addUserWindow.UserName.Trim();
                // Проверка на уникальность имени
                var users = _repository.GetUsers();
                if (users.Any(u => u.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Пользователь с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var user = new User { Name = newName };
                _repository.AddUser(user);
                LoadUsers();
            }
        }

        private void AddNewUser_Click(object sender, RoutedEventArgs e)
        {
            var newUser = new User { Name = "Новый пользователь" };
            _repository.AddUser(newUser);
            LoadUsers();
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteUserCombo.SelectedValue is int userId)
            {
                var users = _repository.GetUsers();
                var user = users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка: есть ли у пользователя задачи
                var tasks = _repository.GetTasks(null, null, userId);
                if (tasks.Any())
                {
                    MessageBox.Show("Нельзя удалить пользователя, у которого есть задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Удалить пользователя '{user.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Удаляем пользователя через контекст напрямую, чтобы избежать проблем с отслеживанием сущностей
                        using (var context = new TaskManager.Data.TaskManagerContext())
                        {
                            var userToDelete = context.Users.FirstOrDefault(u => u.Id == userId);
                            if (userToDelete != null)
                            {
                                context.Users.Remove(userToDelete);
                                context.SaveChanges();
                            }
                        }
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении пользователя: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}