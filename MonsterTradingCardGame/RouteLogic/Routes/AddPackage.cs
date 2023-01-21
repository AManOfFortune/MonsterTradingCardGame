using Newtonsoft.Json.Linq;
using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class AddPackage : Route
    {
        public List<Card>? CardsInPackage = null;

        public AddPackage()
        {
            AuthLevelRequired = User.UserRoles.Admin;
        }

        public override HttpResponse Call()
        {
            var success = CreateNewPackage(CardsInPackage!);

            HttpResponse response;

            if (success)
            {
                response = HttpResponse.Created.WithStatusMessage("Successfully created new package.");
            }
            else
            {
                response = HttpResponse.BadRequest.WithStatusMessage("Make sure the cards have unique IDs and try again.");
            }

            return response;
        }

        public override bool AddData(JObject data)
        {
            if (!data.ContainsKey("Array"))
                return false;

            JArray array = data["Array"]!.ToObject<JArray>()!;
            CardsInPackage = new ();

            // Loop each card in user provided array
            foreach (var entry in array)
            {
                // First check if the type is a JObject
                if (entry.Type != JTokenType.Object)
                    return false;

                JObject cardData = entry.ToObject<JObject>()!;

                var card = new Card();

                // Loop through all properties of a card
                foreach(var property in card.GetType().GetFields())
                {
                    // Check if property exists in user provided json object
                    if (!cardData.ContainsKey(property.Name))
                        return false;

                    // Set the property to the given value
                    property.SetValue(card, cardData[property.Name]!.ToObject(property.FieldType));
                }

                // Adds card object to list
                CardsInPackage.Add(card);
            }

            // If each object the user provided has the correct properties, AddData was successful
            return true;
        }

        private bool CreateNewPackage(List<Card> cardsInPack)
        {
            var request = new DatabaseRequest("INSERT INTO packs(num_of_cards) VALUES(@numberOfCards) returning pack_id");
            request.Data.Add("numberOfCards", cardsInPack.Count);

            // Create a new package
            var response = request.PerformQuery();

            // Parses the response (which should be the pack_id of the newly created package) to an integer
            int.TryParse(response[0][0], out int packId);

            foreach (Card card in cardsInPack)
            {
                var addCardResponse = AddNewCard(card.Id, card.Name, card.Damage, packId);

                if (addCardResponse.ErrorMessage != null)
                    return false;
            }

            return true;
        }

        private DatabaseResponse AddNewCard(Guid id, string name, float damage, int packId)
        {
            var request = new DatabaseRequest("INSERT INTO cards(card_id, name, damage, pack_id) VALUES(@id, @name, @damage, @packId)");

            request.Data.Add("id", id);
            request.Data.Add("name", name);
            request.Data.Add("damage", damage);
            request.Data.Add("packId", packId);

            return request.PerformQuery();
        }
    }
}
