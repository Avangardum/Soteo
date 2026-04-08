using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;

namespace Soteo.Gameplay.Nodes;

public class LogIn : Control
{
    private LineEdit _emailLineEdit = null!;
    private LineEdit _passwordLineEdit = null!;
    private IMasterServerCommunicator _masterServerCommunicator = null!;

    [Inject]
    public void Inject(IMasterServerCommunicator masterServerCommunicator)
    {
        _masterServerCommunicator = masterServerCommunicator;
    }
    
    public override void _Ready()
    {
        _emailLineEdit = GetNode<LineEdit>("Email");
        _passwordLineEdit = GetNode<LineEdit>("Password");
    }

    public void OnPlayerButtonUp()
    {
        string email = _emailLineEdit.Text;
        string password = _passwordLineEdit.Text;
        _masterServerCommunicator.ConnectAsPlayer(email, password);
        Visible = false;
    }
}