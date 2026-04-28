namespace OrderForge.Client.Components.Profile;

internal static class ProfileApiErrors
{
    public static string FromHttpRequest(HttpRequestException ex)
    {
        var msg = ex.Message;
        var idx = msg.IndexOf(": ", StringComparison.Ordinal);
        if (idx > 0 && idx < msg.Length - 2)
        {
            return msg[(idx + 2)..];
        }

        return msg;
    }
}
