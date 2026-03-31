public static class ApiResponse
{
    public static object Message(string message)
    {
        return new { message };
    }

    public static object Data<T>(string message, T data)
    {
        return new { message, data };
    }
}
