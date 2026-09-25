using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //1. preventing duplicate email registration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            // 2. Prevent multiple cascade paths to TaskItem
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
            // 3. Prevent deleting an owner from cascading and deleting projects unexpectedly
            modelBuilder.Entity<Project>()
                .HasOne(t => t.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(t => t.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
            // 4. Configure junction table name for Tasks <-> Tags
            modelBuilder.Entity<TaskItem>()
                .HasMany(t => t.Tags)
                .WithMany(t => t.Tasks)
                .UsingEntity(j => j.ToTable("TaskItemTags"));
        }
    }
}
