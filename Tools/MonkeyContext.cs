using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CippMcp.Tools;

[JsonSerializable(typeof(List<Monkey>))]
[JsonSerializable(typeof(Monkey))]
public partial class MonkeyContext : JsonSerializerContext { }
