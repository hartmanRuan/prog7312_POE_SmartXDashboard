public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public SensorNode? SensorNode { get; set; }
}

public class SensorNode
{
    public int Id { get; set; }
    public string MacAddress { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string LocationZone { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? FilePath { get; set; }
    public User? User { get; set; }
}