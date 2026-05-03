namespace Soteo.Shared.Extensions;

public static class PacketSceneExtensions
{
    extension (PackedScene self)
    {
        public void InstanceAndReparentTo(Node newParent)
        {
            Node instance = self.Instance();
            instance.ReplaceBy(newParent);
            instance.QueueFree();
        }
    }
}