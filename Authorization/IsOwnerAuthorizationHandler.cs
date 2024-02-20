using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace ToDoAndNotes3.Authorization
{
    public class IsOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, object>
    {
        private readonly UserManager<Models.User> _userManager;
        public IsOwnerAuthorizationHandler(UserManager<Models.User> userManager)
        {
            _userManager = userManager;
        }
        protected override System.Threading.Tasks.Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, object resource)
        {
            if (context.User == null || resource == null) { return Task.CompletedTask; }
            if (requirement.Name != Constants.FullAccessOperationName) { return Task.CompletedTask; }

            if (resource == null) { return Task.CompletedTask; }

            string? ownerId = null;
            Func<Models.Project, string?> getProjectOwnerIdFunc = project => project?.UserId;
            Func<Models.Label, string?> getLabelOwnerIdFunc = label => label?.UserId;
            Func<Models.Task, string?> getTaskOwnerIdFunc = task => task?.Project?.UserId;
            Func<Models.Note, string?> getNoteOwnerIdFunc = note => note?.Project?.UserId;

            switch (resource)
            {
                case Models.Project p:
                    ownerId = getProjectOwnerIdFunc(p);
                    break;
                case Models.Label l:
                    ownerId = getLabelOwnerIdFunc(l);
                    break;
                case Models.Task t:
                    ownerId = getTaskOwnerIdFunc(t);
                    break;
                case Models.Note n:
                    ownerId = getNoteOwnerIdFunc(n);
                    break;
            }

            if (ownerId == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
                Console.WriteLine("Success authorization: " + DateTime.Now);
            }
            return Task.CompletedTask;
        }
    }
}
