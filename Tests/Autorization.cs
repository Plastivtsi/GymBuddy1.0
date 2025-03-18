using System;
using Xunit;
using BLL.Models;

public class AutorizationTests
{
    private readonly Autorization _auth;

    public AutorizationTests()
    {
        _auth = new Autorization();
    }

    [Fact]
    public void Register_ThrowsException_WhenFieldsAreEmpty()
    {
        try
        {
            _auth.Register("", "", "");
        }
        catch (ArgumentException ex)
        {
            Assert.Equal("All fields must be filled.", ex.Message);
        }
    }

    [Fact]
    public void Register_ThrowsException_WhenEmailIsInvalid()
    {
        try
        {
            _auth.Register("User", "invalidemail", "password123");
        }
        catch (ArgumentException ex)
        {
            Assert.Equal("Invalid email format.", ex.Message);
        }
    }

    [Fact]
    public void Register_ThrowsException_WhenNicknameAlreadyExists()
    {
        try
        {
            _auth.Register("ExistingUser", "user@example.com", "password123");
        }
        catch (InvalidOperationException ex)
        {
            Assert.Equal("User with this nickname already exists.", ex.Message);
        }
    }

    [Fact]
    public void Login_ThrowsException_WhenUserNotFound()
    {
        try
        {
            _auth.Login("NonExistentUser", "password123");
        }
        catch (InvalidOperationException ex)
        {
            Assert.Equal("User not found.", ex.Message);
        }
    }

}
