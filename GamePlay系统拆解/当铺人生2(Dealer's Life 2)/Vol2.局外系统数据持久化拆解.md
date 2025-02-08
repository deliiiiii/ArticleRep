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

# 局外系统

## 物品目录

![image-20250208215258836](https://raw.githubusercontent.com/deliiiiii/PictureRep/main/24/9/image-20250208215258836.png)

（自然语言描述版）

- 物品目录包含若干物品种类（文化、服饰、家具、武器、休闲），同一物品种类中的所有物品Item的名字、贴图各不相同。点击物品贴图后，显示物品名称和物品描述，同一种类中多个物体的描述可以相同。

~~这里插播一段心路历程：物品的“已获得”在上一篇拆解（Vol1.局内系统数据持久化拆解.md）中并没有保存，且大图标标签我怎么打的是iii（→物品名）？？当时应该是觉得用一个额外的字典存，太傻了~~

![image-20250208220305584](https://raw.githubusercontent.com/deliiiiii/PictureRep/main/24/9/image-20250208220305584.png)

（简记版）

~~心路历程·续：原来如此！当时没有存“已获得”，是因为没有意识到外围系统对每一个道具都要保存。。好了中划线的快捷键越用越熟悉了~~

- 新建一个结构体，存储名字ccc（可以是遍历一个表格）和是否已获得vvv。

（C#版）

新增一个结构体ItemShowCase，不长，故稍后再一并列出。

## 最贵售出

![image-20250208222037845](https://raw.githubusercontent.com/deliiiiii/PictureRep/main/24/9/image-20250208222037845.png)

（自然语言描述版）

- 会列出历史最高5个卖出价格。灭霸手套大卖特卖，每次遇到必买的。点击物品贴图后，显示物品详情，相比于卖家视角，只多了一个“已赚”属性，这里显示的是售出时的柜台状态，因此很明显能沿用Vol1中设计的ItemData

- 中下方显示所有物品售价的总和

（简记版）

- 物品已赚vvv

- 至多5个Vol1中提到的ItemData（vvv）

- 贴图iii（→道具名），种类iii（道具名）

- 售价总和iii（→各物品已赚）

（C#版）

ItemData中新增“已赚”，稍后再一并列出。

## 最贵购入

![image-20250208223908874](https://raw.githubusercontent.com/deliiiiii/PictureRep/main/24/9/image-20250208223908874.png)

（自然语言描述版）

- 数据跟“最贵售出”几乎一致，需要关注的是，这里显示的是购入时的柜台状态，不会显示大图标标签“未获得”

~~心路历程·续续：这里想的是买入时，使用外围系统的set，将已获得设置为true；退出到主界面进入外围系统时，使用外围系统的get，拿到的一定是已获得。这样一来就不会出现在外围系统中显示“未获得”的bug~~

（简记版）

跟“最贵售出”几乎一致，略

（C#版）

跟“最贵售出”几乎一致，稍后再一并列出

## 个人收藏

（自然语言描述版）

- 数据跟“最贵购入”几乎一致，需要注意的是，中下方的总和是物品的估价的和

（简记版）

- 若干Vol1中提到的ItemData（vvv）

- 贴图iii（→道具名），种类iii（道具名）

- 估价总和iii（→各物品估价）

（C#版）

这里将本文的四个外围系统新增的数据结构一并列出

~~（见下图的Commit修改，划掉）~~

![image-20250208225031521](https://raw.githubusercontent.com/deliiiiii/PictureRep/main/24/9/image-20250208225031521.png)

```c#
struct ItemData
{
	...
    long? earnedPrice;
    ...
}

...

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

至此，《当铺人生2(Dealer's Life 2)》关于物品的局内、局外系统的数据持久化拆解完成。



# 下节预告 Preview

ItemData中的ItemOccupyInfo引用了占用者Person，但Person类中会存储这个人正在操作的物品，这样会导致持久化数据的循环引用...