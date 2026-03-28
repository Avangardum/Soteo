using System;
using System.Text;
using Godot;
using Environment = System.Environment;

namespace Soteo.Client;

public class LogIn : Control
{
    const string AuthServerUrl = "localhost:3705";
    
    private LineEdit _emailLineEdit = null!;
    private LineEdit _passwordLineEdit = null!;
    private HTTPRequest _httpRequest = null!;
    private bool _isHttpRequestInProgress;

    public override void _Ready()
    {
        _emailLineEdit = GetNode<LineEdit>("Email");
        _passwordLineEdit = GetNode<LineEdit>("Password");
        _httpRequest = GetNode<HTTPRequest>("HTTPRequest");
    }

    public void OnPlayerButtonUp()
    {
        if (_isHttpRequestInProgress) return;
        _isHttpRequestInProgress = true;
        string[] headers = ["Content-Type: application/x-www-form-urlencoded"];
        string email = _emailLineEdit.Text;
        string password = _passwordLineEdit.Text;
        string body = $"email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}";
        string url = $"http://{AuthServerUrl}/token"; // todo https
        _httpRequest.Request(url, method: HTTPClient.Method.Post, customHeaders: headers, requestData: body);
    }
    
    public void OnShardButtonUp()
    {
        if (_isHttpRequestInProgress) return;
        _isHttpRequestInProgress = true;
        string[] headers = ["Content-Type: application/x-www-form-urlencoded"];
        string intercomSecret = Environment.GetEnvironmentVariable("Soteo__IntercomSecret") ??
                                throw new InvalidOperationException("Intercom secret is not set.");
        var id = Guid.NewGuid();
        string body = $"id={Uri.EscapeDataString(id.ToString())}&role=shard" +
                      $"&intercomSecret={Uri.EscapeDataString(intercomSecret)}";
        string url = $"http://{AuthServerUrl}/token/service"; // todo https
        _httpRequest.Request(url, method: HTTPClient.Method.Post, customHeaders: headers, requestData: body);
    }
    
    public void OnRequestCompleted(int result, int responseCode, string[] headers, byte[] body)
    {
        _isHttpRequestInProgress = false;
        GD.Print(responseCode + Encoding.UTF8.GetString(body));
    }
}