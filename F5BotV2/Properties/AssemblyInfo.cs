using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("F5BotV3")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("F5BotV3")]
[assembly: AssemblyCopyright("Copyright © Microsoft 2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("221ebd80-7b7c-459d-a094-80a00acda949")]

// 程序集的版本信息由下列四个值组成: 
//
//      主版本
//      次版本
//      生成号
//      修订号
//
//可以指定所有这些值，也可以使用“生成号”和“修订号”的默认值
//通过使用 "*"，如下所示:
//公开版本
//[assembly: AssemblyVersion("5.1.17.0")]
/*
 *  W3.exe
 * 3.5.0
 *    修复微信重复数据发送问题, 接收数据做hashcode, 过滤掉重复的数据
 *    添加新口子, S880..
 * 3.7.1
 *    增加ADK口子支持
 * 3.9.2
 *    修复删除数据，最后一条不能删除的BUG
 *    修复补分操作
 * 3.9.6
 *    增加果然口子
 *   3.9.7
 *      0619增加适配平台
 */

//---------------------------------------------------------------------------------------------
//内部版本
[assembly: AssemblyFileVersion("6.9.9")]
[assembly: AssemblyVersion("6.9.9")]


//W3用的
/*  最新的放上面
 *  6.9.8.0
 *      添加hy.168网站支持
 *      
 *  6.8.8
 *      修正中奖名单, 跟封盘订单不一致的问题，小数点进位问题。
 *  6.8.5
 *      添加新口子
 *      
 *  6.8.3
 *      修复下分BUG.
 *      如果没客人用下命令，管理直接下分，那么会导致显示额度不够, 又再次显示下分成功的，余额多少的问题。
 *  6.8.1
 *      修复第二次打开挂的时候, 今日盈利计算不正确的BUG
 *  6.7.11
 *      还有BUG，需要继续修复，重新打开订单时候, 计算有问题。
 *      
 * 6.7.2.0
 *    增加果然口子
 *    
 * 6.3.5
 *     增加管理指令: 刷新   //用于群新会员进来后，不识别，可以手动刷新
 * 6.3.10
 *     修复微信重复数据发送问题, 接收数据做hashcode, 过滤掉重复的数据
 * 6.3.11
 *     增加Bet方法的Debug日志输出,排查S880问题
 * 6.3.11
 *     托结算显示错误。显示成盘外。
 * 6.3.18
 *     Q主管理功能
 * 6.3.19
 *    未完成，正在开发中
 *    6.3.21
 *      新增加QT支持
 *      修改订单创建, 结算流程, 去除冗余代码
 * 6.3.27
 *    -0627
 *    -修复补分订单消失的BUG。
 * 6.6.30
 *    -增加设置订单状态
 *    
 *   6.5.1
 *   修正合单何霜BUG
 */
//-----------------------------------------------------------------------------------------------------------------
//[assembly: AssemblyFileVersion("5.8.0")]
//[assembly: AssemblyVersion("5.2.7")]
// 
// W3 版本
// *3.82.* 增加伪 3.9.10.16 版本微信的支持, 协议有变化, 会员进群和退群是一个ID, 通过阿凡达参数区分
// W3.5.2.7
//新增加口子

