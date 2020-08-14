using GraphQL.Types;
using GraphQL.Types.Relay;
using JoinMonster;

namespace StarWars.Types
{
    public class CharacterInterface : InterfaceGraphType
    {
        public CharacterInterface()
        {
            Name = "Character";

            this.SqlTable("characters", "id").AlwaysFetch("type");

            Field<NonNullGraphType<IdGraphType>>("id", "The id of the character.");
            Field<StringGraphType>("name", "The name of the character.");

            Field<ListGraphType<CharacterInterface>>("friends");

            // Field<ConnectionType<CharacterInterface, EdgeType<CharacterInterface>>>("friendsConnection");
            // Field<ListGraphType<EpisodeEnum>>("appearsIn", "Which movie they appear in.");
        }
    }
}
