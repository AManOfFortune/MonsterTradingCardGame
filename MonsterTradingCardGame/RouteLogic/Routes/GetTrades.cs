using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class GetTrades : Route
    {
        public GetTrades()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            var data = GetAllTrades();

            if (data.ErrorMessage == null)
            {
                var deals = new JArray();

                for (int i = 0; i < data.Rows; i++)
                {
                    var cardToTrade = GetCards.GetCard(Guid.Parse(data["card_to_trade"][i]!));

                    if (cardToTrade == null) return HttpResponse.InternalServerError;

                    var dealJson = new JObject
                    {
                        { "Id", data["deal_id"][i] },
                        { "Offer", JObject.FromObject(cardToTrade!) },
                        { "Wanted_type", data["wanted_type"][i] },
                        { "Wanted_element", data["wanted_element"][i] },
                        { "Wanted_min_damage", data["wanted_min_damage"][i] }
                    };

                    deals.Add(dealJson);
                }

                var response = HttpResponse.Ok.WithStatusMessage("Success! Shop currently has " + data.Rows + " open trades.");

                response.Body.Add("Tradingdeals", deals);

                return response;
            }
            else
                return HttpResponse.InternalServerError;
        }

        private DatabaseResponse GetAllTrades()
        {
            var request = new DatabaseRequest("SELECT * FROM tradingdeals");

            return request.PerformQuery();
        }
    }
}
