using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace CippMcp.Tools;

[McpServerToolType]
public sealed class MonkeyTools
{
    public Task<string> GetRandomFact()
    {
        var facts = new[]
        {
            "Monkeys use tools in the wild!",
            "Some monkeys can count.",
            "Capuchin monkeys are known for their intelligence.",
            "Marmosets are among the smallest monkeys."
        };
        var random = new Random();
        var fact = facts[random.Next(facts.Length)];
        return Task.FromResult(fact);
    }

    private static readonly List<Monkey> monkeys;
    static MonkeyTools()
    {
        var rand = new Random();
        monkeys = new List<Monkey>
        {
            new Monkey { Name = "Banana", Species = "Capuchin", Age = 5, InterestingFact = "Capuchins are known for their intelligence and ability to use tools.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Coco", Species = "Marmoset", Age = 3, InterestingFact = "Marmosets are among the smallest monkeys and often give birth to twins.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "George", Species = "Curious George (Fictional)", Age = 7, InterestingFact = "Curious George is a beloved children's book character known for his curiosity.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Jack", Species = "Capuchin", Age = 10, InterestingFact = "Jack is the famous monkey sidekick from the Pirates of the Caribbean movies.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Albert", Species = "Rhesus Macaque", Age = 6, InterestingFact = "Albert I was the first monkey launched into space in 1948.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Gordo", Species = "Squirrel Monkey", Age = 4, InterestingFact = "Gordo survived a suborbital space flight in 1958, paving the way for human space travel.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Enos", Species = "Chimpanzee", Age = 5, InterestingFact = "Enos was the first chimpanzee to orbit the Earth in 1961.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Kanzi", Species = "Bonobo", Age = 43, InterestingFact = "Kanzi is famous for communicating with humans using lexigrams.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Bubbles", Species = "Chimpanzee", Age = 41, InterestingFact = "Bubbles was Michael Jackson's pet and often appeared in public with him.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Abu", Species = "Capuchin", Age = 8, InterestingFact = "Abu is Aladdin's loyal monkey companion in Disney's Aladdin.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Spike", Species = "Capuchin", Age = 12, InterestingFact = "Spike appeared in both 'Ace Ventura: Pet Detective' and the TV show 'Friends'.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Crystal", Species = "Capuchin", Age = 30, InterestingFact = "Crystal is a Hollywood animal actor, starring in 'Night at the Museum' and other films.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Koko", Species = "Gorilla", Age = 46, InterestingFact = "Koko was famous for learning and using sign language to communicate.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Mr. Teeny", Species = "Chimpanzee", Age = 9, InterestingFact = "Mr. Teeny is Krusty the Clown’s chain-smoking sidekick on 'The Simpsons'.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Hanuman", Species = "Langur", Age = 15, InterestingFact = "Named after the Hindu monkey god, Hanuman langurs are revered in India.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Tarsier Tom", Species = "Tarsier", Age = 2, InterestingFact = "Tarsiers have the largest eyes relative to body size of any mammal.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Mandrill Max", Species = "Mandrill", Age = 13, InterestingFact = "Mandrills have brightly colored faces and are the largest monkey species.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Snowball", Species = "Japanese Macaque", Age = 7, InterestingFact = "Japanese macaques, or snow monkeys, are famous for bathing in hot springs.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Momo", Species = "Spider Monkey", Age = 6, InterestingFact = "Spider monkeys have prehensile tails that act like a fifth limb.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Rafiki", Species = "Mandrill (Fictional)", Age = 20, InterestingFact = "Rafiki is the wise shaman in Disney's 'The Lion King'.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "King Louie", Species = "Orangutan (Fictional)", Age = 25, InterestingFact = "King Louie is the jazz-loving orangutan king in 'The Jungle Book'.", Bananas = rand.Next(0, 21) },
            new Monkey { Name = "Clyde", Species = "Orangutan", Age = 14, InterestingFact = "Clyde was Clint Eastwood’s sidekick in 'Every Which Way But Loose'.", Bananas = rand.Next(0, 21) }
        };
    }


    [McpServerTool, Description("Get a list of monkeys. Each entry includes a description, title, and read-only hint.")]
    public Task<string> GetMonkeys()
    {
        var result = monkeys.Select(m => new
        {
            m.Name,
            m.Species,
            m.Age,
            m.InterestingFact,
            m.Bananas,
            Description = $"{m.Name} is a {m.Species} monkey.",
            Title = $"Monkey: {m.Name}",
            ReadOnlyHint = true
        }).ToList();
        return Task.FromResult(JsonSerializer.Serialize(result));
    }

    [McpServerTool, Description("Get a monkey by name. Includes description, title, and read-only hint.")]
    public Task<string> GetMonkey(
        [Description("The name of the monkey to get details for")] string name
    )
    {
        var monkey = monkeys.FirstOrDefault(m => m.Name == name);
        if (monkey == null)
        {
            return Task.FromResult(JsonSerializer.Serialize(new { Error = $"Monkey '{name}' not found." }));
        }
        var result = new
        {
            monkey.Name,
            monkey.Species,
            monkey.Age,
            monkey.InterestingFact,
            monkey.Bananas,
            Description = $"{monkey.Name} is a {monkey.Species} monkey.",
            Title = $"Monkey: {monkey.Name}",
            ReadOnlyHint = true
        };
        return Task.FromResult(JsonSerializer.Serialize(result));
    }

    [McpServerTool, Description("Returns the number of bananas a monkey currently has in their stash."), DisplayName("GetBananaCount")]
    public Task<string> GetBananaCount(
        [Description("The name of the monkey to get the banana count for")] string name
    )
    {
        var monkey = monkeys.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (monkey == null)
        {
            return Task.FromResult(JsonSerializer.Serialize(new { Error = $"Monkey '{name}' not found." }));
        }
        var result = new
        {
            Name = monkey.Name,
            Bananas = monkey.Bananas,
            Description = "Bananas are classic monkey currency!",
            Title = "Banana Count",
            ReadOnlyHint = true
        };
        return Task.FromResult(JsonSerializer.Serialize(result));
    }

    [McpServerTool, Description("Give a banana to a monkey by name. Increments the monkey's banana count by 1 and returns the new count.")]
    public Task<string> GiveBanana(
        [Description("The name of the monkey to give a banana to")] string name
    )
    {
        var monkey = monkeys.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (monkey == null)
        {
            return Task.FromResult(JsonSerializer.Serialize(new { Error = $"Monkey '{name}' not found." }));
        }
        monkey.Bananas += 1;
        var result = new
        {
            Name = monkey.Name,
            Bananas = monkey.Bananas,
            Message = $"{monkey.Name} now has {monkey.Bananas} bananas!"
        };
        return Task.FromResult(JsonSerializer.Serialize(result));
    }
}
