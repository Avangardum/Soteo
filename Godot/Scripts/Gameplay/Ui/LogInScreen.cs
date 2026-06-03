using Soteo.Core.Gameplay.Interfaces;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Ui;

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
        string email = _emailLineEdit.Text;
        string password = _passwordLineEdit.Text;
        _campaignServerConnector.ConnectAsPlayer(email, password);
        _node.Visible = false;
    }
}
