using System.Collections.Generic;
using GraphQL.Types;
using JoinMonster;

namespace StarWars.Types
{
    public class HumanType : ObjectGraphType
    {
        public HumanType()
        {
            Name = "Human";

            this.SqlTable("characters", "id").AlwaysFetch("type");

            Field<NonNullGraphType<IdGraphType>>("id", "The id of the human.").SqlColumn();
            Field<StringGraphType>("name", "The name of the human.").SqlColumn();

            Field<ListGraphType<CharacterInterface>>("friends")
                // .SqlJunction("friends", new [] { "characterId", "friendId" }, "friendId", "id", (friends, characters, _, __) => $"{friends}.\"id\" = {characters}.\"id\"")
                .SqlJunction("friends",
                    (join, humans, friends, _) => join.On("id", "characterId"),//$"{humans}.\"id\" = {friends}.\"characterId\"",
                    (join, friends, characters, _) => join.On("friendId", "id"))// $"{friends}.\"friendId\" = {characters}.\"id\"")
                .Where((where, _, __, ___) => where.Columns("id", "characterId", "<>"));

            // Connection<CharacterInterface>()
            //     .Name("friendsConnection")
            //     .Description("A list of a character's friends.")
            //     .Bidirectional()
            //     .Resolve(context => context.GetPagedResults<Human, StarWarsCharacter>(data, context.Source.Friends));
            //
            // Field<ListGraphType<EpisodeEnum>>("appearsIn", "Which movie they appear in.");

            Field<ListGraphType<PlanetType>>("homePlanet", "The home planet of the human.")
                .SqlJoin((join, humans, planets, _) => join.On("homePlanet", "id"));

            Interface<CharacterInterface>();

            IsTypeOf = o => Equals(((IDictionary<string, object>) o)["type"], "Human");
        }
    }
}
