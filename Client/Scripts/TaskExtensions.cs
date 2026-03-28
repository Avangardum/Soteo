using System.Threading.Tasks;
using Godot;

namespace Soteo.Client;

public static class TaskExtensions
{
    extension (Task task)
    {
        public void PrintException()
        {
            task.ContinueWith(_ => { if (task.IsFaulted) GD.Print(task.Exception); });
        }
    }
}