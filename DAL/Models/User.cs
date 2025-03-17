public class User
{
    public int Id { get; set; }
#pragma warning disable SA1206 // Declaration keywords should follow order
    public required string UserName { get; set; }
#pragma warning restore SA1206 // Declaration keywords should follow order
#pragma warning disable SA1206 // Declaration keywords should follow order
    public required string Email { get; set; }
#pragma warning restore SA1206 // Declaration keywords should follow order
#pragma warning disable SA1206 // Declaration keywords should follow order
    public required string Password { get; set; }
#pragma warning restore SA1206 // Declaration keywords should follow order
    public double Weight { get; set; }
    public double Height { get; set; }
    public bool Role { get; set; } // true = Admin, false = Non-Admin
}
