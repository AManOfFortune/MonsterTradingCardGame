using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class DeleteTrade : Route
    {
        public Guid TradeToDelete;

        public DeleteTrade()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            // Get the user_id associated with the authorization token
            int userId = UserManager.Instance.GetUserId(AuthToken);

            // Get owner of card to trade (eg the user who created the trade)
            int ownerId = GetTradeOwner(TradeToDelete);

            if (ownerId == -1) return HttpResponse.InternalServerError;

            // Make sure we are the owner, otherwise we are not permitted to delete the trade
            if (ownerId != userId) return HttpResponse.Forbidden.WithStatusMessage("Error! You cannot delete trades of other players.");

            // Remove the trade
            if (RemoveTrade(TradeToDelete))
                return HttpResponse.Ok.WithStatusMessage("Successfully deleted!");

            return HttpResponse.InternalServerError;
        }

        public override bool AddData(JObject data)
        {
            if (!data.ContainsKey("LocationParams"))
                return false;

            TradeToDelete = Guid.Parse(data["LocationParams"]!.ToString());

            return true;
        }

        public static bool RemoveTrade(Guid tradeId)
        {
            var request = new DatabaseRequest("DELETE FROM tradingdeals WHERE deal_id = @deal_id");

            request.Data.Add("deal_id", tradeId);

            return request.PerformQuery().ErrorMessage == null;
        }

        private int GetTradeOwner(Guid tradeId)
        {
            var request = new DatabaseRequest("SELECT cards.owner FROM tradingdeals JOIN cards ON tradingdeals.card_to_trade = cards.card_id WHERE tradingdeals.deal_id = @trade_id");

            request.Data.Add("trade_id", tradeId);

            var data = request.PerformQuery();

            if(data.ErrorMessage == null)
            {
                return int.Parse(data["owner"][0]!);
            }

            return -1;
        }
    }
}
