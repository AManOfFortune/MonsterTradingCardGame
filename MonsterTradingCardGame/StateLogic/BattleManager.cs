using MonsterTradingCardGame.DatabaseLogic.DataModels;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.StateLogic
{
    internal class BattleManager
    {
        private static readonly Lazy<BattleManager> Lazy = new(() => new BattleManager());

        public static BattleManager Instance { get { return Lazy.Value; } }

        // { UserIDLookingForBattle: Tuple<User_Elo, FightReport (or null if no opponent was fought yet)> }
        private Dictionary<int, (int, JObject?)> _battleReadyUserIDs = new();

        public void SetUserReadyForBattle(int userId, int userElo)
        {
            if(!_battleReadyUserIDs.ContainsKey(userId))
                _battleReadyUserIDs.Add(userId, (userElo, null));
        }

        public void RemoveBattleReadyUser(int userId)
        {
            if (_battleReadyUserIDs.ContainsKey(userId))
                _battleReadyUserIDs.Remove(userId);
        }

        public JObject? GetFightReport(int userId)
        {
            return _battleReadyUserIDs[userId].Item2;
        }

        public void SetFightReportOf(int userId, JObject fightReport)
        {
            _battleReadyUserIDs[userId] = (_battleReadyUserIDs[userId].Item1, fightReport);
        }

        public int GetEligibleOpponent(int userId, int userElo)
        {
            const int eloRange = 20;

            foreach(var battleReadyUser in _battleReadyUserIDs)
            {
                int currentUserId = battleReadyUser.Key;
                int currentUserElo = battleReadyUser.Value.Item1;
                JObject? currentUserFightReport = battleReadyUser.Value.Item2;

                // Make sure we are not looking at the same user
                if(currentUserId != userId)
                {
                    // Make sure the current user does not already have a finished fight
                    if (currentUserFightReport == null)
                    {
                        // Compare our user's elo with the current user
                        // If their elo's are within <eloRange> of each other, the opponent is eligible
                        if (currentUserElo + eloRange >= userElo && currentUserElo - eloRange <= userElo)
                        {
                            return currentUserId;
                        }
                    }
                }
            }

            return -1;
        }
    }
}
