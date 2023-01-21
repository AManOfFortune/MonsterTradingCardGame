using NUnit.Framework;
using MonsterTradingCardGame.DatabaseLogic.DataModels;

namespace Testing;

[TestFixture]
public class CardsTests
{
    [Test]
    public void TestTwoWordCardElement()
    {
        var card = new Card(new Guid(), "WaterGoblin", 15, 0);
        
        Assert.AreEqual(card.GetElement(), Card.Element.Water);
    }
    
    [Test]
    public void TestOneWordCardElement()
    {
        var card = new Card(new Guid(), "Goblin", 15, 0);
        
        Assert.AreEqual(card.GetElement(), Card.Element.Normal);
    }
    
    [Test]
    public void TestInvalidCardElement()
    {
        var card = new Card(new Guid(), "StrongGoblin", 15, 0);
        
        Assert.AreEqual(card.GetElement(), Card.Element.Normal);
    }
    
    [Test]
    public void TestTwoWordCardType()
    {
        var card = new Card(new Guid(), "WaterGoblin", 15, 0);
        
        Assert.AreEqual(card.GetCardType(), Card.Type.Goblin);
    }

    [Test]
    public void TestOneWordCardType()
    {
        var card = new Card(new Guid(), "Goblin", 15, 0);
        
        Assert.AreEqual(card.GetCardType(), Card.Type.Goblin);
    }
    
    [Test]
    public void TestInvalidCardType()
    {
        var card = new Card(new Guid(), "RegularGargoyle", 15, 0);
        
        Assert.AreEqual(card.GetCardType(), Card.Type.Other);
    }
}