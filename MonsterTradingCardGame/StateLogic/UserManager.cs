using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.RouteLogic.Routes;

namespace MonsterTradingCardGame.StateLogic
{
    public class UserManager
    {
        private static readonly Lazy<UserManager> Lazy = new(() => new UserManager());

        public static UserManager Instance { get { return Lazy.Value; } }

        // { UserToken: UserID }
        private Dictionary<string, int> _userTokenUserIdMap = new();

        public bool NewUserLogin(string userToken, int userId)
        {
            if(IsUserLoggedIn(userToken))
                return false;

            _userTokenUserIdMap.Add(userToken, userId);
            return true;
        }

        public bool IsUserLoggedIn(string userToken)
        {
            return _userTokenUserIdMap.ContainsKey(userToken);
        }

        public bool DoesUserHavePermission(string userToken, User.UserRoles requiredLevel)
        {
            return GetUser(userToken).Role >= requiredLevel;
        }

        public User GetUser(string userToken)
        {
            return GetUserData.GetAllUserData(_userTokenUserIdMap[userToken])!;
        }

        public int GetUserId(string userToken)
        {
            return _userTokenUserIdMap[userToken];
        }

        public bool LogoutUser(string userToken)
        {
            if (!IsUserLoggedIn(userToken))
                return false;

            _userTokenUserIdMap.Remove(userToken);
            return true;
        }
    }
}
