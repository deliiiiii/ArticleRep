```
对2款游戏中的主要系统进行归纳，**<u>程序</u>应当包含对主要系统（任选单一系统即可，不用面面俱到）实现解决方案的设想（说明系统架构和数据管线方案）和对现有框架的调研**

游戏数据库编辑器和数据管理接口

烙印组件合成与烙印镶嵌的表现和肉鸽经营玩法流程

文字冒险部分的表现和流程控制



- 无序列表：输入-之后输入空格 / ctrl + shift + ] (对选中行可用)

- 有序列表：输入数字+“.”之后输入空格 / ctrl + shift + [ (对选中行可用)

- 删除线：alt+shift+5

- 代码块：ctrl+shift+k

- 代码词：ctrl+shift+`
```





本文是对《当铺人生2(Dealer's Life 2)》的数据结构进行拆分，最终实现数据持久化即存档系统。一些简记的描述如下。

- 常量（const，写死在表格/ScriptableObject里，方便策划配置的数据）用ccc表示
- 可变属性（vary，局内会发生变化的数据）用vvv表示
- 推断属性（infer，运行时，根据前面的常量或可变属性属性进行推断和赋值，不需要写入游戏存档）用iii表示



# 解决循环引用

```c#
struct ItemOccupyInfo
{
    ItemOccupyType itemOccupyType;
    int? remainDays;
    Person? occupier;
}
struct ItemData
{
    ...
    ItemOccupyInfo itemOccupyInfo;
    ...
}

struct Person
{
    ItemData occupiedItem;
}
```

ItemData中的ItemOccupyInfo引用了占用者Person，但Person类中会存储这个人正在操作的物品ItemData，这样会导致持久化数据的循环引用...

解决方法是，将这ItemData、Person这两个类提升一层，新建一个类同时存这两个类的引用。

新增代码如下。

```c#
struct ItemOccupyData
{
    ItemData itemData;
    Person? occupier;
    ItemOccupyType itemOccupyType;
    int? remainDays;
}
```



再贴一下此时的全部数据结构。

```C#
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
```

