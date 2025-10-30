using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public static class CardLoader
{
    public static async Task<List<CardData>> LoadCardsFromJson(string filePath)
    {
        var options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };

        using FileStream openStream = File.OpenRead(filePath);

        return await JsonSerializer.DeserializeAsync<List<CardData>>(openStream, options);
    }
}
