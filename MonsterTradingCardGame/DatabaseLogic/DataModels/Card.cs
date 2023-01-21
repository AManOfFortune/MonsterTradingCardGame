using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.DatabaseLogic.DataModels
{
    public class Card
    {
        public Guid Id;
        public string Name;
        public float Damage;
        public int PosInDeck { get; set; }

        public enum Element
        {
            Water,
            Fire,
            Normal
        }

        public enum Type
        {
            Goblin,
            Dragon,
            Wizard,
            Ork,
            Knight,
            Kraken,
            Elf,
            Spell,
            Monster,
            Other
        }

        public Card()
        {
            Id = Guid.NewGuid();
            Name = "";
            Damage = 0;
            PosInDeck = 0;
        }

        public Card(Guid id, string name, float damage, int posInDeck)
        {
            Id = id;
            Name = name;
            Damage = damage;
            PosInDeck = posInDeck;
        }

        public Element GetElement()
        {
            var nameSplitOnCapitalLetters = Regex.Split(Name, "(?<=[a-z])(?=[A-Z])");

            string element = "";

            // If name does not have 2 words, it means the element is not given, meaning an empty string gets passed to the Parse function
            // which then returns Element.NORMAL
            if(nameSplitOnCapitalLetters.Length > 1)
                element = nameSplitOnCapitalLetters[0];

            return (Element)ParseElement(element)!;
        }

        public Type GetCardType()
        {
            var nameSplitOnCapitalLetters = Regex.Split(Name, "(?<=[a-z])(?=[A-Z])");

            string type = nameSplitOnCapitalLetters[0];

            // If name has more than 2 words, it means an element is given and index 1 is the type
            if (nameSplitOnCapitalLetters.Length > 1)
                type = nameSplitOnCapitalLetters[1];

            return (Type)ParseType(type)!;
        }

        public static Element? ParseElement(string? element)
        {
            if (element == null)
                return null;

            element = element.ToLower();

            return element switch
            {
                "fire" => Element.Fire,
                "water" => Element.Water,
                _ => Element.Normal,
            };
        }

        public static Type? ParseType(string? type)
        {
            if (type == null)
                return null;

            type = type.ToLower();

            return type switch
            {
                "goblin" => Type.Goblin,
                "dragon" => Type.Dragon,
                "wizard" => Type.Wizard,
                "ork" => Type.Ork,
                "knight" => Type.Knight,
                "kraken" => Type.Kraken,
                "elf" => Type.Elf,
                "spell" => Type.Spell,
                "monster" => Type.Monster,
                _ => Type.Other
            };
        }
    }
}
