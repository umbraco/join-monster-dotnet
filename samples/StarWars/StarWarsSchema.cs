using System;
using GraphQL.StarWars;
using GraphQL.Types;
using GraphQL.Utilities;

namespace StarWars
{
    public class StarWarsSchema : Schema
    {
        public StarWarsSchema(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<StarWarsQuery>();

            // Description = "Example StarWars universe schema";
        }
    }
}
