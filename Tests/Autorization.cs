using System;
using Xunit;
using BLL.Models;
using BLL.Models.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PL.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


public class AutorizationTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Mock<ILogger<Autorization>> _loggerMock;

    public AutorizationTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _loggerMock = new Mock<ILogger<Autorization>>();
    }

    [Fact]
    public async Task CreateNewUser_ShouldThrowException_WhenFieldsAreEmpty()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var service = new Autorization(context, _loggerMock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateNewUser("", "", ""));
    }


    [Fact]
    public async Task CreateNewUser_ShouldCreateUserSuccessfully()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var service = new Autorization(context, _loggerMock.Object);

        var user = await service.CreateNewUser("newuser", "new@example.com", "password");
        Assert.NotNull(user);
        Assert.Equal("newuser", user.Name);
        Assert.Equal("new@example.com", user.Email);
    }

    [Fact]
    public void Login_ShouldThrowException_WhenUserDoesNotExist()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        var service = new Autorization(context, _loggerMock.Object);

        Assert.Throws<InvalidOperationException>(() => service.Login("unknown", "password"));
    }

    [Fact]
    public void Login_ShouldAuthenticateSuccessfully()
    {
        using var context = new ApplicationDbContext(_dbContextOptions);
        context.Users.Add(new User { Id = 1, Name = "testuser", Email = "test@example.com", Password = "password" });
        context.SaveChanges();

        var service = new Autorization(context, _loggerMock.Object);
        var result = service.Login("testuser", "password");

        Assert.True(result);
        Assert.Equal(1, Autorization.CurrentUserId);
    }
}