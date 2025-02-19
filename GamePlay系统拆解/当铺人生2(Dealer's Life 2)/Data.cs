//ccc，配表
struct CSVData
{
    <string,string> itemName_to_itemType = {}
    <string,Sprite> itemName_to_itemSprite = {}
}


enum ItemRarity
{
    Normal,
    Rare,
    Epic,
    Legendary,
    Mythic,
}
enum ItemCondition
{
    Terrible,
    Bad,
    Good,
    VeryGood,
}
enum ItemStringAttributeType
{
    Belong,
    Associate,
}
struct ItemStringAttribute
{
    ItemStringAttributeType itemStringAttributeType;
    string detail;
}
enum ItemBigAttributeType
{
    NotGainedEver,
    Estimated,
    Pawned,
    Stolen,
    Fake,
    Replica,
}
enum ItemSmallAttributeType
{
    VeryGood,
    Estimated,
    Stolen,
    Fake,
    Replica,
}
enum ItemOccupyType
{
    Locked,
    Pawned,
    BeingEstimated,
    BeingRepaired,
}
struct ItemOccupyData
{
    ItemData itemData;
    Person? occupier;
    ItemOccupyType itemOccupyType;
    int? remainDays;
}
struct ItemData
{
    string bornYear;
    string name;
    List<ItemStringAttribute> itemStringAttributes;
    List<ItemBigAttributeType> itemBigAttributeTypes;
    ItemRarity itemRarity;
    ItemCondition itemCondition;
    //估价是可空long
    long? estimatedPrice;
    long? paidPrice;
    long? earnedPrice;
    bool estimatedByExpert;
}


struct Person
{
    string name;
    PersonJobType job;
}

class Item
{
    //vvv
    ItemData itamData;
    //iii 读表itemName_to_itemType
    Sprite sprite;
    //iii 读表itemName_to_itemSprite
    ItemType itemType;
    //iii 根据itemData中的bornYear、itemName、itemStringAttributes拼凑
    string itemDescription;
    //iii 根据itemData中的itemBigAttributeTypes读，加一个VeryGood特判
    List<ItemSmallAttributeType> itemSmallAttributeTypes;
}

//外围系统
struct ItemCatelogue
{
    string itemName;
    bool gainedEver;
}

struct ItemShowCase
{
    List<ItemCatelogue> itemCatelogues;
    List<ItemData> highestSales;
    List<ItemData> highestPurchases;
    List<ItemData> personalCollections;
}