using System.Xml;
using Verse;

namespace DynamicEconomy;
// xml stuff

public class BaseThingPriceMultipilerInfo
{
    public float buyMultiplier = 1f;
    public float sellMultiplier = 1f;
    public string thingDefName = "";

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        if (xmlRoot.ChildNodes.Count != 1)
        {
            Log.Error($"Biome thing price multipiler error at {xmlRoot.OuterXml}");
            return;
        }

        thingDefName = xmlRoot.Name;
        buyMultiplier = ParseHelper.ParseFloat(xmlRoot.FirstChild.Value);
        sellMultiplier = xmlRoot.Attributes is { Count: > 0 }
            ? ParseHelper.ParseFloat(xmlRoot.Attributes["Sell"].Value)
            : buyMultiplier;
    }
}