namespace VoxCore.Plugins.Contracts.Services;

public interface ICurrentDialog
{
    void Say(string msg);

    Task<string> AskAsync(string req);
}