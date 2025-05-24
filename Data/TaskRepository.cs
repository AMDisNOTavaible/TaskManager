using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Models;

namespace TaskManager.Data
{
    public class TaskRepository
    {
        private readonly TaskManagerContext _context;

        public TaskRepository(TaskManagerContext context)
        {
            _context = context;
        }

        public List<TaskItem> GetTasks(string? search = null, string? statusFilter = null, int? userId = null)
        {
            try
            {
                var query = _context.Tasks.Include(t => t.User).AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t => (t.Title ?? string.Empty).Contains(search) || 
                                            (t.Description ?? string.Empty).Contains(search));
                }

                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                {
                    query = query.Where(t => t.Status == statusFilter);
                }

                if (userId.HasValue)
                {
                    query = query.Where(t => t.UserId == userId);
                }

                return query.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении задач: " + ex.Message);
            }
        }

        public List<User> GetUsers()
        {
            try
            {
                return _context.Users.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении пользователей: " + ex.Message);
            }
        }

        public void AddTask(TaskItem task)
        {
            try
            {
                if (string.IsNullOrEmpty(task.Title))
                    throw new ArgumentException("Название задачи не может быть пустым.");
                if (!_context.Users.Any(u => u.Id == task.UserId))
                    throw new ArgumentException("Указанный пользователь не существует.");

                task.CreatedAt = DateTime.Now;
                _context.Tasks.Add(task);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при добавлении задачи: " + ex.Message);
            }
        }

        public void UpdateTask(TaskItem task)
        {
            try
            {
                if (string.IsNullOrEmpty(task.Title))
                    throw new ArgumentException("Название задачи не может быть пустым.");
                if (!_context.Users.Any(u => u.Id == task.UserId))
                    throw new ArgumentException("Указанный пользователь не существует.");

                _context.Tasks.Update(task);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при обновлении задачи: " + ex.Message);
            }
        }

        public void DeleteTask(int id)
        {
            try
            {
                var task = _context.Tasks.Find(id);
                if (task == null)
                    throw new ArgumentException("Задача не найдена.");

                _context.Tasks.Remove(task);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при удалении задачи: " + ex.Message);
            }
        }

        public Dictionary<string, int> GetTaskStatistics()
        {
            try
            {
                return _context.Tasks
                    .GroupBy(t => t.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении статистики: " + ex.Message);
            }
        }

        public void AddUser(User user)
        {
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при добавлении пользователя: " + ex.Message);
            }
        }
    }
}