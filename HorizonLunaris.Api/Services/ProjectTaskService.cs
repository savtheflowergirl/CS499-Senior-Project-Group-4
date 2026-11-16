using HorizonLunaris.Contracts;
using HorizonLunaris.Data;

namespace HorizonLunaris.Api.Services;
using HorizonLunaris.Domain.Entities;
using Microsoft.EntityFrameworkCore;

// FEATURE 1 OWNER: replace each NotImplementedException with a real implementation.
// Suggested rules: only a Project Manager creates projects/tasks and assigns owners;
// a parent task must belong to the same project; the owner must exist.
public class ProjectTaskService(HorizonLunarisDbContext db, ICurrentUser currentUser) : IProjectTaskService
{
   public async Task<ProjectDto> CreateProjectAsync(
       CreateProjectRequest request,
       CancellationToken ct = default)
   {
       // make sure this feature is only availiable to PM; SM
       var user = await currentUser.GetRequiredProjectManagerAsync(ct);
       // check project name; SM
       if (string.IsNullOrWhiteSpace(request.Name))
       {
           throw new BusinessRuleException("Project name cannot be null.");
       }
       // create project; SM
       var project = new Project
       {
           Name = request.Name.Trim(),
           Description = request.Description,
           CreatedById = user.Id,
           CreatedAtUtc = DateTime.UtcNow
       };
       // add project to database list; SM
       db.Projects.Add(project);
       await db.SaveChangesAsync(ct);
       return project.ToDto();
   }
      public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default)
      {
          // pull all projects listed in database;SM
          var projects = await db.Projects
              .OrderBy(p => p.Id)
              .ToListAsync(ct);
          // return to homepage list; SM
          return projects.Select(p => p.ToDto()).ToList();
      }
       public async Task<ProjectDto> GetProjectAsync(int projectId, CancellationToken ct = default)
       {
           // find project by its name id;SM
           var project = await db.Projects.FindAsync(new object[] { projectId }, ct);
           // make sure project exists in database;SM
           if (project == null)
           {
               throw new NotFoundException("Project not found.");
           }
           // if it exists, return project;SM
           return project.ToDto();
       }
       public async Task<TaskDto> CreateTaskAsync(int projectId, CreateTaskRequest request, CancellationToken ct = default)
       {
           // only PM editorr can make tasks; SM
           await currentUser.GetRequiredProjectManagerAsync(ct);
           // project is in database;SM
           var project = await db.Projects.FindAsync(new object[] { projectId }, ct);
           if (project == null)
           {
               throw new NotFoundException("Project not found.");
           }
           // title is valid;SM
           if (string.IsNullOrWhiteSpace(request.Title))
           {
               throw new BusinessRuleException("Task title cannot be null.");
           }
           // must have valid parent task value;SM
           if (request.ParentTaskId.HasValue)
           {
               var parentTask = await db.Tasks.FindAsync(new object[] { request.ParentTaskId.Value }, ct);
               if (parentTask == null || parentTask.ProjectId != projectId)
               {
                   throw new BusinessRuleException("Parent task must be initiated to this project.");
               }
           }
           // assigned project user is correct;SM
           if (request.OwnerId.HasValue)
           {
               var owner = await db.Users.FindAsync(new object[] { request.OwnerId.Value }, ct);

               if (owner == null)
               {
                   throw new NotFoundException("Assigned user not found in Database.");
               }
           }
           // create and assign task; SM
           var task = new TaskItem
           {
               Title = request.Title.Trim(),
               Description = request.Description,
               DueDateUtc = request.DueDateUtc,
               ProjectId = projectId,
               ParentTaskId = request.ParentTaskId,
               OwnerId = request.OwnerId,
               CreatedAtUtc = DateTime.UtcNow
           };
           db.Tasks.Add(task);
           await db.SaveChangesAsync(ct);
           await db.Entry(task).Reference(t => t.Owner).LoadAsync(ct);

           return task.ToDto();
       }
       public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(int projectId, CancellationToken ct = default)
       {
           // checj project exists in DB; SM
           var project = await db.Projects.FindAsync(new object[] { projectId }, ct);
           if (project == null)
           {
               throw new NotFoundException("Project not found.");
           }
           // find all tasks within project; SM
           var tasks = await db.Tasks
               .Where(t => t.ProjectId == projectId)
               .Include(t => t.Owner)
               .Include(t => t.Prerequisites)
               .OrderBy(t => t.Id)
               .ToListAsync(ct);
           // return task list; SM
           return tasks.Select(t => t.ToDto()).ToList();
       }
        public async Task<TaskDto> GetTaskAsync(int taskId, CancellationToken ct = default)
        {
            // find task by name
            var task = await db.Tasks
                .Include(t => t.Owner)
                .Include(t => t.Prerequisites)
                .FirstOrDefaultAsync(t => t.Id == taskId, ct);
            // Check if the task exists
            if (task == null)
            {
                throw new NotFoundException("Task not found.");
            }
            // if found, return requested task; SM
            return task.ToDto();
        }
        public async Task<TaskDto> AssignOwnerAsync(int taskId, AssignOwnerRequest request, CancellationToken ct = default)
        {
            // only access to PM;SM
            await currentUser.GetRequiredProjectManagerAsync(ct);
            // find task by name;SM
            var task = await db.Tasks
                .Include(t => t.Prerequisites)
                .FirstOrDefaultAsync(t => t.Id == taskId, ct);
            if (task == null)
            {
                throw new NotFoundException("Task not found.");
            }
            // find user to assigne; SM
            var owner = await db.Users.FindAsync(new object[] { request.OwnerId }, ct);
            if (owner == null)
            {
                throw new NotFoundException("User not found.");
            }
            // assign user if found
            task.OwnerId = owner.Id;
            task.Owner = owner;
            // save change to DB;SM
            await db.SaveChangesAsync(ct);
            return task.ToDto();
        }
        public async Task<TaskDto> UpdateTaskAsync(
            int taskId,
            UpdateTaskRequest request,
            CancellationToken ct = default)
        {
            // Only Project Managers can edit tasks; SM
            await currentUser.GetRequiredProjectManagerAsync(ct);

            // Find the requested task; SM
            var task = await db.Tasks
                .Include(t => t.Owner)
                .Include(t => t.Prerequisites)
                .FirstOrDefaultAsync(t => t.Id == taskId, ct);

            if (task == null)
            {
                throw new NotFoundException("Task not found.");
            }

            // Validate the updated title; SM
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new BusinessRuleException("Task title cannot be empty.");
            }

            if (request.Title.Trim().Length > 200)
            {
                throw new BusinessRuleException("Task title is too long.");
            }

            if (request.Description?.Length > 4000)
            {
                throw new BusinessRuleException("Task description is too long.");
            }

            // Update task information; SM
            task.Title = request.Title.Trim();
            task.Description = request.Description;
            task.DueDateUtc = request.DueDateUtc;

            // Save changes to database; SM
            await db.SaveChangesAsync(ct);

            return task.ToDto();
        }
        // Delete a task with chain deletion options; SM
        public async Task DeleteTaskAsync(
            int taskId,
            DeleteTaskRequest request,
            CancellationToken ct = default)
        {
            // Only Project Managers can delete tasks; SM
            await currentUser.GetRequiredProjectManagerAsync(ct);

            // Find the selected task; SM
            var task = await db.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId, ct);

            if (task == null)
            {
                throw new NotFoundException("Task not found.");
            }

            // Validate the deletion option; SM
            if (!Enum.IsDefined(request.Mode))
            {
                throw new BusinessRuleException("Invalid deletion option.");
            }

            // Load task hierarchy for this project; SM
            var projectTasks = await db.Tasks
                .Where(t => t.ProjectId == task.ProjectId)
                .ToListAsync(ct);

            var children = projectTasks
                .Where(t => t.ParentTaskId == taskId)
                .ToList();

            // TaskOnly does not remove subtasks; SM
            if (request.Mode == TaskDeletionMode.TaskOnly &&
                children.Count > 0)
            {
                throw new BusinessRuleException(
                    "This task has subtasks. Choose a chain deletion option.");
            }

            var tasksToDelete = new List<TaskItem> { task };

            if (request.Mode == TaskDeletionMode.DeleteChildren)
            {
                // Collect all descendants and their depth; SM
                var queue = new Queue<(int Id, int Depth)>();
                var visited = new HashSet<int> { taskId };

                queue.Enqueue((taskId, 0));

                while (queue.Count > 0)
                {
                    var (parentId, depth) = queue.Dequeue();

                    foreach (var child in projectTasks
                        .Where(t => t.ParentTaskId == parentId))
                    {
                        if (visited.Add(child.Id))
                        {
                            tasksToDelete.Add(child);
                            queue.Enqueue((child.Id, depth + 1));
                        }
                    }
                }
            }

            var deletedIds = tasksToDelete
                .Select(t => t.Id)
                .ToHashSet();

            // Refuse deletion when dependency links involve surviving tasks; SM
            var hasExternalDependencies =
                await db.TaskDependencies.AnyAsync(
                    d =>
                        (deletedIds.Contains(d.DependentTaskId) &&
                         !deletedIds.Contains(d.PrerequisiteTaskId)) ||
                        (deletedIds.Contains(d.PrerequisiteTaskId) &&
                         !deletedIds.Contains(d.DependentTaskId)),
                    ct);

            if (hasExternalDependencies)
            {
                throw new BusinessRuleException(
                    "Cannot delete tasks with dependencies outside the deletion group.");
            }

            // Keep all database changes atomic; SM
            await using var transaction =
                await db.Database.BeginTransactionAsync(ct);

            // Remove dependencies entirely within the deletion group; SM
            var internalDependencies = await db.TaskDependencies
                .Where(d =>
                    deletedIds.Contains(d.DependentTaskId) &&
                    deletedIds.Contains(d.PrerequisiteTaskId))
                .ToListAsync(ct);

            db.TaskDependencies.RemoveRange(internalDependencies);
            await db.SaveChangesAsync(ct);

            if (request.Mode == TaskDeletionMode.KeepChildren)
            {
                // Reassign direct children to the deleted task's parent; SM
                foreach (var child in children)
                {
                    child.ParentTaskId = task.ParentTaskId;
                }

                await db.SaveChangesAsync(ct);
            }

            if (request.Mode == TaskDeletionMode.DeleteChildren)
            {
                // Delete leaf tasks first to respect parent-child restrictions; SM
                var remaining = tasksToDelete.ToDictionary(t => t.Id);

                while (remaining.Count > 0)
                {
                    var leafTasks = remaining.Values
                        .Where(t => !remaining.Values.Any(
                            child => child.ParentTaskId == t.Id))
                        .ToList();

                    if (leafTasks.Count == 0)
                    {
                        throw new BusinessRuleException(
                            "Invalid task hierarchy detected.");
                    }

                    db.Tasks.RemoveRange(leafTasks);
                    await db.SaveChangesAsync(ct);

                    foreach (var leaf in leafTasks)
                    {
                        remaining.Remove(leaf.Id);
                    }
                }
            }
            else
            {
                db.Tasks.Remove(task);
                await db.SaveChangesAsync(ct);
            }

            await transaction.CommitAsync(ct);
        }
}
