using Newtonsoft.Json.Linq;
using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class ConfigureDeck : Route
    {
        public List<Guid>? CardIDsInDeckOrder = null;

        public ConfigureDeck()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            // Get the user_id associated with the authorization token
            int userId = UserManager.Instance.GetUserId(AuthToken);

            // Check if all provided cards exist
            if (!CardsExistAreOwnedAndAreNotInShop(CardIDsInDeckOrder!, userId))
                return HttpResponse.BadRequest.WithStatusMessage("Error! One or more cards are one or more of the following: Not owned by you | Non existent | Offered in shop");

            // Reset old deck configuration
            // If not successful, return an internal server error
            if (!ResetCurrentDeckConfiguration(userId))
                return HttpResponse.InternalServerError;

            // Add new deck configuration
            if (!AddDeckOrderToCards(CardIDsInDeckOrder!))
                return HttpResponse.InternalServerError;

            return HttpResponse.Ok;
        }

        public override bool AddData(JObject data)
        {
            if (!data.ContainsKey("Array"))
                return false;

            JArray array = data["Array"]!.ToObject<JArray>()!;
            CardIDsInDeckOrder = new ();

            // Loop each card_id in user provided array
            foreach (var entry in array)
            {
                // Convert the entry to a guid
                var cardId = entry.ToObject<Guid>()!;

                // Adds card object to list
                CardIDsInDeckOrder.Add(cardId);
            }

            // If more (or less) than 4 cards were given, also return false
            if (CardIDsInDeckOrder.Count != 4)
                return false;

            // If each object the user provided was correct, AddData was successful
            return true;
        }

        public static bool CardBelongsToUser(Guid cardId, int userId)
        {
            var request = new DatabaseRequest("SELECT * FROM cards WHERE owner = @owner_id AND card_id = @card_id");

            request.Data.Add("owner_id", userId);
            request.Data.Add("card_id", cardId);

            return request.PerformQuery().Rows > 0;
        }

        public static bool CardIsInShop(Guid cardId)
        {
            var request = new DatabaseRequest("SELECT * FROM tradingdeals WHERE card_to_trade = @card_id");

            request.Data.Add("card_id", cardId);

            return request.PerformQuery().Rows > 0;
        }

        private bool CardsExistAreOwnedAndAreNotInShop(List<Guid> cardIDs, int userId)
        {
            // Loop all card ids and check if they exist, are owned, and if they are in the shop
            for (int i = 0; i < cardIDs.Count; i++)
            {
                if (!CardBelongsToUser(cardIDs[i], userId) || CardIsInShop(cardIDs[i]))
                    return false;
            }

            // If all cards were found, return true
            return true;
        }

        private bool ResetCurrentDeckConfiguration(int userId)
        {
            var request = new DatabaseRequest("UPDATE cards SET posindeck = 0 WHERE owner = @owner_id");

            request.Data.Add("owner_id", userId);

            return request.PerformQuery().ErrorMessage == null;
        }

        private bool AddDeckOrderToCards(List<Guid> cardsInDeckOrder)
        {
            // Loop all card ids and change the deck number of the corresponding card
            for(int i = 0; i < cardsInDeckOrder.Count; i++)
            {
                var request = new DatabaseRequest("UPDATE cards SET posindeck = " + (i+1) + " WHERE card_id = @card_id");

                request.Data.Add("card_id", cardsInDeckOrder[i]);
                
                if (request.PerformQuery().ErrorMessage != null) return false;
            }

            // If no error was thrown, the operation was successful
            return true;
        }
    }
}
