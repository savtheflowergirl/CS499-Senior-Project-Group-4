using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Data;

public class HorizonLunarisDbContext : DbContext
{
    public HorizonLunarisDbContext(DbContextOptions<HorizonLunarisDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- User ----------
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();

            // Store the enum as readable text in the database
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);

            // Demo users so the prototype works without a login feature
            e.HasData(
                new User { Id = 1, Name = "Demo Project Manager", Email = "pm@example.com", Role = UserRole.ProjectManager },
                new User { Id = 2, Name = "Demo Team Member 1", Email = "member1@example.com", Role = UserRole.TeamMember },
                new User { Id = 3, Name = "Demo Team Member 2", Email = "member2@example.com", Role = UserRole.TeamMember }
            );
        });

        // ---------- Project ----------
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(150);
            e.Property(p => p.Description).HasMaxLength(2000);

            e.HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- TaskItem ----------
        modelBuilder.Entity<TaskItem>(e =>
        {
            e.ToTable("Tasks");
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).IsRequired().HasMaxLength(200);
            e.Property(t => t.Description).HasMaxLength(4000);
            e.Property(t => t.Status).HasConversion<string>().HasMaxLength(30);

            e.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Sub-tasks: self-referencing. Restrict so deleting a parent
            // does not silently remove its sub-tasks.
            e.HasOne(t => t.ParentTask)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Owner is optional; deleting a user un-assigns their tasks.
            e.HasOne(t => t.Owner)
                .WithMany(u => u.OwnedTasks)
                .HasForeignKey(t => t.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(t => new { t.ProjectId, t.Status });
        });

        // ---------- TaskDependency (many-to-many join) ----------
        modelBuilder.Entity<TaskDependency>(e =>
        {
            e.HasKey(d => new { d.DependentTaskId, d.PrerequisiteTaskId });

            e.HasOne(d => d.DependentTask)
                .WithMany(t => t.Prerequisites)
                .HasForeignKey(d => d.DependentTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict avoids two cascade paths to the same table
            // (an error on SQL Server, and good hygiene everywhere).
            e.HasOne(d => d.PrerequisiteTask)
                .WithMany(t => t.Dependents)
                .HasForeignKey(d => d.PrerequisiteTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fast lookup for the unlock step: "who depends on this task?"
            e.HasIndex(d => d.PrerequisiteTaskId);

            // A task may not depend on itself. Longer cycles (A -> B -> A)
            // cannot be expressed here and must be rejected in the service layer.
            e.ToTable(t => t.HasCheckConstraint(
                "CK_TaskDependency_NoSelfReference",
                "\"DependentTaskId\" <> \"PrerequisiteTaskId\""));
        });

        // ---------- Comment ----------
        modelBuilder.Entity<Comment>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Text).IsRequired().HasMaxLength(2000);

            e.HasOne(c => c.TaskItem)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
