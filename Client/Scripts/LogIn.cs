using System;
using System.Text;
using Godot;
using Soteo.Shared;
using Environment = System.Environment;

namespace Soteo.Client;

public class LogIn : Control
{
    private LineEdit _emailLineEdit = null!;
    private LineEdit _passwordLineEdit = null!;

    public override void _Ready()
    {
        _emailLineEdit = GetNode<LineEdit>("Email");
        _passwordLineEdit = GetNode<LineEdit>("Password");
    }

    public void OnPlayerButtonUp()
    {
        string email = _emailLineEdit.Text;
        string password = _passwordLineEdit.Text;
        MasterServerCommunicator.Instance.ConnectAsPlayer(email, password);
    }
}