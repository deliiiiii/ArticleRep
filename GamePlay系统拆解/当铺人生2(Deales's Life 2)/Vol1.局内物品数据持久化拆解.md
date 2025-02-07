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

（一句碎碎念：白天在公司上班写联机游戏的断线重连，服务器那边发的数据包的大小当然是越小越好。第一次写，没有区分开哪些需要存哪些不需要存，很头大。后来一想断线重连不就是高配版的数据持久化么，于是——就有了写这篇拆解的想法。今，我将亲自当一次游戏服务器开发！bushi）

# 局内系统

## 物品-柜台上买家视角

![image-20250207231526935](https://raw.githubusercontent.com/deliiiiii/PictureRep/main/24/9/image-20250207231526935.png)

（自然语言描述版）作为买家时点击柜台上的物品。

- 左边是贴图，背景是错落显示的物品名

- 右侧上方1-3句物品描述，i图标点击后显示关于物品的趣闻

- 右侧中下方是稀有度、状态、类别、估价

- 右侧最下方有物品大图标标签：黄色-未获得、橙色-典当、紫色-失窃、红色-确认为假货，按钮：专家是否已查验（未查验图标且有查验次数亮，否则不亮）。<u>买物品时一定不会出现“已付”</u>

（简记版）

- 贴图iii（→物品名），趣闻iii（→物品名）

- 物品描述（vvv生产的vvv，属于vvv，与vvv有关），稀有度vvv，状态vvv，类别iii（→物品名），估价vvv
- 大图标标签：未获得iii（→物品名）、典当vvv、确认为假货vvv，是否已查验vvv。<u>这里的大图标标签比较特殊，既包含可变属性vvv又包含推断属性，因此整个List按可变属性vvv计</u>（算是亏待了一点存档数据大小的折中）

（C#版）

```c#
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
enum ItemBigAttributeType
{
    NotGainedEver,
    Pawned,
    Stolen,
    Fake,
}
struct ItemStringAttribute
{
    ItemStringAttributeType itemStringAttributeType;
    string detail;
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
    bool estimatedByExpert;
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
}
```



## 物品-柜台上卖家视角

![image-20250207233430423](https://raw.githubusercontent.com/deliiiiii/PictureRep/main/24/9/image-20250207233430423.png)

（自然语言描述版）相比于买家视角没有什么新增的，除了“已付”。

- 已付：一般情况下是会出现已付的，但<u>通过各种渠道白嫖来的物品，“已付”不会出现，卖掉时不会显示利润比率</u>

（简写版）

- 已付vvv

（C#版）只需在ItemData中加上已付即可。

```c#
struct ItemData
{
    ...
    long? paidPrice;
    ...
}
```



## 物品-仓库内视角

![屏幕截图 2025-02-04 210806 ](D:\SAVE\Assignment\Semester5\aa-Memo\AAA珍宝要术\当铺人生2\屏幕截图 2025-02-04 210806 .jpg)

（变成复制品后）

![image-20250208004351546](https://raw.githubusercontent.com/deliiiiii/PictureRep/main/24/9/image-20250208004351546.png)

（自然语言描述版）

- 右侧区域中，左上角：删除物品可以忽略；中上：显示物体详情，没有专家鉴定和早晨可选的所有操作按钮，可以忽略；右上：一则关于稀有度的UI提示，可以忽略；中下的大图标标签：紫色花瓶-被专家鉴定过、红色面具-假货、绿色面具-假货复制品、红色蒙眼脑袋-失窃、黄色钱袋-典当。

早晨开业前我们可以对物品进行任何可行的操作，开业期间只能进行一部分操作。操作后有这些状态：是否锁定不允许卖、正在被专家鉴定、正在被修复师修复、是否是复制品、销毁并设置为个人收藏（TODO，在外围系统中再描述）。

<u>锁定、正在被鉴定、正在被修复这几个状态是互斥显示在贴图左上角的，因此需要定义一种数据结构表示这种不可出售的状态。</u><u>**但是！！**</u><u>原游戏中典当也是一种不可出售状态，却没有显示在左上角。在下面的拆解里，我将把小图标标签中的黄色钱袋-典当移到左上角进行存储和表达。</u>

我此处的数据仍和上文中的数据结构融在一块，所以接下来也只描述新增的数据。

（简记版）

- 被占用不允许卖vvv（锁定、典当、正在被鉴定、正在被修复）

- 右侧下方大图标标签：跟小图标标签几乎一致，只不过少一个蓝色星-超棒。

- 贴图右上角小图标标记：蓝色星-状态超棒iii（→itemData.itemCondition）、紫色花瓶-被专家鉴定过iii（从大标签读）、红色面具-假货iii（从大标签读）、绿色面具-假货复制品iii（从大标签读）、红色蒙眼脑袋-失窃iii（从大标签读）、黄色钱袋-典当iii（从大标签读）。

（C#版）所有代码如下。

```c#
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
struct ItemStringAttribute
{
    ItemStringAttributeType itemStringAttributeType;
    string detail;
}
struct ItemOccupyInfo
{
    ItemOccupyType itemOccupyType;
    int? remainDays;
    Person? occupier;
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
    bool estimatedByExpert;
    ItemOccupyInfo itemOccupyInfo;
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
```



# 下节预告 Preview

局外系统：物品目录、最大买到、最大卖出、个人收藏

### 物品目录

物品目录包含若干**物品种类**（文化、服饰、家具、武器、休闲），同一**物品种类**中的所有物品Item的**名字**、**贴图**各不相同，但是同一种类中多个物体的**描述**可以相同。

