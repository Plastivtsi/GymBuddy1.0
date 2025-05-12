using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BLL.Models;
using DAL.Models;

public class FriendshipServiceRevard
{
    private ApplicationDbContext GetContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ReturnsSingleExerciseRecord()
    {
        using var context = GetContext("TestDb1");
        context.Trainings.Add(new Training
        {
            UserId = 1,
            Template = false,
            Exercises = new List<Exercise>
            {
                new Exercise { Name = "Push Up", Weight = 0, Repetitions = 20 }
            }
        });
        await context.SaveChangesAsync();

        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Single(result);
        Assert.Equal("Push Up", result[0].ExerciseName);
        Assert.Equal(0, result[0].MaxWeight);
        Assert.Equal(20, result[0].MaxReps);
    }

    [Fact]
    public async Task ReturnsMaxWeightAndReps()
    {
        using var context = GetContext("TestDb2");
        context.Trainings.Add(new Training
        {
            UserId = 1,
            Template = false,
            Exercises = new List<Exercise>
            {
                new Exercise { Name = "Squat", Weight = 100, Repetitions = 10 },
                new Exercise { Name = "Squat", Weight = 120, Repetitions = 8 }
            }
        });
        await context.SaveChangesAsync();

        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Single(result);
        Assert.Equal("Squat", result[0].ExerciseName);
        Assert.Equal(120, result[0].MaxWeight);
        Assert.Equal(10, result[0].MaxReps);
    }

    [Fact]
    public async Task IgnoresTemplateTrainings()
    {
        using var context = GetContext("TestDb3");
        context.Trainings.Add(new Training
        {
            UserId = 1,
            Template = true,
            Exercises = new List<Exercise>
            {
                new Exercise { Name = "Deadlift", Weight = 150, Repetitions = 5 }
            }
        });
        await context.SaveChangesAsync();

        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ReturnsMultipleExerciseTypes()
    {
        using var context = GetContext("TestDb4");
        context.Trainings.Add(new Training
        {
            UserId = 1,
            Template = false,
            Exercises = new List<Exercise>
            {
                new Exercise { Name = "Bench", Weight = 90, Repetitions = 8 },
                new Exercise { Name = "Row", Weight = 60, Repetitions = 12 }
            }
        });
        await context.SaveChangesAsync();

        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.ExerciseName == "Bench");
        Assert.Contains(result, r => r.ExerciseName == "Row");
    }

    [Fact]
    public async Task IgnoresOtherUsers()
    {
        using var context = GetContext("TestDb5");
        context.Trainings.Add(new Training
        {
            UserId = 2,
            Template = false,
            Exercises = new List<Exercise>
            {
                new Exercise { Name = "Pull Up", Weight = 0, Repetitions = 10 }
            }
        });
        await context.SaveChangesAsync();

        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ReturnsEmpty_WhenNoExercises()
    {
        using var context = GetContext("TestDb6");
        context.Trainings.Add(new Training
        {
            UserId = 1,
            Template = false,
            Exercises = new List<Exercise>()
        });
        await context.SaveChangesAsync();

        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandlesSameWeightWithDifferentReps()
    {
        using var context = GetContext("TestDb7");
        context.Trainings.Add(new Training
        {
            UserId = 1,
            Template = false,
            Exercises = new List<Exercise>
            {
                new Exercise { Name = "Dip", Weight = 40, Repetitions = 8 },
                new Exercise { Name = "Dip", Weight = 40, Repetitions = 12 }
            }
        });
        await context.SaveChangesAsync();

        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Single(result);
        Assert.Equal(40, result[0].MaxWeight);
        Assert.Equal(12, result[0].MaxReps);
    }

    [Fact]
    public async Task HandlesWhitespaceInExerciseName()
    {
        using var context = GetContext("TestDb8");
        context.Trainings.Add(new Training
        {
            UserId = 1,
            Template = false,
            Exercises = new List<Exercise>
            {
                new Exercise { Name = "Plank ", Weight = 0, Repetitions = 60 },
                new Exercise { Name = "Plank", Weight = 0, Repetitions = 90 }
            }
        });
        await context.SaveChangesAsync();

        // GroupBy fails if names differ by whitespace; clean beforehand if needed in service
        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Equal(2, result.Count); // depending on implementation — both "Plank" and "Plank " may count
    }

    [Fact]
    public async Task HandlesNoTrainings()
    {
        using var context = GetContext("TestDb9");
        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandlesZeroWeightCorrectly()
    {
        using var context = GetContext("TestDb10");
        context.Trainings.Add(new Training
        {
            UserId = 1,
            Template = false,
            Exercises = new List<Exercise>
            {
                new Exercise { Name = "Run", Weight = 0, Repetitions = 100 }
            }
        });
        await context.SaveChangesAsync();

        var service = new FriendshipService(context);
        var result = await service.GetFriendExerciseRecords(1);

        Assert.Single(result);
        Assert.Equal(0, result[0].MaxWeight);
        Assert.Equal(100, result[0].MaxReps);
    }

}
