using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class GetDeck : Route
    {

        // Specifies if the information returned should be normal or plain version
        private Format RepresentationFormat { get; set; }

        public enum Format
        {
            Normal,
            Plain
        }

        public GetDeck(Format format = Format.Normal)
        {
            AuthLevelRequired = User.UserRoles.Normal;
            RepresentationFormat = format;
        }

        public override HttpResponse Call()
        {
            // Get the user_id associated with the authorization token
            var userId = UserManager.Instance.GetUserId(AuthToken);

            // Get the deck of the user
            var data = GetDeckOfUser(userId);

            if (data != null)
            {
                var response = HttpResponse.Ok;

                var cardsList = new JArray();

                for (int i = 0; i < data.Count; i++)
                {
                    if (RepresentationFormat == Format.Normal)
                    {
                        var cardJson = JObject.FromObject(data[i]);

                        cardsList.Add(cardJson);
                    }
                    else if (RepresentationFormat == Format.Plain)
                    {
                        var card = new JObject
                        {
                            { "name",  data[i].Name },
                            { "damage", data[i].Damage }
                        };

                        cardsList.Add(card);
                    }
                    else
                        return HttpResponse.InternalServerError;
                }

                response.Body.Add("deck", cardsList);

                return response;
            }
            else
                return HttpResponse.InternalServerError;
        }

        // Marked as static because the DoBattle class makes use of it too
        public static List<Card>? GetDeckOfUser(int userId)
        {
            var request = new DatabaseRequest("SELECT * FROM cards WHERE owner = @owner_id AND posindeck <> 0");

            request.Data.Add("owner_id", userId);

            var data = request.PerformQuery();

            var cardsList = new List<Card>();

            if (data.ErrorMessage == null)
            {
                for (int i = 0; i < data.Rows; i++)
                {
                    var card = new Card(
                        Guid.Parse(data["card_id"][i]),
                        data["name"][i],
                        float.Parse(data["damage"][i]),
                        int.Parse(data["posindeck"][i])
                    );

                    cardsList.Add(card);
                }
            }
            else
                return null;

            return cardsList;
        }
    }
}
