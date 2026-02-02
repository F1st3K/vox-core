namespace VoxCore.Runtime.Contracts;

public interface IInputService
{
    public sealed record InputData(string Message);

    event EventHandler<InputData> InputReceived;
}