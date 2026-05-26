namespace Soteo.Shared.Extensions;

public static class PackedSceneExtensions
{
    extension (PackedScene self)
    {
        public void InstanceAndReparentTo(Node newParent) // todo replace this with injecting a node into a plain C# class
        {
            Node instance = self.Instance();
            instance.ReplaceBy(newParent);
            instance.QueueFree();
        }
    }
}
