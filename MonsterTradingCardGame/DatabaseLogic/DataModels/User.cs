using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.DatabaseLogic.DataModels
{
    public class User
    {
        public int Id;
        public string Name;
        public UserRoles Role;
        public int Elo;

        // Used only when a user wants to see his data
        public int? Coins = null;
        public string? Bio = null;
        public string? Image = null;

        public enum UserRoles
        {
            None,
            Normal,
            Admin
        }

        public User(int id, string name, string role, int elo)
        {
            Id = id;
            Name = name;
            Elo = elo;

            UserRoles userRoleEnum = UserRoles.Normal;

            if (role == "admin")
                userRoleEnum = UserRoles.Admin;

            Role = userRoleEnum;
        }
    }
}
