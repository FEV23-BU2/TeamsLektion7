using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace TeamsLektion7;

public class ExampleIntegrationTests : IClassFixture<ApplicationFactory<TeamsLektion5.Program>>
{
    ApplicationFactory<TeamsLektion5.Program> factory;

    public ExampleIntegrationTests(ApplicationFactory<TeamsLektion5.Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async void CreateTodo_WhenCreate_ThenSuccess()
    {
        // given
        var client = factory.CreateClient();
        var todoTitle = "Städa";

        // when
        var request = await client.PostAsync($"/todo?title={todoTitle}", null);
        TeamsLektion5.TodoDto? response =
            await request.Content.ReadFromJsonAsync<TeamsLektion5.TodoDto>();

        // then
        request.EnsureSuccessStatusCode();
        Assert.NotNull(response);
        Assert.Equal(todoTitle, response.Title);
    }

    [Fact]
    public async void GetAll_WhenEmpty_ThenReturnEmpty()
    {
        // given
        var client = factory.CreateClient();

        // Radera databasen och skapa den igen specifikt för detta test.
        var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TeamsLektion5.ApplicationContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // when
        var response = await client.GetFromJsonAsync<List<TeamsLektion5.TodoDto>>("/todo");

        // then
        Assert.NotNull(response);
        Assert.Empty(response);
    }
}
