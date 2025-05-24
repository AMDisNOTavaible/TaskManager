using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using System;

namespace TaskManager.Data
{
    public class TaskManagerContext : DbContext
    {
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=taskmanager.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId);

            // Инициализация тестовых данных
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Алексей" },
                new User { Id = 2, Name = "Мария" }
            );

            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem { Id = 1, Title = "Разработать UI", Description = "Создать интерфейс для приложения", Status = "To Do", CreatedAt = DateTime.Now, UserId = 1 },
                new TaskItem { Id = 2, Title = "Тестирование", Description = "Протестировать функционал", Status = "In Progress", CreatedAt = DateTime.Now, UserId = 2 }
            );
        }
    }
}