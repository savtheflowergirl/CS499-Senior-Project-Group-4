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
           var requestedAssigneeIds = (request.AssigneeIds ?? [])
               .Concat(request.OwnerId.HasValue ? [request.OwnerId.Value] : [])
               .Distinct()
               .ToArray();
           var assignees = await db.Users
               .Where(user => requestedAssigneeIds.Contains(user.Id))
               .ToListAsync(ct);
           if (assignees.Count != requestedAssigneeIds.Length)
           {
               throw new NotFoundException("One or more assigned users were not found.");
           }

           var task = new TaskItem
           {
               Title = request.Title.Trim(),
               Description = request.Description,
               DueDateUtc = request.DueDateUtc,
               ProjectId = projectId,
               ParentTaskId = request.ParentTaskId,
               Assignees = assignees,
               CreatedAtUtc = DateTime.UtcNow
           };
           db.Tasks.Add(task);
           await db.SaveChangesAsync(ct);

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
               .Include(t => t.Assignees)
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
                .Include(t => t.Assignees)
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
        public Task<TaskDto> AssignOwnerAsync(int taskId, AssignOwnerRequest request, CancellationToken ct = default) =>
            AssignAssigneesAsync(taskId, new AssignAssigneesRequest([request.OwnerId]), ct);

        public async Task<TaskDto> AssignAssigneesAsync(
            int taskId, AssignAssigneesRequest request, CancellationToken ct = default)
        {
            await currentUser.GetRequiredProjectManagerAsync(ct);

            var task = await db.Tasks
                .Include(t => t.Assignees)
                .Include(t => t.Prerequisites)
                .FirstOrDefaultAsync(t => t.Id == taskId, ct);
            if (task is null)
            {
                throw new NotFoundException("Task not found.");
            }

            var requestedIds = request.AssigneeIds.Distinct().ToArray();
            var assignees = await db.Users
                .Where(user => requestedIds.Contains(user.Id))
                .ToListAsync(ct);
            if (assignees.Count != requestedIds.Length)
            {
                throw new NotFoundException("One or more assigned users were not found.");
            }

            task.Assignees.Clear();
            foreach (var assignee in assignees)
            {
                task.Assignees.Add(assignee);
            }

            await db.SaveChangesAsync(ct);
            return task.ToDto();
        }
}
