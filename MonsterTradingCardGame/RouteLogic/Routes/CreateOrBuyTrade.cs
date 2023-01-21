using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class CreateOrBuyTrade : Route
    {
        public Guid DealId;
        public Guid CardToTradeId;
        public string? WantedType;
        public string? WantedElement;
        public int? WantedMinimumDamage;

        public CreateOrBuyTrade()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            // Get user_id associated with auth token
            int userId = UserManager.Instance.GetUserId(AuthToken);

            var cardToTrade = GetCards.GetCard(CardToTradeId);
            
            // Make sure the card is possible to be traded
            if (!CardPossibleToTrade(cardToTrade, userId))
                return HttpResponse.BadRequest.WithStatusMessage("Error! The card you offered is one or more of the following: Not owned by you | Non existent | Offered in shop | Part of your deck");

            // If a type is given, we want to create a new trade
            if (WantedType != null)
            {
                return CreateTrade(DealId, CardToTradeId, WantedType, WantedElement, WantedMinimumDamage) ? 
                    HttpResponse.Created.WithStatusMessage("Card is now offered in shop.") : 
                    HttpResponse.InternalServerError.WithStatusMessage("Error! Make sure the trade id is unique and try again.");
            }
            // Otherwise we want to buy the given trade
            else
            {
                // Get all trade info
                var tradeData = GetTrade(DealId);

                if (tradeData.ErrorMessage == null && tradeData.Rows > 0)
                {
                    // Get owner id of card in shop
                    var ownerOfCardInShop = GetCardOwner(Guid.Parse(tradeData["card_to_trade"][0]!));

                    if (ownerOfCardInShop == -1) return HttpResponse.InternalServerError;

                    // Make sure we are not the owner ourselves (trading with yourself is forbidden)
                    if (userId == ownerOfCardInShop) return HttpResponse.Forbidden.WithStatusMessage("Error! Trading with yourself is not allowed.");

                    // Next make sure card to trade matches criteria
                    WantedType = tradeData["wanted_type"][0]!;
                    WantedElement = tradeData["wanted_element"][0];

                    if(tradeData["wanted_min_damage"][0] != null)
                        WantedMinimumDamage = int.Parse(tradeData["wanted_min_damage"][0]!);

                    if (!CardMatchesCriteria(cardToTrade!, (Card.Type) Card.ParseType(WantedType)!, Card.ParseElement(WantedElement), WantedMinimumDamage))
                        return HttpResponse.BadRequest.WithStatusMessage("Error! Card does not match wanted criteria.");

                    // If trade is valid, change owner of our card to user who created trade
                    if (!ChangeCardOwnerTo(cardToTrade!.Id, ownerOfCardInShop))
                        return HttpResponse.InternalServerError;

                    // Change owner of card in shop to us
                    if(!ChangeCardOwnerTo(Guid.Parse(tradeData["card_to_trade"][0]!), userId))
                        return HttpResponse.InternalServerError;

                    // Remove the trade from the shop
                    if(!DeleteTrade.RemoveTrade(DealId)) 
                        return HttpResponse.InternalServerError;

                    return HttpResponse.Ok.WithStatusMessage("Cards successfully traded!");
                }
                // If we had an error or there was no trade found, return not found
                else
                    return HttpResponse.NotFound.WithStatusMessage("Error! The trade you requested was not found.");
            }
        }

        public override bool AddData(JObject data)
        {
            // If a locationParam was provided, it was a tradeID and we want to buy that trade
            if (data.ContainsKey("LocationParams"))
            {
                DealId = Guid.Parse(data["LocationParams"]!.ToString());

                if (!data.ContainsKey("String")) return false;

                CardToTradeId = Guid.Parse(data["String"]!.ToString());
            }
            // Otherwise we want to add a trade offer
            else
            {
                if (!data.ContainsKey("Id") || !data.ContainsKey("CardToTrade") || !data.ContainsKey("Type"))
                    return false;

                DealId = Guid.Parse(data["Id"]!.ToString());
                CardToTradeId = Guid.Parse(data["CardToTrade"]!.ToString());
                WantedType = data["Type"]!.ToString();
                
                if (data.ContainsKey("Element"))
                    WantedElement = data["Element"]!.ToString();

                if (data.ContainsKey("MinimumDamage"))
                    WantedMinimumDamage = data["MinimumDamage"]!.Value<int>();

                // At least damage or element needs to be given, both cannot be null
                if (WantedMinimumDamage == null && WantedElement == null)
                    return false;
            }

            return true;
        }

        private bool CardPossibleToTrade(Card? cardToTrade, int userId)
        {
            // Check if card is not null, it is not in the users deck, it belongs to the user, and it is not in the shop
            return cardToTrade is { PosInDeck: 0 } &&
                   ConfigureDeck.CardBelongsToUser(cardToTrade.Id, userId) &&
                   !ConfigureDeck.CardIsInShop(cardToTrade.Id);
        }
        
        private bool CardMatchesCriteria(Card cardToTrade, Card.Type type, Card.Element? element, int? damage)
        {
            // If types don't match
            if (type != cardToTrade.GetCardType())
            {
                // If we want a monster (type is OTHER), and the card is a spell, return false
                // Otherwise it's fine, since every other card is a monster
                if (type == Card.Type.Monster && cardToTrade.GetCardType() == Card.Type.Spell)
                    return false;
            }

            // If we want a specific element and they don't match, return false
            if (element != null && cardToTrade.GetElement() != element)
                return false;

            // If we want a specific min damage, make sure damage is not lower than what we want
            if (damage != null && cardToTrade.Damage < damage)
                return false;

            return true;
        }

        private int GetCardOwner(Guid cardId)
        {
            var request = new DatabaseRequest("SELECT owner FROM cards WHERE card_id = @card_id");

            request.Data.Add("card_id", cardId);

            var data = request.PerformQuery();

            if(data.ErrorMessage == null)
                return int.Parse(data["owner"][0]!);

            return -1;
        }

        private bool ChangeCardOwnerTo(Guid cardId, int newOwnerId)
        {
            var request = new DatabaseRequest("UPDATE cards SET owner = @new_owner WHERE card_id = @card_id");

            request.Data.Add("new_owner", newOwnerId);
            request.Data.Add("card_id", cardId);

            return request.PerformQuery().ErrorMessage == null;
        }

        private DatabaseResponse GetTrade(Guid tradeId)
        {
            var request = new DatabaseRequest("SELECT * FROM tradingdeals WHERE deal_id = @deal_id");

            request.Data.Add("deal_id", tradeId);

            return request.PerformQuery();
        }

        private bool CreateTrade(Guid tradeId, Guid cardToTradeId, string wantedType, string? wantedElement, int? wantedMinDamage)
        {
            var request = new DatabaseRequest("INSERT INTO tradingdeals (deal_id, card_to_trade, wanted_type, wanted_element, wanted_min_damage) VALUES(@deal_id, @card, @type, @element, @damage)");

            request.Data.Add("deal_id", tradeId);
            request.Data.Add("card", cardToTradeId);
            request.Data.Add("type", wantedType);
            request.Data.Add("element", wantedElement ?? "");
            request.Data.Add("damage", wantedMinDamage ?? 0);

            return request.PerformQuery().ErrorMessage == null;
        }
    }
}
