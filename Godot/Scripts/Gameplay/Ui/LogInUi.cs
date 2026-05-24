using Soteo.Core.Gameplay.Interfaces;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Ui;

public class LogInUi : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/LogIn.tscn");
    
    private readonly LineEdit _emailLineEdit;
    private readonly LineEdit _passwordLineEdit;
    private readonly ICampaignServerCommunicator _campaignServerCommunicator;

    public LogInUi(ICampaignServerCommunicator campaignServerCommunicator)
    {
        _campaignServerCommunicator = campaignServerCommunicator;
        Scene.InstanceAndReparentTo(this);
        _emailLineEdit = GetNode<LineEdit>("Email");
        _passwordLineEdit = GetNode<LineEdit>("Password");
        GetNode<Button>("LogIn").Connect("pressed", this, nameof(OnLogInPressed));
        LoadCredentialsFromCmdLineArgs();
        Name = nameof(LogInUi);
    }
    
    private void LoadCredentialsFromCmdLineArgs()
    {
        string[] args = OS.GetCmdlineArgs();
        int emailIndex = args.IndexOf("--email") + 1;
        if (emailIndex > 0 && emailIndex < args.Length)
            _emailLineEdit.Text = args[emailIndex];
    }

    public void OnLogInPressed()
    {
        string email = _emailLineEdit.Text;
        string password = _passwordLineEdit.Text;
        _campaignServerCommunicator.ConnectAsPlayer(email, password);
        Visible = false;
    }
}