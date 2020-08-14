using System.Collections.Generic;
using GraphQL.Types;
using JoinMonster;

namespace StarWars.Types
{
    public class DroidType : ObjectGraphType
    {
        public DroidType()
        {
            Name = "Droid";
            Description = "A mechanical creature in the Star Wars universe.";

            this.SqlTable("characters", "id").AlwaysFetch("type");

            Field<NonNullGraphType<IdGraphType>>("id", "The id of the droid.").SqlColumn();
            Field<StringGraphType>("name", "The name of the droid.").SqlColumn();

            Field<ListGraphType<CharacterInterface>>("friends");
                // .SqlJunction("friends",
                //     (droids, friends, _, __) => $"{droids}.\"id\" = {friends}.\"characterId\"",
                //     (friends, characters, _, __) => $"{friends}.\"friendId\" = {characters}.\"id\"")
                // .Where((where, _, __, ___) => where.Not().ColumnsEqual("id", "characterId"));

            // Connection<CharacterInterface>()
            //     .Name("friendsConnection")
            //     .Description("A list of a character's friends.")
            //     .Bidirectional()
            //     .Resolve(context => context.GetPagedResults<Droid, StarWarsCharacter>(data, context.Source.Friends));
            //
            // Field<ListGraphType<EpisodeEnum>>("appearsIn", "Which movie they appear in.");

            Field<StringGraphType>("primaryFunction", "The primary function of the droid.").SqlColumn();

            Interface<CharacterInterface>();
            IsTypeOf = o => Equals(((IDictionary<string, object>) o)["type"], "Droid");
        }
    }
}
