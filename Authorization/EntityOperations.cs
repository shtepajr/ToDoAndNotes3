using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace ToDoAndNotes3.Authorization
{
    public static class EntityOperations
    {
        public static OperationAuthorizationRequirement FullAccess = new OperationAuthorizationRequirement()
        {
            Name = Constants.FullAccessOperationName
        };
    }
    public class Constants
    {
        public static readonly string FullAccessOperationName = "FullAccess";
    }
}
