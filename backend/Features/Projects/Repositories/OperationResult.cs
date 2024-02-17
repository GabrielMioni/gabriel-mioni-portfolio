namespace backend.Features.Projects.Repositories
{
    public class OperationResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }

        public static OperationResult Fail(string message)
        {
            return new OperationResult { Succeeded = false, Message = message };
        }

        public static OperationResult Success(string? message = null)
        {
            var displayMessage = message != null ? message : "Operation succeeded.";
            return new OperationResult { Succeeded = true, Message = displayMessage };
        }
    }
}
