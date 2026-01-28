namespace VoxCore.Runtime.Contracts;

public interface IInputService
{
    public sealed class Args : EventArgs
    {
        public string Source { get; }
        public string? Message { get; }

        public Args(string source, string message)
        {
            Source = string.IsNullOrEmpty(source) 
                ? "default" 
                : message;

            Message = string.IsNullOrEmpty(message) 
                ? null 
                : message;
        }
    }

    event EventHandler<Args> InputReceived;
}