namespace Tests;
using BirdWatching;

[TestClass]
public sealed class BirdTest
{
    [DataTestMethod]
    [DataRow("Sykora", "Konadra")]
    // not good test, relies on external things, tests too many things.
    // just for the sake of testing
    public void ValidBirdInfo_InitEmpty_CorrectBird(string genus, string species)
    {
        string expectedName = $"{genus} {species}";

        Bird actual = new Bird(genus, species);

        string actualName = actual.ToString();

        Assert.AreEqual(expectedName, actualName);
    }
}
