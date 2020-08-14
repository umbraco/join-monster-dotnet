using GraphQL.Types;
using JoinMonster;

namespace StarWars.Types
{
    public class PlanetType : ObjectGraphType
    {
        public PlanetType()
        {
            Name = "Planet";

            this.SqlTable("planets", "id");

            Field<NonNullGraphType<IdGraphType>>("id", "The id of the planet.").SqlColumn();
            Field<StringGraphType>("name", "The name of the planet.").SqlColumn();
        }
    }
}
