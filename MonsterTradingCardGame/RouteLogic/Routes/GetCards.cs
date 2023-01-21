using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class GetCards : Route
    {

        public GetCards()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            // Get the user_id associated with the authorization token
            int userId = UserManager.Instance.GetUserId(AuthToken);

            // Get all cards of user
            var data = GetAllCardsOfUser(userId);

            // If there was no error, loop all rows and add each card to the body
            if (data.ErrorMessage == null)
            {
                var response = HttpResponse.Ok.WithStatusMessage("Success! You own " + data.Rows + " cards.");

                var cardsList = new JArray();

                for (var i = 0; i < data.Rows; i++)
                {
                    var card = new Card(
                        Guid.Parse(data["card_id"][i]!),
                        data["name"][i]!,
                        float.Parse(data["damage"][i]!),
                        int.Parse(data["posindeck"][i]!)
                    );

                    var cardJson = JObject.FromObject(card);

                    cardsList.Add(cardJson);
                }

                response.Body.Add("cards", cardsList);

                return response;
            }
            else
                return HttpResponse.InternalServerError;
        }

        private DatabaseResponse GetAllCardsOfUser(int userId)
        {
            var request = new DatabaseRequest("SELECT * FROM cards WHERE owner = @owner_id");

            request.Data.Add("owner_id", userId);

            return request.PerformQuery();
        }

        // Not used in this class, but makes sense here and is used in other routes
        public static Card? GetCard(Guid cardId)
        {
            var request = new DatabaseRequest("SELECT * FROM cards WHERE card_id = @card_id");

            request.Data.Add("card_id", cardId);

            var data = request.PerformQuery();

            if (data.ErrorMessage == null && data.Rows > 0)
            {
                var card = new Card(
                    Guid.Parse(data["card_id"][0]),
                    data["name"][0],
                    float.Parse(data["damage"][0]),
                    int.Parse(data["posindeck"][0])
                );

                return card;
            }
            else
                return null;
        }
    }
}
