namespace Soteo.Gameplay.Interfaces;

public interface ISceneLoader
{
    void LoadShard();
    T InstanceScene<T>(string path, Guid shardId, Func<Type, IServiceProvider, Node> nodeFactory) where T : Node;
}