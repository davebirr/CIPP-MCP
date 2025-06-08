using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace CippMcp.Tools;

[McpServerToolType]
public sealed class MonkeyTools
{
    private static readonly List<Monkey> monkeys = new List<Monkey>
{
    new Monkey { Name = "Banana", Species = "Capuchin", Age = 5, InterestingFact = "Capuchins are known for their intelligence and ability to use tools." },
    new Monkey { Name = "Coco", Species = "Marmoset", Age = 3, InterestingFact = "Marmosets are among the smallest monkeys and often give birth to twins." },
    new Monkey { Name = "George", Species = "Curious George (Fictional)", Age = 7, InterestingFact = "Curious George is a beloved children's book character known for his curiosity." },
    new Monkey { Name = "Jack", Species = "Capuchin", Age = 10, InterestingFact = "Jack is the famous monkey sidekick from the Pirates of the Caribbean movies." },
    new Monkey { Name = "Albert", Species = "Rhesus Macaque", Age = 6, InterestingFact = "Albert I was the first monkey launched into space in 1948." },
    new Monkey { Name = "Gordo", Species = "Squirrel Monkey", Age = 4, InterestingFact = "Gordo survived a suborbital space flight in 1958, paving the way for human space travel." },
    new Monkey { Name = "Enos", Species = "Chimpanzee", Age = 5, InterestingFact = "Enos was the first chimpanzee to orbit the Earth in 1961." },
    new Monkey { Name = "Kanzi", Species = "Bonobo", Age = 43, InterestingFact = "Kanzi is famous for communicating with humans using lexigrams." },
    new Monkey { Name = "Bubbles", Species = "Chimpanzee", Age = 41, InterestingFact = "Bubbles was Michael Jackson's pet and often appeared in public with him." },
    new Monkey { Name = "Abu", Species = "Capuchin", Age = 8, InterestingFact = "Abu is Aladdin's loyal monkey companion in Disney's Aladdin." },
    new Monkey { Name = "Spike", Species = "Capuchin", Age = 12, InterestingFact = "Spike appeared in both 'Ace Ventura: Pet Detective' and the TV show 'Friends'." },
    new Monkey { Name = "Crystal", Species = "Capuchin", Age = 30, InterestingFact = "Crystal is a Hollywood animal actor, starring in 'Night at the Museum' and other films." },
    new Monkey { Name = "Koko", Species = "Gorilla", Age = 46, InterestingFact = "Koko was famous for learning and using sign language to communicate." },
    new Monkey { Name = "Mr. Teeny", Species = "Chimpanzee", Age = 9, InterestingFact = "Mr. Teeny is Krusty the Clown’s chain-smoking sidekick on 'The Simpsons'." },
    new Monkey { Name = "Hanuman", Species = "Langur", Age = 15, InterestingFact = "Named after the Hindu monkey god, Hanuman langurs are revered in India." },
    new Monkey { Name = "Tarsier Tom", Species = "Tarsier", Age = 2, InterestingFact = "Tarsiers have the largest eyes relative to body size of any mammal." },
    new Monkey { Name = "Mandrill Max", Species = "Mandrill", Age = 13, InterestingFact = "Mandrills have brightly colored faces and are the largest monkey species." },
    new Monkey { Name = "Snowball", Species = "Japanese Macaque", Age = 7, InterestingFact = "Japanese macaques, or snow monkeys, are famous for bathing in hot springs." },
    new Monkey { Name = "Momo", Species = "Spider Monkey", Age = 6, InterestingFact = "Spider monkeys have prehensile tails that act like a fifth limb." },
    new Monkey { Name = "Rafiki", Species = "Mandrill (Fictional)", Age = 20, InterestingFact = "Rafiki is the wise shaman in Disney's 'The Lion King'." },
    new Monkey { Name = "King Louie", Species = "Orangutan (Fictional)", Age = 25, InterestingFact = "King Louie is the jazz-loving orangutan king in 'The Jungle Book'." },
    new Monkey { Name = "Clyde", Species = "Orangutan", Age = 14, InterestingFact = "Clyde was Clint Eastwood’s sidekick in 'Every Which Way But Loose'." }
};


    [McpServerTool, Description("Get a list of monkeys.")]
    public Task<string> GetMonkeys()
    {
        return Task.FromResult(JsonSerializer.Serialize(monkeys, MonkeyContext.Default.ListMonkey));
    }

    [McpServerTool, Description("Get a monkey by name.")]
    public Task<string> GetMonkey(
        [Description("The name of the monkey to get details for")] string name
    )
    {
        var monkey = monkeys.FirstOrDefault(m => m.Name == name);
        return Task.FromResult(JsonSerializer.Serialize(monkey, MonkeyContext.Default.Monkey));
    }
}
