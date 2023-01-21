using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic
{
    public abstract class Route
    {
        public abstract HttpResponse Call();

        public User.UserRoles AuthLevelRequired { get; protected set; }
        public string AuthToken { protected get; set; }

        protected Route()
        {
            AuthLevelRequired = User.UserRoles.None;
            AuthToken = string.Empty;
        }

        public virtual bool AddData(JObject data)
        {
            foreach(var property in GetType().GetFields())
            {
                if (!data.ContainsKey(property.Name))
                    return false;

                property.SetValue(this, data[property.Name]!.ToObject(property.FieldType));
            }

            return true;
        }
    }
}
