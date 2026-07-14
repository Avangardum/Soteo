using Soteo.Core.Interfaces;

namespace Soteo.Main.Gameplay.Ui;

public sealed class LogInScreen
{
    private readonly ICampaignServerConnector _campaignServerConnector;
    
    private readonly LogInScreenNode _node;
    private readonly LineEdit _emailLineEdit;
    private readonly LineEdit _passwordLineEdit;

    public LogInScreen(LogInScreenNode node, ICampaignServerConnector campaignServerConnector)
    {
        _campaignServerConnector = campaignServerConnector;
        
        _node = node;
        _emailLineEdit = node.GetNode<LineEdit>("Email");
        _passwordLineEdit = node.GetNode<LineEdit>("Password");
        node.GetNode<Button>("LogIn").Connect("pressed", OnLogInPressed);
        LoadCredentialsFromCmdLineArgs();
    }
    
    private void LoadCredentialsFromCmdLineArgs()
    {
        string[] args = OS.GetCmdlineArgs();
        int emailIndex = args.IndexOf("--email") + 1;
        if (emailIndex > 0 && emailIndex < args.Length)
            _emailLineEdit.Text = args[emailIndex];
    }

    private void OnLogInPressed()
    {
        GD.PrintErr("Hello from PrintErr 1");
        GD.PrintErr("Hello from PrintErr 2");
        GD.PrintErr("Hello from PrintErr 3");
        GD.PrintErr("Hello from PrintErr 4");
        GD.PrintErr("Hello from PrintErr 5");
        string email = _emailLineEdit.Text;
        string password = _passwordLineEdit.Text;
        _campaignServerConnector.ConnectAsPlayer(email, password);
        _node.Visible = false;
    }
}
