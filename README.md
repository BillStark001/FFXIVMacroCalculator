# Final Fantasy XIV Macro Calculator

A simple tool to calculate the macro of operation codes used by craftsmen for a specified recipe in Final Fantasy XIV.

Currently it only supports the Simplified Chinese version. It will support other languages in the future.

The reference version of the game is 5.55. The last update date of the tool is Sep 15, 2021.

## 因为只支持中文所以用中文写说明文档没问题吧库啵

用法: 把这个仓库clone到本地，然后执行：

```main.py [-h] path```

其中path是配方文件的目录库啵。

在执行后，会根据配方文件的设置，每过一定的迭代次，输出一个带有评分最高的五个宏及其模拟的文本文件库啵。

## 配方文件格式

配方文件是json文件，其继承结构如下库啵：


* 配方名称 Name 字串 

    没有作用，只是方便使用者而已库啵（
* 玩家 Player：
	* 等级 LV 正整数 1~80
	* 制作力 CP 正整数
	* 作业精度 CM 正整数
	* 加工精度 CT 正整数
	* 掌握可用 Manipulation 布尔

        掌握不能靠升级自然取得，需要去接职能任务库啵
* （未实装）配方Recipe：
	* 格式 Format = 1

        根据各项数据算出技能效率库啵
	* 等级 LV 正整数 1~80
	* 制作难度星级 Star 正整数 0~4
	* 建议作业精度 SCM 正整数
	* 建议加工精度 SCT 正整数
	* 难度 P 正整数
	* 最大品质 Q 正整数
	* 耐久 E 正整数
* 配方 Recipe：
	* 格式 Format = 2

        直接给出技能效率库啵

        技能效率可以在配方详情页面上查看库啵
	* 工匠的神速技巧可用 TrainedEye 布尔
	
        需要配方等级比玩家等级低10级库啵
	* 制作技能效率 DP 正整数
	* 加工技能效率 DQ 正整数
	* 难度 P 正整数
	* 最大品质 Q 正整数
	* 耐久 E 正整数
* 系统 System：
	* 宏长度 MacroLength 正整数 默认15
	* 启用概率模拟 AllowProbSkills 布尔 默认false

        会出现奇奇怪怪的问题库啵
	* 启用加入hq的模拟 AllowHQ 布尔 默认false

        会慢一点库啵；会出现奇奇怪怪的问题库啵
	* （未实装）使用的算法 Algorithm 字串 默认"Genetic"
	* 算法参数 Arguments 根据不同算法不同 默认[1500, 0.15, 0.15, 0.3]
        * 遗传算法：
            * 种群个数 P 正整数 默认1500
            * 选择率 SR 浮点数 0~1 默认0.15
            * 保留率 PR 浮点数 0~1 默认0.15
            * 变异率 MR 浮点数 0~1 默认0.15

        不知道是什么意思的就别动，用默认的一般不会有问题库啵。。。
	* 最大迭代次数 MITC 正整数 默认100000
	* 输出配方间隔的迭代次数 OITC 正整数 默认20
    * 生成宏的语言 Language 字串 默认"zhCN"


在仓库里提供了一个样例配方文件args_example_1.json（品级500的白票校服搓前沿套）和一些样例输出文件，如果不明白可以参考库啵w