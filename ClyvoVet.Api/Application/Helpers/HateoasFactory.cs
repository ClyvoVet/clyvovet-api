using ClyvoVet.API.Application.Dtos;

namespace ClyvoVet.API.Application.Helpers
{
    public static class HateoasFactory
    {
        public static List<LinkDto> ItemLinks(string resource, object id) =>
        [
            new($"/api/{resource}/{id}", "self", "GET"),
            new($"/api/{resource}/{id}", "update", "PUT"),
            new($"/api/{resource}/{id}", "delete", "DELETE")
        ];

        public static List<LinkDto> PageLinks(
            string resource,
            int deslocamento,
            int registroRetornado,
            int totalRegistros,
            string additionalQuery = "")
        {
            var suffix = string.IsNullOrWhiteSpace(additionalQuery)
                ? string.Empty
                : "&" + additionalQuery.TrimStart('&');

            var links = new List<LinkDto>
            {
                new($"/api/{resource}?deslocamento={deslocamento}&registroRetornado={registroRetornado}{suffix}", "self", "GET"),
                new($"/api/{resource}", "create", "POST")
            };

            var next = deslocamento + registroRetornado;
            if (next < totalRegistros)
                links.Add(new($"/api/{resource}?deslocamento={next}&registroRetornado={registroRetornado}{suffix}", "next", "GET"));

            if (deslocamento > 0)
            {
                var previous = Math.Max(0, deslocamento - registroRetornado);
                links.Add(new($"/api/{resource}?deslocamento={previous}&registroRetornado={registroRetornado}{suffix}", "previous", "GET"));
            }

            return links;
        }
    }
}
