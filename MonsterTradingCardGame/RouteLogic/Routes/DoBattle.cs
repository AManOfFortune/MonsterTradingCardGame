using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class DoBattle : Route
    {
        private List<BattleRule> SpecialRules { get; }
        private List<BattleRule> ElementRules { get; }

        public DoBattle()
        {
            AuthLevelRequired = User.UserRoles.Normal;
            SpecialRules = new();
            ElementRules = new();
        }

        public override HttpResponse Call()
        {
            // Get the user associated with the authorization token
            var user = UserManager.Instance.GetUser(AuthToken);

            int userId = user.Id;

            // Get user deck
            var userDeck = GetDeck.GetDeckOfUser(userId);

            // If there was an error or the user has no deck, return bad request
            if (userDeck == null || userDeck.Count <= 0)
                return HttpResponse.BadRequest.WithStatusMessage("Error! You do not have configured a deck!");

            // Set user ready for battle
            BattleManager.Instance.SetUserReadyForBattle(userId, user.Elo);

            // See if our user already has a finished fight
            var fightReport = BattleManager.Instance.GetFightReport(userId);

            // If current user does not have a finished fight, look for an opponent, and depending if we found one, fight or return "waiting".
            // If fightReport is not null, we have been found by another user, and they calculated the fight already
            // All we then have to do is to remove us from the looking-for-battle-list and send the report. All other logic has already been done by our opponent.
            if (fightReport == null)
            {
                // Get an eligible opponent
                var opponentId = BattleManager.Instance.GetEligibleOpponent(userId, user.Elo);

                // If no eligible opponent was found
                if(opponentId == -1)
                {
                    var waitingResponse = HttpResponse.Ok.WithStatusMessage("Searching for an opponent...");
                    waitingResponse.KeepConnectionAlive = true;

                    return waitingResponse;
                }
                // Otherwise we found an eligible opponent
                else
                {
                    var opponent = GetUserData.GetAllUserData(opponentId)!;

                    // Fill battle rules
                    FillRules();

                    // Get our fight report
                    fightReport = GetFightReport(user, opponent);

                    // Save the fight report of our opponent (= create the fight report from our opponent's perspective)
                    BattleManager.Instance.SetFightReportOf(opponentId, GetFightReport(opponent, user));

                    // Depending on who won, save the stat changes
                    // When the winner is "Draw", do nothing
                    switch (fightReport["Winner"]!.ToString())
                    {
                        case "You" when !SaveFightWin(userId) || !SaveFightLoss(opponentId):
                        case "Opponent" when !SaveFightWin(opponentId) || !SaveFightLoss(userId):
                            // If somehow an error saving wins and losses occured, return server error
                            return HttpResponse.InternalServerError;
                    }
                }
            }

            // Remove us from the looking for battle list
            BattleManager.Instance.RemoveBattleReadyUser(userId);

            // Return the fight response
            var response = HttpResponse.Ok.WithStatusMessage("Opponent found!");
            response.Body = fightReport!;
            return response;
        }

        private JObject GetFightReport(User user, User opponent)
        {
            var userDeck = GetDeck.GetDeckOfUser(user.Id);
            var opponentDeck = GetDeck.GetDeckOfUser(opponent.Id);

            int currentUserCardIndex = 0;
            int currentOpponentCardIndex = 0;

            var fightlog = new JArray();
            string winner = "Draw";
            int roundNumber;

            // Do fight rounds
            // Number of rounds limited to 100, if reached then winner is "Draw"
            for (roundNumber = 0; roundNumber < 100; roundNumber++)
            {
                Card userCard = userDeck[currentUserCardIndex];
                Card opponentCard = opponentDeck[currentOpponentCardIndex];

                // Get and save fight round json
                var roundJson = GetRoundReport(userCard, opponentCard);
                roundJson.Add("Round", roundNumber + 1);
                fightlog.Add(roundJson);

                // Depending on who won the round, switch cards
                if(roundJson["Winner"]!.ToString() == "You")
                {
                    userDeck.Add(opponentCard);
                    opponentDeck.Remove(opponentCard);
                }
                else if (roundJson["Winner"]!.ToString() == "Opponent")
                {
                    opponentDeck.Add(userCard);
                    userDeck.Remove(userCard);
                }

                // Check if someone won the full fight
                // If someone won, save winner and exit loop
                if(userDeck.Count <= 0)
                {
                    winner = "Opponent";
                    break;
                }
                else if (opponentDeck.Count <= 0)
                {
                    winner = "You";
                    break;
                }
                
                // Next card
                currentUserCardIndex++;
                currentUserCardIndex %= userDeck.Count;
                
                currentOpponentCardIndex++;
                currentOpponentCardIndex %= opponentDeck.Count;
            }

            var fightReport = new JObject();

            var opponentJson = new JObject
                {
                    { "Name", opponent.Name },
                    { "Bio", opponent.Bio },
                    { "Image", opponent.Image },
                    { "Elo", opponent.Elo }
                };

            fightReport.Add("Opponent", opponentJson);
            fightReport.Add("Winner", winner);
            fightReport.Add("Number of Rounds", roundNumber);
            fightReport.Add("Fightlog", fightlog);

            return fightReport;
        }

        private JObject GetRoundReport(Card userCard, Card opponentCard)
        {
            var roundLog = new JObject
            {
                { "Your card", JObject.FromObject(userCard) },
                { "Opponent card", JObject.FromObject(opponentCard) }
            };

            // Check if any special rules apply for this match-up
            // If a special rule applies, the winner gets returned here since all other rules don't matter
            foreach(var rule in SpecialRules)
            {
                if(WinnerRuleApplies(rule, userCard) && LoserRuleApplies(rule, opponentCard))
                {
                    // User wins because of special rule
                    roundLog.Add("Winner", "You");
                    roundLog.Add("Reason", rule.GetReason(userCard.Name, opponentCard.Name));
                    return roundLog;
                }
                else if (WinnerRuleApplies(rule, opponentCard) && LoserRuleApplies(rule, userCard))
                {
                    // Opponent wins because of special rule
                    roundLog.Add("Winner", "Opponent");
                    roundLog.Add("Reason", rule.GetReason(opponentCard.Name, userCard.Name));
                    return roundLog;
                }
            }

            // If no special rule applies, damage will determine the winner
            float userCardDamage = userCard.Damage;
            float opponentCardDamage = opponentCard.Damage;

            // If a spell is involved, it means elements matter and we need to check if an element advantage applies
            if (userCard.GetCardType() == Card.Type.Spell || opponentCard.GetCardType() == Card.Type.Spell)
            {
                // Loop all element rules to check if someone has an element advantage
                foreach (var rule in ElementRules)
                {
                    if (WinnerRuleApplies(rule, userCard) && LoserRuleApplies(rule, opponentCard))
                    {
                        // User has element advantage
                        userCardDamage *= 2;
                        opponentCardDamage /= 2;
                        roundLog.Add("Advantage", rule.GetReason(userCard.Name, opponentCard.Name));
                    }
                    else if (WinnerRuleApplies(rule, opponentCard) && LoserRuleApplies(rule, userCard))
                    {
                        // Opponent has element advantage
                        opponentCardDamage *= 2;
                        userCardDamage /= 2;
                        roundLog.Add("Advantage", rule.GetReason(opponentCard.Name, userCard.Name));
                    }
                }
            }

            if (userCardDamage > opponentCardDamage)
            {
                // User wins because of damage
                roundLog.Add("Winner", "You");
                roundLog.Add("Reason", userCardDamage + " (You) > " + opponentCardDamage + " (Opponent)");
            }
            else if (opponentCardDamage > userCardDamage)
            {
                // Opponent wins because of damage
                roundLog.Add("Winner", "Opponent");
                roundLog.Add("Reason", opponentCardDamage + " (Opponent) > " + userCardDamage + " (You)");
            }
            else
            {
                // Nobody wins, damage is equal
                roundLog.Add("Winner", "Draw");
                roundLog.Add("Reason", userCardDamage + " (You) = " + opponentCardDamage + " (Opponent)");
            }

            return roundLog;
        }

        private void FillRules()
        {
            ElementRules.Add(new BattleRule(Card.Element.Water, null, Card.Element.Fire, null, "@winner is super effective against @loser!"));
            ElementRules.Add(new BattleRule(Card.Element.Fire, null, Card.Element.Normal, null, "@winner is super effective against @loser!"));
            ElementRules.Add(new BattleRule(Card.Element.Normal, null, Card.Element.Water, null, "@winner is super effective against @loser!"));

            SpecialRules.Add(new BattleRule(null, Card.Type.Dragon, null, Card.Type.Goblin, "@loser is too afraid to attack @winner and runs away!"));
            SpecialRules.Add(new BattleRule(null, Card.Type.Wizard, null, Card.Type.Ork, "@winner controlls the mind of @loser and makes him kill himself!"));
            SpecialRules.Add(new BattleRule(Card.Element.Water, Card.Type.Spell, null, Card.Type.Knight, "@loser is too heavy and @winner makes him drown!"));
            SpecialRules.Add(new BattleRule(null, Card.Type.Kraken, null, Card.Type.Spell, "@winner is immune against spells!"));
            SpecialRules.Add(new BattleRule(Card.Element.Fire, Card.Type.Elf, null, Card.Type.Dragon, "@winner knows @loser since childhood and evades his attacks!"));
        }

        private bool WinnerRuleApplies(BattleRule rule, Card card)
        {
            // Both element and type matter
            if(rule.WinnerElement != null && rule.WinnerType != null)
            {
                if (card.GetElement() == rule.WinnerElement && card.GetCardType() == rule.WinnerType)
                    return true;
            }
            // Only element matters
            else if (rule.WinnerElement != null)
            {
                if (card.GetElement() == rule.WinnerElement)
                    return true;
            }
            // Only type matters
            else
            {
                if (card.GetCardType() == rule.WinnerType)
                    return true;
            }

            return false;
        }

        private bool LoserRuleApplies(BattleRule rule, Card card)
        {
            // Both element and type matter
            if (rule.LoserElement != null && rule.LoserType != null)
            {
                if (card.GetElement() == rule.LoserElement && card.GetCardType() == rule.LoserType)
                    return true;
            }
            // Only element matters
            else if (rule.LoserElement != null)
            {
                if (card.GetElement() == rule.LoserElement)
                    return true;
            }
            // Only type matters
            else
            {
                if (card.GetCardType() == rule.LoserType)
                    return true;
            }

            return false;
        }

        private bool SaveFightWin(int userId)
        {
            var request = new DatabaseRequest("UPDATE users SET wins = wins + 1, elo = elo + 3 WHERE user_id = @user_id");

            request.Data.Add("user_id", userId);

            return request.PerformQuery().ErrorMessage == null;
        }

        private bool SaveFightLoss(int userId)
        {
            var request = new DatabaseRequest("UPDATE users SET losses = losses + 1, elo = elo - 5 WHERE user_id = @user_id");

            request.Data.Add("user_id", userId);

            return request.PerformQuery().ErrorMessage == null;
        }

        private class BattleRule
        {
            public readonly Card.Element? WinnerElement;
            public readonly Card.Type? WinnerType;
            public readonly Card.Element? LoserElement;
            public readonly Card.Type? LoserType;
            private readonly string _reason;

            public BattleRule(Card.Element? winnerElement, Card.Type? winnerType, Card.Element? loserElement, Card.Type? loserType, string reason)
            {
                this.WinnerElement = winnerElement;
                this.WinnerType = winnerType;
                this.LoserElement = loserElement;
                this.LoserType = loserType;
                this._reason = reason;
            }

            public string GetReason(string winner, string loser)
            {
                var reasonWithWinner = _reason.Replace("@winner", winner);

                return reasonWithWinner.Replace("@loser", loser);
            }
        }
    }
}
