using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.StateLogic;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class BuyPackage : Route
    {
        public BuyPackage()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            // Get the user_id associated with the authorization token
            int userId = UserManager.Instance.GetUserId(AuthToken);

            // Get the pack_id of the first non-bought pack
            int packId = GetFirstAvailablePackId();

            // If pack_id is -1, it means there is no non-bought pack so we must return
            if (packId == -1) return HttpResponse.NotFound.WithStatusMessage("There is no pack currently available. Try again later.");

            // The user's coins get reduced by 5
            // If that did not work, it means the user does not have 5 coins so we return
            if (!ReduceUserMoneyBy(userId, 5)) return HttpResponse.BadRequest.WithStatusMessage("You do not have enough coins to buy a package!");

            // "Buy" the package by changing the packs table
            // If that did not work return internal server error
            if (!SetPackToBought(packId)) return HttpResponse.InternalServerError;

            // Change the owner of the cards associated with the pack_id of the bought pack
            if(ChangePackageCardsOwnerTo(packId, userId).ErrorMessage != null) return HttpResponse.InternalServerError;

            return HttpResponse.Ok.WithStatusMessage("Package bought!");
        }

        private int GetFirstAvailablePackId()
        {
            var request = new DatabaseRequest("SELECT pack_id FROM packs WHERE bought = 'false' LIMIT 1");

            var response = request.PerformQuery();

            if (response.Rows > 0)
                return int.Parse(response["pack_id"][0]!);
            return -1;
        }

        private bool SetPackToBought(int packId)
        {
            var request = new DatabaseRequest("UPDATE packs SET bought = 'true' WHERE pack_id = @packageID");

            request.Data.Add("packageID", packId);

            var response = request.PerformQuery();

            if (response.ErrorMessage == null)
                return true;
            return false;
        }

        private bool ReduceUserMoneyBy(int userId, int reduceBy)
        {
            var request = new DatabaseRequest("UPDATE users SET coins = coins - " + reduceBy + " WHERE user_id = @userID");

            request.Data.Add("userID", userId);

            var response = request.PerformQuery();

            if (response.ErrorMessage == null)
                return true;
            return false;
        }

        private DatabaseResponse ChangePackageCardsOwnerTo(int packageId, int newOwnerId)
        {
            var request = new DatabaseRequest("UPDATE cards SET owner = @newOwnerID WHERE pack_id = @packID");

            request.Data.Add("newOwnerID", newOwnerId);
            request.Data.Add("packID", packageId);

            var response = request.PerformQuery();

            return response;
        }
    }
}
