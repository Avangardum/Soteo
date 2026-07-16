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
        
        if (ClientCmdLineArgs.Email != null)
            _emailLineEdit.Text = ClientCmdLineArgs.Email;
    }
    
    private void OnLogInPressed()
    {
        string email = _emailLineEdit.Text;
        string password = _passwordLineEdit.Text;
        _campaignServerConnector.ConnectAsPlayer(email, password);
        _node.Visible = false;
    }
}
