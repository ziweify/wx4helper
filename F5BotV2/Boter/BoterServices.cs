using CCWin.SkinClass;
using CefSharp;
using CefSharp.DevTools.Media;
using CefSharp.DevTools.Network;
using CsQuery.Engine.PseudoClassSelectors;
using CsQuery.StringScanner.Patterns;
using F5Bot.Ext;
using F5BotV2.BetSite;
using F5BotV2.BetSite.Boter;
using F5BotV2.Game.BinGou;
using F5BotV2.Main;
using F5BotV2.MainOpenLottery;
using F5BotV2.Model;
using F5BotV2.Model.BindSqlite;
using F5BotV2.Model.Setting;
using F5BotV2.Wx;
using F5BotV2.Wx.Msg;
using HPSocket;
using HPSocket.AsyncQueue;
using LxLib.LxSys;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Image = System.Drawing.Image;

namespace F5BotV2.Boter
{

    public enum BoterStatus
    {
        等待中 = 0
      , 开盘中 = 1 // -> Reply_开盘提示, 下注时间
      , 封盘中 = 2   //时间到, 停止进仓 -> Reply_回合停止
      , 开奖中 = 3
      , 期号变更 = 4  //期号变更
    } //开奖中, 停止进仓

    /// <summary>
    /// 
    /// </summary>
    /// <param name="status">状态</param>
    /// <param name="issueid">状态对应期号</param>
    /// <param name="data">状态对应数据</param>
    /// <returns></returns>
    public delegate int BoterStatusChangeHandle(BoterStatus status, int issueid, BgLotteryData data);
    public delegate int BoterServicesIssueChangeHandle(int newIssueid, BgLotteryData data);    //期号变更

    /// <summary>
    ///     机器人本地服务
    /// </summary>
    public class BoterServices
        : INotifyPropertyChanged
    {
        #region 数据绑定模板
        public event PropertyChangedEventHandler PropertyChanged;
        public BoterStatusChangeHandle BoterStatusChange;
        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            //if (PropertyChanged == null)
            //    return;

            //var memberExpression = property.Body as MemberExpression;
            //if (memberExpression == null)
            //    return;

            //Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            //if (dispatcher != null)
            //    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
            //else
            //{
            //    _view.Invoke(new Action(() => {
            //        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
            //    }));
            //}

            if (PropertyChanged == null)
                return;

            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }
        #endregion 数据绑定模板

        private BoterApi _boterApi;     //机器人webapi接口, 获取相关数据, 保存数据

        //状态和期号
        private BoterStatus _status = BoterStatus.等待中;
        private object _lockStatus = new object();

        //最后一起数据
        private int _IssueidCur = 0;
        public int IssueidCur { get { return _IssueidCur; } }

        private LogBindlite _loglite;   //这个在main中初始化的
        public LogBindlite loglite { get { return _loglite; } }


        //视图, 窗口
        private Form _view;
        private AppMainSettingModel _appSetting;
        private OpenLotteryView _lotteryView;
        public OpenLotteryView lotteryView { get { return _lotteryView; } }
        private Form _coninsView; //CoinsOrderView
        public Form coninsView { get { return _coninsView; } }
        private LogView _logView = null;    //日志窗口
        public LogView logView { get { return _logView; } }
        private BetOrderManager _betOrderView = null;
        public BetOrderManager betOrderView { get { return _betOrderView; } }

        private WxHelper _wxHelper;
        public WxHelper wxHelper { get { return _wxHelper; } }

        private System.Threading.Timer _timer_updata_group_member;



        //绑定网盘的数据, 网盘数据更新后, 更新进来, 因为网盘是可以选择的，界面不能绑定网盘，可以绑定这里数据
        private float _Amount = 0;
        public float Amount
        {
            get { return _Amount; }
            set
            {
                if (_Amount == value)
                    return;
                _Amount = value;
                NotifyPropertyChanged(() => Amount);
            }
        }


        private bool _RunningStatus = false;
        public bool RunningStatus { get { return _RunningStatus; } }

        public bool fixmode = true; //修复补分模式出的BUG


        //会员表
        private V2MemberBindlite _v2Memberbindlite = new V2MemberBindlite();
        public V2MemberBindlite v2Memberbindlite { get { return _v2Memberbindlite; } }
        //订单表
        private V2MemberOrderBindlite _v2memberOderbindlite = null;
        public V2MemberOrderBindlite v2memberOderbindlite { get { return _v2memberOderbindlite; } }
        //充值表-全部
        private V2MemberCoinsBuySellBindlite _v2MemberCoinsBuySellbindlite = null;
        public V2MemberCoinsBuySellBindlite v2MemberCoinsBuySellbindlite { get { return _v2MemberCoinsBuySellbindlite; } }
        //充值表-未处理
        private V2MemberCoinsBuySellBindlite _v2MemberCoinsBuySellbindliteUnProcessed = null;
        public V2MemberCoinsBuySellBindlite v2MemberCoinsBuySellbindliteUnProcessed { get { return _v2MemberCoinsBuySellbindliteUnProcessed; } }

        //收单回复配置
        private string _Reply_收单失败 = "客官我有点不明白!";
        public string Reply_收单失败 { get { return _Reply_收单失败; } }


        private string _Reply_收单成功 = "已进仓!";
        public string Reply_收单成功 { get { return _Reply_收单成功; } }


        private string _Reply_暂停服务 = "已进仓!";
        public string Reply_暂停服务 { get { return _Reply_暂停服务; } }


        private string _Reply_余额不足 = "客官你的荷包是否不足!";
        public string Reply_余额不足 { get { return _Reply_余额不足; } }


        private string _Reply_开盘提示 = "---------线下开始---------";
        public string Reply_开盘提示 { get { return _Reply_开盘提示; } }


        private string _Reply_回合停止 = "时间到! 停止进仓! 以此为准!";
        public string Reply_回合停止 { get { return _Reply_回合停止; } }


        private string _Reply_收盘提示 = "---------线下开始---------";
        public string Reply_收盘提示 { get { return _Reply_收盘提示; } }


        //功能开启, 禁用
        public bool b群发消息 = false;

        //30秒报时
        bool b30 = false;
        bool b15 = false;

  

        //限额临时数据, 玩法额度限制<车号+玩法名, 金额>
        Dictionary<string, int> money_limit = new Dictionary<string, int>();


        //公共信息
        private string _panDescribe;
        public string panDescribe
        {
            get { return _panDescribe; }
            set
            {
                if (_panDescribe == value)
                    return;
                _panDescribe = value;
                NotifyPropertyChanged(() => panDescribe);
            }
        }

        private float _IncomeTotal;     //总盈亏
        public float IncomeTotal
        {
            get { return _IncomeTotal; }
            set
            {
                if (_IncomeTotal == value)
                    return;
                _IncomeTotal = value;
                NotifyPropertyChanged(() => IncomeTotal);
            }
        }

        private float _IncomeToday;     //今日盈亏
        public float IncomeToday
        {
            get { return _IncomeToday; }
            set
            {
                if (_IncomeToday == value)
                    return;
                _IncomeToday = value;
                NotifyPropertyChanged(() => IncomeToday);
            }
        }

        /// <summary>
        ///     总下注
        /// </summary>
        private int _BetMoneyTotal; 
        public int BetMoneyTotal
        {
            get { return _BetMoneyTotal; }
            set
            {
                if (_BetMoneyTotal == value)
                    return;
                _BetMoneyTotal = value;
                NotifyPropertyChanged(() => BetMoneyTotal);
            }
        }

        private int _BetMoneyToday;
        /// <summary>
        ///     今日下注
        /// </summary>
        public int BetMoneyToday
        {
            get { return _BetMoneyToday; }
            set
            {
                if (_BetMoneyToday == value)
                    return;
                _BetMoneyToday = value;
                NotifyPropertyChanged(() => BetMoneyToday);
            }
        }


        private int _BetMoneyCur;   //本期当前下注
        public int BetMoneyCur
        {
            get { return _BetMoneyCur; }
            set
            {
                if (_BetMoneyCur == value)
                    return;
                _BetMoneyCur = value;
                NotifyPropertyChanged(() => BetMoneyCur);
            }
        }

        //总上分
        private int _CreditTotal;
        public int CreditTotal
        {
            get { return _CreditTotal; }
            set
            {
                if (_CreditTotal == value)
                    return;
                _CreditTotal = value;
                NotifyPropertyChanged(() => CreditTotal);
            }
        }

        /// <summary>
        ///     今日上分
        /// </summary>
        private int _CreditToday;
        public int CreditToday
        {
            get { return _CreditToday; }
            set
            {
                if (_CreditToday == value)
                    return;
                _CreditToday = value;
                NotifyPropertyChanged(() => CreditToday);
            }
        }

        private int _WithdrawTotal;  //总下分
        public int WithdrawTotal
        {
            get { return _WithdrawTotal; }
            set
            {
                if (_WithdrawTotal == value)
                    return;
                _WithdrawTotal = value;
                NotifyPropertyChanged(() => WithdrawTotal);
            }
        }

        private int _WithdrawToday;  //总下分
        public int WithdrawToday
        {
            get { return _WithdrawToday; }
            set
            {
                if (_WithdrawToday == value)
                    return;
                _WithdrawToday = value;
                NotifyPropertyChanged(() => WithdrawToday);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="appSetting"></param>
        /// <param name="Start">是否默认启动服务</param>
        public BoterServices(Form view, AppMainSettingModel appSetting, bool Start = false)
        {
            this._view = view;
            this._appSetting = appSetting;

            _v2MemberCoinsBuySellbindlite = new V2MemberCoinsBuySellBindlite(view, funcCoinsOrderUpdata);
            _v2MemberCoinsBuySellbindliteUnProcessed = new V2MemberCoinsBuySellBindlite(view, null);
            _loglite = new LogBindlite(view);   //日志类
            _logView = new LogView(_loglite);
            _boterApi = BoterApi.GetInstance();
            this._lotteryView = new OpenLotteryView(lotteryDatas);
            if(Program.appMode == AppMode.普通模式)
                this._coninsView = new CoinsOrderView(this, _v2MemberCoinsBuySellbindlite);
            else
                this._coninsView = new XCoinsOrderView(this, _v2MemberCoinsBuySellbindlite);

            this._v2memberOderbindlite = new V2MemberOrderBindlite(view);
            this._betOrderView = new BetOrderManager(this, _v2memberOderbindlite);
            _wxHelper = new WxHelper(view);             //微信助手.微信相关功能hook
            //_wxHelper.func_11032 = func_11032;          //初始化回调:群成员获得
            //_wxHelper.func_11046 = func_11046;          //初始化群消息接受处理
            _wxHelper.wxServices.onReceiveCallback = OnReceiveCallback;
            _wxHelper.func_11098 = func_11098;          //群成员增加回调
            _wxHelper.func_11099 = func_11099;          //群成员退出回调
            if(Start)
                this.Start();

            _timer_updata_group_member = null;  // new System.Threading.Timer(TimerUpdataGroupMember, null, 2, 1000);
        }



        //上分动作.完整整个上分逻辑...,
        //不要在外部直接操作上下分表, 只能在这里面操作
        //
        public bool OnActionMemberCredit(V2MemberCoinsBuySell CoinsOrder, bool loading = false)
        {
            //检查订单是否有效, 有些输入了错误的会员ID


            //首先把订单加入列表
            _v2MemberCoinsBuySellbindlite.Add(CoinsOrder, loading);


            //按道理这里不对, 按之前逻辑，如果重新打开软件，今日上下分累计数据会清空。有空待测试。
            if (!loading)
            {
                //_v2MemberCoinsBuySellbindliteUnProcessed.Add(CoinsOrder, loading);
                if (CoinsOrder.PayStatus == V2MemberPayStatus.等待处理)
                {
                    if (CoinsOrder.PayAction == V2MemberPayAction.上分)
                    {
                        var member = this.v2Memberbindlite.FirstOrDefault(p => p.wxid == CoinsOrder.wxid);

                        member.Balance += CoinsOrder.Money;
                        member.CreditToday += CoinsOrder.Money;  //今日上分
                        member.CreditTotal += CoinsOrder.Money;  //总上分

                        //系统总上分
                        this.CreditToday += CoinsOrder.Money;
                        this.CreditTotal += CoinsOrder.Money;

                        this.wxHelper.CallSendText_11036(CoinsOrder.GroupWxId, $"@{member.nickname}\r[{member.id}]上{CoinsOrder.Money}完成|余:{(int)member.Balance}");
                        CoinsOrder.PayStatus = V2MemberPayStatus.同意;
                    }
                    else
                    {
                        string msg = $"上分调用错误! {JsonConvert.SerializeObject(CoinsOrder)}";
                        MessageBox.Show(msg);
                        _loglite.Add(Log.Create($"OnActionMemberCredit::上分调用错误", msg));
                    }
                }
                //刷新主界面，上下分数据显示
                UpdataPanDescribe();
            }
            
            return true;
        }


        /// <summary>
        ///     下分动作
        /// </summary>
        /// <param name="member"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public bool OnActionMemberWithdraw(V2MemberCoinsBuySell CoinsOrder)
        {
            //WithdrawTotal += money;
            if (CoinsOrder.PayStatus == V2MemberPayStatus.等待处理)
            {
                if (CoinsOrder.PayAction == V2MemberPayAction.下分)
                {
                    var member = this.v2Memberbindlite.FirstOrDefault(p => p.wxid == CoinsOrder.wxid);
                    if (CoinsOrder.Money > (int)member.Balance)
                    {
                        this.wxHelper.CallSendText_11036(CoinsOrder.GroupWxId, $"@{member.nickname} 存储不足!");
                        return false;
                    }

                    member.Balance -= CoinsOrder.Money;
                    member.WithdrawToday += CoinsOrder.Money;
                    member.WithdrawTotal += CoinsOrder.Money;

                    this.WithdrawToday += CoinsOrder.Money;
                    this.WithdrawTotal += CoinsOrder.Money;

                    this.wxHelper.CallSendText_11036(CoinsOrder.GroupWxId, $"@{member.nickname}\r[{member.id}]下{CoinsOrder.Money}完成|余:{(int)member.Balance}");
                    CoinsOrder.PayStatus = V2MemberPayStatus.同意;
                }
            }

            UpdataPanDescribe();
            return true;
        }

        //订单成功插入回调。
        public bool funcCoinsOrderUpdata(V2MemberCoinsBuySellBindlite list, V2MemberCoinsBuySell item)
        {
            //有订单变消息, 或者属性变更消息, 就调用这个
                //_view.setBuySellConis(new Func<UcBuySellCoins, bool>((p) =>
                //{
                //    //查找没有处理过的订单
                //    try
                //    {
                //        if(item.PayStatus == V2MemberPayStatus.等待处理)
                //        {
                //            if (item.PayStatus == V2MemberPayStatus.同意
                //            || item.PayStatus == V2MemberPayStatus.忽略)
                //            {
                //                p.SetMemberCoinsBuySell(null);

                //                //这个时候再次查找下一个订单, 再播报一次
                //                var coninsOrder = v2MemberCoinsBuySellbindlite.FirstOrDefault(x => x.Timestamp > item.Timestamp);
                //                if (coninsOrder != null)
                //                    p.SetMemberCoinsBuySell(coninsOrder);
                //            }
                //            else
                //            {
                //                p.SetMemberCoinsBuySell(item);
                //            }
                //        }
                //        else
                //        {
                //            var coninsOrder = v2MemberCoinsBuySellbindlite.FirstOrDefault(x => x.Timestamp > item.Timestamp && x.PayStatus == V2MemberPayStatus.等待处理);
                //            if (coninsOrder != null)
                //            {
                //                p.SetMemberCoinsBuySell(coninsOrder);
                //            }    
                //        }
                //    }
                //    catch
                //    {

                //    }
                //    return true;
                //}));

            return true;
        }

        /// <summary>
        ///     订单添加成功回调
        ///         0622这里修改订单添加成功, 仅仅是成功
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool OnMemberOrderCreate(V2MemberOrder order)
        {
            //判断下是否有取消的订单, 有就不处理
            //因为有可能初始化进来, 加载所有订单的时候, 会调用到这里
            //这里事投注成功回调
            var m = v2Memberbindlite.FirstOrDefault(p => p.wxid == order.wxid);
            if(m != null)
            {
                m.BetWait = m.BetWait + order.AmountTotal;
            }
            if (order.OrderType != OrderTypeEnum.托 && order.OrderStatus != OrderStatusEnum.已取消)
            {
                DateTime dtOrder = LxTimestampHelper.GetDateTime(order.TimeStampBet);
                DateTime dtNow = DateTime.Now;
                if (dtOrder.ToShortDateString() == dtNow.ToShortDateString())
                {
                    BetMoneyToday += (int)order.AmountTotal;
                    BetMoneyTotal += (int)order.AmountTotal;
                        
                    if (order.IssueId == _IssueidCur)
                    {
                        //当期投注量
                        BetMoneyCur += (int)order.AmountTotal;
                    }
                }
                else
                {
                    BetMoneyTotal += (int)order.AmountTotal;
                    //if (order.OrderStatus == OrderStatusEnum.已完成)
                    //    IncomeTotal += (order.AmountTotal - order.Profit);
                }
                UpdataPanDescribe();
            }

            return true;
        }

        //订单完成, 在这里面执行开奖完成订单操作。
        /// <summary>
        ///     绑定群主初始化long得时候, data == null
        /// </summary>
        /// <param name="order"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool OnMemberOrderFinish(V2MemberOrder order, BgLotteryData data)
        {
            try
            {
                var order_cheak = v2memberOderbindlite.FirstOrDefault(p => p.id == order.id);
                if (order_cheak == null)
                {
                    string error = $"OnMemberOrderFinish::订单中没找到该订单! ";
                    _loglite.Add(Log.Create($"订单校验错误::群::{order.wxid}::订单{order.BetContentOriginal}", JsonConvert.SerializeObject(order)));
                    Debug.WriteLine(error + "::" + JsonConvert.SerializeObject(order));
                    return false;
                }
                if (!object.ReferenceEquals(order, order_cheak))
                {
                    order = order_cheak;
                }

                if (order.OrderStatus != OrderStatusEnum.已完成)
                {
                    Debug.WriteLine("补分:OrderStatus !=  已完成");
                    if (data == null)
                    {
                        Debug.WriteLine("补分:data==null");
                        return false;
                    }

                    if (string.IsNullOrEmpty(data.lotteryData))
                    {
                        Debug.WriteLine("补分:lotterydata==null");
                        return false;
                    }

                    if (order.OrderStatus == OrderStatusEnum.已取消)
                    {
                        Debug.WriteLine("补分:OrderStatus==已取消");
                        return false;
                    }


                    var m = v2Memberbindlite.FirstOrDefault(p => p.wxid == order.wxid);
                    float win = order.OpenLottery(data, _appSetting.wxOdds, _appSetting.Zsjs); //开奖, 赔率  MainConfigure.appSetting, 毛利, 没有扣除本钱的 // win有输, 有盈
                    float win_net = win - (win / _appSetting.wxOdds);        // 纯利  x * 1.97 = win    纯利  = (win-(win/1.97))
                    m.OpenLottery(order);
                    //计算今日盈利, 总盈利
                    if (order.OrderType != OrderTypeEnum.托)
                    {
                        _view.BeginInvoke(new Action(() =>
                        {
                            this.IncomeToday -= order.NetProfit;
                            this.IncomeTotal -= order.NetProfit;
                            order.OrderStatus = OrderStatusEnum.已完成;
                            m.BetWait = m.BetWait - order.AmountTotal;
                        }));
                        UpdataPanDescribe();
                    }

                    Debug.WriteLine($"补分:成功，{JsonConvert.SerializeObject(order)}");
                    return true;
                }
                else if (order.OrderStatus == OrderStatusEnum.已完成)
                {
                    Debug.WriteLine("补分:OrderStatus ==  完成");
                    var m = v2Memberbindlite.FirstOrDefault(p => p.wxid == order.wxid);
                    if (m != null)
                    {
                        m.IncomeTodayStart += order.NetProfit;
                        m.BetWait = m.BetWait - order.AmountTotal;
                    }

                    if (order.OrderType != OrderTypeEnum.托)
                    {
                        _view.BeginInvoke(new Action(() =>
                        {
                            IncomeToday -= order.NetProfit;
                            IncomeTotal -= order.NetProfit;
                            order.OrderStatus = OrderStatusEnum.已完成;
                            //--
                            //机器人盈亏，是相反的，用总金额-会员盈利
                            //IncomeTotal += (order.AmountTotal - order.Profit);
                            //IncomeToday += (order.AmountTotal - order.Profit);
                        }));
                        UpdataPanDescribe();
                    }
                    Debug.WriteLine($"补分:成功，{JsonConvert.SerializeObject(order)}");
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"补分:失败,ex={ex.Message}");
            }
            return false;
        }

        /// <summary>
        ///     订单取消回调
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool OnMemberOrderCancel(V2MemberOrder order)
        {
            var m = v2Memberbindlite.FirstOrDefault(p => p.wxid == order.wxid);
            if (m != null)
            {
                m.BetWait = m.BetWait - order.AmountTotal;
            }

            if (order.OrderType != OrderTypeEnum.托)
            {
                DateTime dtOrder = LxTimestampHelper.GetDateTime(order.TimeStampBet);
                DateTime dtNow = DateTime.Now;
                if (dtOrder.ToShortDateString() == dtNow.ToShortDateString())
                {
                    BetMoneyTotal -= (int)order.AmountTotal;
                    BetMoneyToday -= (int)order.AmountTotal;
                    if (order.IssueId == _IssueidCur)
                    {
                        BetMoneyCur -= (int)order.AmountTotal;
                    }
                }
                else
                {
                    BetMoneyTotal -= (int)order.AmountTotal;
                }

                UpdataPanDescribe();
            }
            return true;
        }


        /// <summary>
        ///     上分结束.其实上分应该也要统一调用这里的方法的,没时间整在一起了,放到会员类里面调用了
        /// </summary>
        /// <returns></returns>
        //public bool OnCredited(V2MemberCoinsBuySell MoneyOrder)
        //{
        //    try
        //    {
        //        if (MoneyOrder.PayStatus == V2MemberPayStatus.同意)
        //        {
        //            DateTime dtOrder = LxTimestampHelper.GetDateTime(MoneyOrder.Timestamp);
        //            DateTime dtNow = DateTime.Now;

        //            if (MoneyOrder.PayAction == V2MemberPayAction.上分)
        //            {
        //                CreditTotal += MoneyOrder.Money;
        //                if (dtOrder.ToShortDateString() == dtNow.ToShortDateString())
        //                {
        //                    CreditToday += MoneyOrder.Money;
        //                }
        //            }
        //            else if (MoneyOrder.PayAction == V2MemberPayAction.下分)
        //            {
        //                WithdrawTotal += MoneyOrder.Money;
        //                if (dtOrder.ToShortDateString() == dtNow.ToShortDateString())
        //                {
        //                    WithdrawToday += MoneyOrder.Money;
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //    return true;
        //}


 

        //绑定的群信息
        private WxGroup _groupBind = new WxGroup();
        public WxGroup groupBind { get { return _groupBind; } set { _groupBind = value; } }


        //开奖表
        private BgLotteryDataBindlite _lotteryDatas = new BgLotteryDataBindlite();
        public BgLotteryDataBindlite lotteryDatas { get { return _lotteryDatas; } }

        /// <summary>
        ///     期号
        ///     数据
        /// </summary>
        private ConcurrentDictionary<int, BgLotteryData> itemUpdata = new ConcurrentDictionary<int, BgLotteryData>();


         public bool Refresh()
        {
            //刷新好友, 刷新群组数据
            try
            {
                wxHelper.UiUpdataContact();
            }
            catch
            {
                return false;
            }
            return true;
        }

        //回调函数
        

        /// <summary>
        ///     刷新盘信息, 注单信息
        /// </summary>
        public void UpdataPanDescribe(bool setZero = false)
        {
            _view.BeginInvoke(new Action(() => {
                if (setZero)
                {
                    BetMoneyTotal = 0;
                    BetMoneyToday = 0;
                    BetMoneyCur = 0;
                    IncomeTotal = 0f;
                    IncomeToday = 0f;
                    CreditTotal = 0;
                    WithdrawTotal = 0;
                    CreditToday = 0;
                    WithdrawToday = 0;
                }
                panDescribe = $"总注:{BetMoneyTotal}|今投:{BetMoneyToday}|当前:{_IssueidCur}投注:{BetMoneyCur} | 总/今盈利:{IncomeTotal}/{IncomeToday} | 总上/今上:{CreditTotal}/{CreditToday} 总下/今下:{WithdrawTotal}/{WithdrawToday}";
            }));
        }

        //需要更新数据的队列
        public bool BindGroup(WxGroup group, bool offline = false)
        {
            try
            {
                _loglite.Add(Log.Create($"BindGroup::绑定群::{group.wxid}", JsonConvert.SerializeObject(group)));
                //首先清理数据
                if (group.wxid != this._groupBind.wxid)
                {
                    UpdataPanDescribe(true);
                    _v2memberOderbindlite.Clear();
                    _v2MemberCoinsBuySellbindlite.Clear();
                    _v2MemberCoinsBuySellbindliteUnProcessed.Clear();
                }

                this._groupBind.Updata(group);

                //添加会员进入到列表
                if (!offline)
                {
                    var strlist = wxHelper.wxServices.GetMemberList(group.wxid);
                    var members = strlist.data.members.Split('^');
                    foreach (var member in members)
                    {
                        string member_fix = member.Replace("G", "");
                        var m = wxHelper.wxContacts.FirstOrDefault(p => p.wxid == member_fix);
                        if (m == null)
                        {
                            m = new WxContacts();
                            m.wxid = member_fix;
                            m.remark = "非会员";
                        }
                        else
                        {
                            m.remark = "会员";
                        }
                        if (string.IsNullOrEmpty(m.nickname))
                        {
                            var ss = wxHelper.wxServices.GetMemberNickname(m.wxid);
                            m.nickname = ss.data;
                        }

                        v2Memberbindlite.Add(group.wxid, new V2Member(m), new Func<V2Member, MemBerState>((p) =>
                        {
                            if (p.wxid == this.wxHelper.wxid)
                            {
                                return MemBerState.管理;
                            }
                            if (p.remark == "会员")
                                return MemBerState.会员;
                            if (p.remark == "非会员")
                                return MemBerState.非会员;

                            return MemBerState.非会员;
                        }));
                    }
                }
                else
                {
                    var memners = v2Memberbindlite.sql.getTabble().Where(p => p.GroupWxId == group.wxid).ToList();
                    foreach (var m in memners) {
                        v2Memberbindlite.Add(group.wxid, new V2Member(m), new Func<V2Member, MemBerState>((p) =>
                        {
                            if (p.wxid == this.wxHelper.wxid)
                            {
                                return MemBerState.管理;
                            }
                            if (p.remark == "会员")
                                return MemBerState.会员;
                            if (p.remark == "非会员")
                                return MemBerState.非会员;

                            return MemBerState.非会员;
                        }));
                    }
                }


                //this.wxHelper.CallGroup_11032(group.wxid);
                //结束

                //把本地数据加载进表
                var items = _v2memberOderbindlite.sql.getTabble().Where(p => p.GroupWxId == group.wxid).ToList();
                foreach (var item in items)
                {
                    _v2memberOderbindlite.Add(item, OnMemberOrderCreate, true);    //初始化数据
                    //OnMemberOrderCancel(item);
                    OnMemberOrderFinish(item, null);
                    // if (item.OrderStatus == OrderStatusEnum.已取消)   //托的流水不计算到真实流水

                }

                //加载上下分表
                var money_orders = _v2MemberCoinsBuySellbindlite.sql.getTabble().Where(p => p.GroupWxId == group.wxid).ToList();
                foreach (var item in money_orders)
                {
                    //_v2MemberCoinsBuySellbindlite.Add(item,  true);
                    OnActionMemberCredit(item, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                _loglite.Add(Log.Create($"BindGroup::绑定群异常", JsonConvert.SerializeObject(group)));
            }
            return false;
        }


        /// <summary>
        ///     设置运行状态
        /// </summary>
        /// <returns></returns>
        public void SetRunningState(bool bRunning, Func<bool, bool>FunStateChanged)
        {
            _loglite.Add(Log.Create($"BindGroup::设置绑定群状态::{_groupBind.wxid}", $"{bRunning}"));
            _RunningStatus = bRunning;
            FunStateChanged?.Invoke(_RunningStatus);
        }
        

        /// <summary>
        ///     处理显示,如果最小化了, 激活到前台
        /// </summary>
        /// <param name="view"></param>
        public static void ShowWindows(Form view)
        {
            if (!view.Visible)
            {
                view.Show();
            }
            else
            {
                if (view.WindowState == FormWindowState.Minimized)
                    view.WindowState = FormWindowState.Normal;

                view.Activate();
            }
        }


        ////当期数据
        //private BgLotteryData _bgDataCur = new BgLotteryData();   
        //public BgLotteryData bgDataCur { get { return _bgDataCur; } }

        //默认启动服务
        public void Start()
        {
            //临时测试代码开始..添加更新数据
            //itemUpdata.AddOrUpdate(112049307, new BgLotteryData(), (key, oldValue) => oldValue);
            //itemUpdata.AddOrUpdate(112049308, new BgLotteryData(), (key, oldValue) => oldValue);
            //itemUpdata.AddOrUpdate(112049309, new BgLotteryData(), (key, oldValue) => oldValue);


            //临时测试代码结束
            Task.Factory.StartNew(() => {
                //DateTime dtStart = DateTime.Now;
                while(true)
                {
                    try
                    {
                        if(_status != BoterStatus.开奖中)
                        {
                            DateTime dtNow = DateTime.Now;
                            //得到最后一期开奖时间对应的 - 期号
                            int issueid = BinGouHelper.getNextIssueId(DateTime.Now);
                            if (issueid != _IssueidCur)
                            {
                                //期号变更..更改状态, 变成 等待状态
                                lock(_lockStatus)
                                {
                                    try
                                    {
                                        IssueChange(issueid); //里面只负责数据, 不负责状态
                                        On开奖中(issueid - 1); //On函数负责状态-变更成开奖中状态
                                    }
                                    catch(Exception ex)
                                    {
                                        _loglite.Add(Log.Create($"Start::开奖中::锁内异常!", $"{issueid}"));
                                    }
                                }
                                Thread.Sleep(1000);
                                continue;
                            }

                            if(_status != BoterStatus.开奖中)
                            {
                                DateTime issueTime = BinGouHelper.getOpenDatetime(issueid);
                                var ts = issueTime - dtNow;
                                var sec = ts.TotalSeconds - _appSetting.reduceCloseSeconds; // reduceCloseSeconds = 45秒
                                                                                            //测试输出
                                                                                            //Debug.WriteLine($"min={_appSetting.wxMinBet},max={_appSetting.wxMaxBet}");
                                if (sec >= 0)
                                {
                                    if (sec <= 300)
                                    {
                                        if(sec < 30 && !b30)
                                        {
                                            b30 = true;
                                            wxHelper.CallSendText_11036(groupBind.wxid, $"{issueid%1000} 还剩30秒");
                                        }
                                        if (sec < 15 && !b15)
                                        {
                                            b15 = true;
                                            wxHelper.CallSendText_11036(groupBind.wxid, $"{issueid%1000} 还剩15秒");
                                        }
                                        On开盘中(issueid);
                                    }
                                    else
                                    {//要检查是否处于
                                        //新版
                                        _status = BoterStatus.等待中;
                                        BoterStatusChange?.Invoke(_status, issueid, null);

                                        //旧版
                                        //_view.setLotteryCur(new Func<UcLotteryDataCur, bool>(p => {
                                        //    p.SetStatus(_status);
                                        //    return true;
                                        //}));
                                    }
                                }
                                else if (sec <= 0
                                && sec >= -_appSetting.reduceCloseSeconds)
                                {
                                    On封盘中(issueid);
                                }
                            }
                        }  
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Start::{ex.Message}");
                    }
                    Thread.Sleep(1000);
                }
            });

            //更新队列线程
            Task.Factory.StartNew(() => {
                while (true)
                {
                    try
                    {
                        if(itemUpdata.Count > 0)
                        {
                            var item = itemUpdata.LastOrDefault();
                            if(item.Key != item.Value.IssueId)  //说明没有获取开奖数据
                            {
                                ////计算key时间
                                var response = _boterApi.getBgdata(item.Key);
                                if(response.code == 0)
                                {
                                    if (response.data != null)
                                    {
                                        BgLotteryData bgData = new BgLotteryData();
                                        itemUpdata.TryRemove(item.Key, out bgData);

                                        _lotteryView.GetDgView(new Func<CCWin.SkinControl.SkinDataGridView, bool>((p) => {
                                            _lotteryDatas.Add(bgData.FillLotteryData(response.data.issueid
                                            , response.data.lotteryData
                                            , response.data.lottery_time));
                                            return true;
                                        }));

                                        //开奖
                                        BoterStatusChange?.Invoke(_status, bgData.IssueId, bgData);
                                        On已开奖(bgData);
                                        _status = BoterStatus.等待中;  //等待中, 就会在状态循环中, 变成开盘中
                                    }
                                }
                                else
                                {
                                    if(response.code == BoterApi.VERIFY_SIGN_OFFTIME)
                                    {
                                        //MessageBox.Show("账号过期");
                                        OnBoterOffTime();
                                    }
                                    else if(response.code == BoterApi.VERIFY_SIGN_INVALID)
                                    {
                                        OnBoterInvalid();
                                    }
                                    else
                                    {
                                        _loglite.Add(Log.Create($"OnBoterOther::{_IssueidCur}::{response.code}", ""));
                                        //Thread.Sleep(1000);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //没有数据的时候,  更新网盘账号数据

                        }
                    }
                    catch(Exception ex)
                    {
                        _loglite.Add(Log.Create($"更新队列错误::{_IssueidCur}", $"{ex.Message}"));
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        ///     机器人账号国企
        /// </summary>
        /// <returns></returns>
        public int OnBoterOffTime()
        {
            //账号过期, 就要停在这个函数里, 不给运行下面代码
            string msg = "账号过期";
            _loglite.Add(Log.Create($"OnBoterInvalid::账号过期::{_IssueidCur}", msg));
            MessageBox.Show(msg);

            return 1;
        }
        /// <summary>
        ///     账号无效
        /// </summary>
        /// <returns></returns>
        public int OnBoterInvalid()
        {
            string msg = "账号失效! 请重新登录\r\n请检查是否有在其他地方登录导致本次失效!";
            _loglite.Add(Log.Create($"OnBoterInvalid::账号失效::{_IssueidCur}", msg));
            MessageBox.Show(msg);
            return 1;
        }


        private int On开盘中(int issueid)
        {
            int response = -1;
            try
            {
                if(_status != BoterStatus.开盘中)
                {
                    lock(_lockStatus)
                    {
                        try
                        {
                            _loglite.Add(Log.Create($"On开盘中::{issueid}", $""));
                            _status = BoterStatus.开盘中;
                            BoterStatusChange?.Invoke(_status, issueid, null);
                            //_view.setLotteryCur(new Func<UcLotteryDataCur, bool>(p => {
                            //    p.SetStatus(_status);
                            //    return true;
                            //}));
                            b30 = false;
                            b15 = false;
                            wxHelper.CallSendText_11036(groupBind.wxid, $"第{issueid % 1000}队\r{Reply_开盘提示}");

                            //开始出图
                            On开盘发送历史记录图片(issueid);
                        }
                        catch(Exception ex)
                        {
                            _loglite.Add(Log.Create($"On开盘中错误00::{issueid}-{ex.Message}", $""));
                        }
                    }
                    response = 0;
                }
            }
            catch(Exception ex)
            {
                response = 201;
                _loglite.Add(Log.Create($"On开盘中错误::{_IssueidCur}", $"{ex.Message}"));
            }
            return response;
        }

        private bool On开盘发送历史记录图片(int issueid)
        {
            //开始出图
            bool response = false;
            string imagePath = @"C:\1.jpg";
            for (int xx = 0; xx < 5; xx++)
            {
                string errorMessage = "";
                int nstate = CreateImgZst(issueid, imagePath, out errorMessage, 32);
                if (nstate == 0)
                {
                    wxHelper.CallSendImage(groupBind.wxid, imagePath);    //群发图片
                    _loglite.Add(Log.Create($"On开盘发送历史记录图片成功::{issueid}", $"{errorMessage}"));
                    response = true;
                    break;
                }
                if(!string.IsNullOrEmpty(errorMessage))
                {
                    _loglite.Add(Log.Create($"On开盘发送历史记录图片异常::{issueid}", $"{errorMessage}"));
                }
            }
            return response; 
        }

        //封盘, 投注
        private int On封盘中(int issueid)
        {
            int response = -1;
            try
            {
                if (_status != BoterStatus.封盘中)
                {
                    lock (_lockStatus)
                    {
                        try
                        {
                            _loglite.Add(Log.Create($"On封盘中::{issueid}", $"开始"));
                            _status = BoterStatus.封盘中;

                            BoterStatusChange?.Invoke(_status, issueid, null);
                            //_view.setLotteryCur(new Func<UcLotteryDataCur, bool>(p =>
                            //{
                            //    p.SetStatus(_status);
                            //    return true;
                            //}));
                            _loglite.Add(Log.Create($"On封盘中::{issueid}", $"更新当前开奖期号"));
                            StringBuilder sbTxt = new StringBuilder();
                            sbTxt.Append($"{issueid % 1000} {Reply_回合停止}\r");
                            List<V2MemberOrder> orders_redly = v2memberOderbindlite.Where(p => p.IssueId == issueid && p.OrderStatus != OrderStatusEnum.已取消).ToList();
                            //排序
                            orders_redly.Sort(new V2MemberOrderComparerDefault());
                            foreach (var ods in orders_redly)
                            {
                                sbTxt.Append($"{ods.nickname}[{(int)ods.BetFronMoney}]:{ods.BetContentStandar}|计:{ods.AmountTotal}\r");
                            }

                            //sbTxt.Append("用户数据");
                            sbTxt.Append("------线下无效------");
                            wxHelper.CallSendText_11036(groupBind.wxid, sbTxt.ToString());


                            //这里输入订单数据
                            if (orders_redly.Count > 0)
                            {
                                BetOrder(issueid, orders_redly);
                            }

                            PlayMp3("mp3_fp.mp3");       //播放mp3
                                                         //订单发布. 第xxx队, 以此仓为准!
                                                         //昵称[余额]:1大100,2大200,3大300|共计:1000
                                                         //线下无效
                        }
                        catch(Exception ex)
                        {
                            _loglite.Add(Log.Create($"On封盘中::{issueid}::异常", $"{ex.Message},堆栈={ex.StackTrace}"));
                        }
                        finally
                        {
                            _OrderLimitDic.Clear();
                        }
                    }
                    response = 0;
                    _loglite.Add(Log.Create($"On封盘中::{issueid}", $"完成"));
                }
            }
            catch(Exception ex)
            {
                _loglite.Add(Log.Create($"On封盘中错误::{_IssueidCur}", $"{ex.Message}"));
                response = 201;
            }
            return response;
        }

        /// <summary>
        ///     投注
        /// </summary>
        /// <returns></returns>
        public int BetOrder(int issueid, List<V2MemberOrder> orders)
        {
            _loglite.Add(Log.Create($"BoterServices::BetOrder::BeginCount={orders.Count}", $""));

            if (orders.Count == 0)
                return 0;

            //把是托的订单, 移除
            var betapi = _view.GetBetApi();
            //betapi = BetSiteFactory.Create( BetSiteType.海峡, null); //测试, 正式要注销这句
            //合并订单

            //生成订单
            BetStandardOrderList item = new BetStandardOrderList();
            //当期投注
            int money = 0;
            foreach (var v2order in orders)
            {
                if (v2order.OrderType == OrderTypeEnum.托)
                {
                    _view.BeginInvoke(new Action(() => {
                        v2order.OrderStatus = OrderStatusEnum.待结算;
                    }));
                    continue;
                }
                    
                if (v2order.OrderStatus == OrderStatusEnum.已取消)
                    continue;

                BoterBetContents bets = new BoterBetContents(issueid, v2order.BetContentStandar);
                money += bets.GetAmountTatol();
                foreach (var betitem in bets.boterItems)
                {
                    item.Add(new BetStandardOrder(betitem));
                }
            }
            if (betapi == null)
            {
                _loglite.Add(Log.Create($"BoterServices::BetOrder::betapi未初始化", $""));
                foreach (var order in orders)
                {
                    if (order.OrderType != OrderTypeEnum.托)
                    {
                        if (order.OrderStatus == OrderStatusEnum.待处理)
                        {
                            _view.BeginInvoke(new Action(() =>
                            {
                                order.OrderStatus = OrderStatusEnum.待结算;
                                order.OrderType = OrderTypeEnum.盘外;
                            }));
                        }
                    }
                }
                return -1;
            }
            int item_money_sum = item.GetAmountTatol();
            if(money != item_money_sum)
            {
                //出问题了
                Debug.WriteLine($"BetOrder::issueid={issueid}, money!=item_money_sum");
            }

            //开始投注, 里面进行合单后再投注
            var betstatus = betapi.Bet(item);
            Debug.WriteLine($"BetOrder::issueid={issueid}, {betstatus}");
            foreach (var order in orders)
            {
                if (order.OrderStatus == OrderStatusEnum.待处理)
                {
                    if (order.OrderType != OrderTypeEnum.托)
                    {
                        if(betstatus == BetStatus.成功)
                        {
                            _view.BeginInvoke(new Action(() => {
                                order.OrderStatus = OrderStatusEnum.待结算;
                                order.OrderType = OrderTypeEnum.盘内;
                            }));
                        }
                        else
                        {
                            _view.BeginInvoke(new Action(() => {
                                order.OrderStatus = OrderStatusEnum.待结算;
                                order.OrderType = OrderTypeEnum.盘外;
                            }));
                        }
                    }
                }
            }
            return 0;
        }


        private int On开奖中(int issueid)
        {
            //等待开奖结果
            //cur显示等待中
            int response = -1;
            try
            {
                if (_status != BoterStatus.开奖中)
                {
                    //lock (_lockStatus)
                    //{
                        _view.BeginInvoke(new Action(() => {
                            BetMoneyCur = 0;    //当前投注
                        }));

                        _loglite.Add(Log.Create($"On开奖中::{issueid}", $""));
                        _status = BoterStatus.开奖中;
                    BoterStatusChange?.Invoke(_status, issueid, null);

                    //_view.setLotteryCur(new Func<UcLotteryDataCur, bool>(p =>
                    //{
                    //    p.SetStatus(BoterStatus.等待中);
                    //    return true;
                    //}));
                    // }
                    response = 0;
                }
            }
            catch(Exception ex)
            {
                response = 202;
                _loglite.Add(Log.Create($"On封盘中错误{response}::{_IssueidCur}", $"{ex.Message}"));
               
            }
            return response;
        }

        private int On已开奖(BgLotteryData data)
        {
            int response = -1;
            try
            {
                PlayMp3("mp3_kj.mp3");       //播放mp3
                //int dayIndex = BinGouHelper.getNumber(data.IssueId);
                int issueid_lite = data.IssueId % 1000 ;
                _loglite.Add(Log.Create($"On已开奖::{data.IssueId}", $""));
                string txt = $"第{issueid_lite}队\r{data.ToLotteryString()}\r----中~名单----\r";
               

                //On结算(data.IssueId);
                //
                var orders = v2memberOderbindlite.Where(p => p.IssueId == data.IssueId 
                && (p.OrderStatus != OrderStatusEnum.已取消 && p.OrderStatus != OrderStatusEnum.未知)).ToList();

                //如果赔率有问题, > 1那么就写入日志。默认设置赔率 1.95
                if (_appSetting.wxOdds < 1)
                {
                    _appSetting.wxOdds = 1.95f;
                }

                _loglite.Add(Log.Create($"On已开奖::{data.IssueId}", "统计"));
                //记录盈利情况,输赢都记录, 生成报表, 发布
                List<V2MemberOrder> ordersReports = new List<V2MemberOrder>(); //盈利的订单, 合并
                foreach(var order in orders)
                {
                    OnMemberOrderFinish(order, data);

                    //统计输赢数据.整合显示给会员看的。
                    var odsReport = ordersReports.FirstOrDefault(p => p.wxid == order.wxid);
                    if(odsReport == null)
                    {
                        odsReport = new V2MemberOrder(order, order.IssueId, order.TimeStampBet, "订单统计", "订单统计", order.Nums, (int)order.AmountTotal);
                        odsReport.Profit = order.Profit;
                        ordersReports.Add(odsReport);
                    }
                    else
                    {
                        odsReport.Nums += order.Nums;
                        odsReport.AmountTotal += order.AmountTotal;
                        odsReport.Profit += order.Profit;
                    }
                }
                _loglite.Add(Log.Create($"On已开奖::{data.IssueId}", "合并"));
                foreach (var order in ordersReports)
                {
                    var m = v2Memberbindlite.FirstOrDefault(p => p.wxid == order.wxid);
                    _view.Invoke(new Action(() => {
                        order.OrderStatus = OrderStatusEnum.已完成;
                    }));
                    txt = txt + $"{order.nickname}[{(int)m.Balance}] {(int)order.Profit- order.AmountTotal}\r";
                }
                //发送中奖名单
                _loglite.Add(Log.Create($"On已开奖::", "1"));
                wxHelper.CallSendText_11036(groupBind.wxid, txt);
                //发送留分名单
                StringBuilder sbBlanceNames = new StringBuilder(128);
                sbBlanceNames.Append($"第{issueid_lite}队\r{data.ToLotteryString()}\r----留~名单----\r");
                foreach (var m in v2Memberbindlite)
                {
                    if((int)m.Balance >= 1)
                    {
                        sbBlanceNames.Append($"{m.nickname} {(int)m.Balance}\r");
                    }
                }
                _loglite.Add(Log.Create($"On已开奖::", "2"));
                wxHelper.CallSendText_11036(groupBind.wxid, sbBlanceNames.ToString());

                //清空会员表，当期投注金额
                foreach (var m in v2Memberbindlite)
                {
                    m.BetCurZero();
                }
                _loglite.Add(Log.Create($"On已开奖::", "3"));
                //得到是否今日最后一期数据
                int dayIndex = BinGouHelper.getNumber(data.IssueId);
                if(dayIndex == 203)
                {
                    On开盘发送历史记录图片(data.IssueId);
                    wxHelper.CallSendText_11036(groupBind.wxid, $"各位客官今日份结束咯。\r");
                }
            }
            catch(Exception ex)
            {
                loglite.Add(Log.Create("On已开奖::错误", $"message={ex.Message}\r\nstacktrace={ex.StackTrace}\r\nsource={ex.Source}\r\ndata={ex.Data}"));
                response = 203;
            }
            finally
            {
                UpdataPanDescribe();
            }
            return response;
        }


        public void On上下分同意操作(V2MemberCoinsBuySell order)
        {
            try
            {
                if (order.PayAction == V2MemberPayAction.上分)
                    MainConfigure.boterServices.OnActionMemberCredit(order);
                else if (order.PayAction == V2MemberPayAction.下分)
                    MainConfigure.boterServices.OnActionMemberWithdraw(order);

                //清理掉。已经处理过的上下分命令
                var dd = MainConfigure.boterServices.v2MemberCoinsBuySellbindliteUnProcessed.Where(p => p.PayStatus != V2MemberPayStatus.等待处理).ToList();
                for (int i = dd.Count - 1; i >= 0; i--)
                {
                    MainConfigure.boterServices.v2MemberCoinsBuySellbindliteUnProcessed.Remove(dd[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public void On上下分忽略操作(V2MemberCoinsBuySell order)
        {
            try
            {
                order.PayStatus = V2MemberPayStatus.忽略;
                //清理掉。已经处理过的上下分命令
                var dd = MainConfigure.boterServices.v2MemberCoinsBuySellbindliteUnProcessed.Where(p => p.PayStatus != V2MemberPayStatus.等待处理).ToList();
                for (int i = dd.Count - 1; i >= 0; i--)
                {
                    MainConfigure.boterServices.v2MemberCoinsBuySellbindliteUnProcessed.Remove(dd[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        ///     期号变更
        /// </summary>
        /// <returns></returns>
        public bool IssueChange(int issueid)
        {
            bool response = true;
           _IssueidCur = issueid;
            BgLotteryData b1 = new BgLotteryData();
            itemUpdata.AddOrUpdate(issueid-1, b1, (key, oldValue)=> oldValue);

            try
            {
                //期号变更
                BoterStatusChange?.Invoke( BoterStatus.期号变更, issueid, null);

                //_view.setLotteryLast(new Func<UcLotteryDataLast, bool>(p => {
                //    p.LotteryUpdata(
                //        new BgLotteryData()
                //        {
                //            IssueId = issueid - 1,
                //            opentime = BinGouHelper.getOpenDatetime(issueid - 1).ToString(),
                //        });
                //    return true;
                //}));

                //_view.setLotteryCur(new Func<UcLotteryDataCur, bool>(p => {
                //    p.LotteryUpdata(
                //        new BgLotteryData()
                //        {
                //            IssueId = issueid,
                //            opentime = BinGouHelper.getOpenDatetime(issueid).ToString(),
                //        });
                //   // p.SetStatus(BoterStatus.等待中);
                //    return true;
                //}));
            }
            catch(Exception ex)
            {
                response = false;
            }
            return response;
        }


        //更新
        public bool UpdataUserInfo()
        {
            try
            {
                var betsite = _view.GetBetApi();
                if (betsite.GetUserInfoUpdata())
                {
                    //tbxtbx_pan_blance.Text = betsite.amount.ToString("#.##");
                    Amount = betsite.amount;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     走势图
        /// </summary>
        /// <param name="issue_full">期号,检查期号</param>
        /// <param name="image_out_path">生成的绝对路径</param>
        /// <param name="count">条目数量</param>
        /// <returns>0:成功， 1, 网络获取数据失败, 2,</returns>
        public int CreateImgZst(int issue_full, string image_out_path, out string errorMessage, int count = 32)
        {
            int response = 0;
            errorMessage = "";
            try
            {
                //var items =  BoterApi.GetInstance().getbgday(DateTime.Now.ToString("yyyy-MM-dd"), 20, true);
                var bgday = BoterApi.GetInstance().getbgday(DateTime.Now.ToString("yyyy-MM-dd"), count, true);
                if (bgday == null)
                {
                    response = 1;
                    throw new Exception("网络请求异常");
                }
                if(bgday.data == null)
                {
                    response = 2;
                    throw new Exception("无数据");
                }
                var items = bgday.data;
                Image image = Image.FromFile($"{System.Environment.CurrentDirectory}\\bgzst.png");
                Bitmap bitmap = new Bitmap(image, image.Width, image.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    //字体大小
                    float fontSize = 10.0f;
                    float rectY = 71;
                    //定义几个字体
                    System.Drawing.Font font = new System.Drawing.Font("微软雅黑", 16.0f);
                    System.Drawing.Font font_Bold = new System.Drawing.Font("微软雅黑", 16.0f, FontStyle.Bold);

                    //排序
                    if (items.Count > 0)
                    {
                        items.Sort(new BgLotteryDataComparer());
                    }

                    for (int i = 0; i < items.Count(); i++)
                    {
                        float rectX = 0;
                        int issue = items[i].IssueId % 1000;
                        string time = DateTime.Parse(items[i].opentime).ToString("HH:mm");
                        //List<int> pcode = items[i].preDrawCode.Replace(" ", "").Split(',').Select<string, int>(x => Convert.ToInt32(x)).ToList();

                        DrawText(g, issue.ToString(), rectX + 2, rectY + (i * 28), font, Color.Black);    //期号
                        DrawText(g, time, rectX + 60, rectY + (i * 28), font, Color.Black);    //时间
                                                                                                   //if (pcode.Count() == 5)
                                                                                                   //{
                                                                                                   //----------------------------------码1----------------------------------
                        SetLotteryData(g, 126, (int)rectY + (i * 28), font_Bold, items[i].P1.number);
                        SetLotteryData(g, 226 + 3, (int)rectY + (i * 28), font_Bold, items[i].P2.number);
                        SetLotteryData(g, 326 + 3, (int)rectY + (i * 28), font_Bold, items[i].P3.number);
                        SetLotteryData(g, 428 + 3, (int)rectY + (i * 28), font_Bold, items[i].P4.number);
                        SetLotteryData(g, 530 + 3, (int)rectY + (i * 28), font_Bold, items[i].P5.number);
                        int sum = items[i].P1.number + items[i].P2.number + items[i].P3.number + items[i].P4.number + items[i].P5.number;
                        SetLotterySum(g, 635, (int)rectY + (i * 28), font_Bold, sum);
                        if (items[i].P龙虎 == NumberDragonTiger.龙)
                            SetLotteryLh(g, 750, (int)rectY + (i * 28), font_Bold, "龙");
                        else
                            SetLotteryLh(g, 750, (int)rectY + (i * 28), font_Bold, "虎");
                        //}
                    }

                    ////输出方法一：将文件生成并保存到C盘
                    ////string path = $"{System.Environment.CurrentDirectory}\\bgzst1.png";
                    bitmap.Save(image_out_path, System.Drawing.Imaging.ImageFormat.Jpeg);
                    g.Dispose();
                    bitmap.Dispose();
                    image.Dispose();
                }
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
                if (response == 0)
                    response = 10000;
            }
            return response;
        }

        private bool DrawText(System.Drawing.Graphics g, string text, float rectX, float rectY, Font font, Color color, float fontSize = 10.0f)
        {
            bool response = true;
            try
            {
                //下面定义一个矩形区域，以后在这个矩形里画上白底黑字
                float rectWidth = text.Length * (fontSize + 40);
                float rectHeight = fontSize + 40;
                //声明矩形域
                RectangleF textArea = new RectangleF(rectX, rectY, rectWidth, rectHeight);
                //白笔刷，画文字用
                Brush whiteBrush = new SolidBrush(color);
                g.DrawString(text, font, whiteBrush, textArea);
            }
            catch
            {
                response = false;

            }
            return response;
        }


        /// <summary>
        ///     设置每个球的数据
        /// </summary>
        /// <returns></returns>
        private void SetLotteryData(System.Drawing.Graphics g, int x_start_pos, int y_start_pos, Font font, int value)
        {
            float tmp_rx = x_start_pos;
            if (value >= 1 && value <= 40)
            {
                if (value >= 1 && value <= 9)
                {
                    tmp_rx += 8;
                }
                DrawText(g, value.ToString(), tmp_rx, y_start_pos, font, Color.Black);    // P1数字,
                DrawText(g, "小", x_start_pos + 38, y_start_pos, font, Color.Black);    // P1数字,小
            }
            if (value > 40 && value <= 80)
            {
                DrawText(g, value.ToString(), x_start_pos, y_start_pos, font, Color.Red);    // P1数字, 大
                DrawText(g, "大", x_start_pos + 38, y_start_pos, font, Color.Red);    // P1数字, 大
            }
            if (value % 2 == 1)
            {
                DrawText(g, "单", x_start_pos + 68 + 3, y_start_pos, font, Color.Black);    // P1数字, 大
            }
            if (value % 2 == 0)
            {
                DrawText(g, "双", x_start_pos + 68 + 3, y_start_pos, font, Color.Red);    // P1数字, 大
            }
        }


        private void SetLotterySum(System.Drawing.Graphics g, int x_start_pos, int y_start_pos, Font font, int value)
        {
            float tmp_rx = x_start_pos;
            if (value >= 15 && value <= 202)
            {
                if (value >= 1 && value <= 9)
                {
                    tmp_rx += 8;
                }
                DrawText(g, value.ToString(), tmp_rx, y_start_pos, font, Color.Black);    // P1数字,
                DrawText(g, "小", x_start_pos + 50, y_start_pos, font, Color.Black);    // P1数字,小
            }
            if (value >= 203 && value <= 390)
            {
                DrawText(g, value.ToString(), x_start_pos, y_start_pos, font, Color.Red);    // P1数字, 大
                DrawText(g, "大", x_start_pos + 50, y_start_pos, font, Color.Red);    // P1数字, 大
            }
            if (value % 2 == 1)
            {
                DrawText(g, "单", x_start_pos + 80, y_start_pos, font, Color.Black);    // P1数字, 大
            }
            if (value % 2 == 0)
            {
                DrawText(g, "双", x_start_pos + 80, y_start_pos, font, Color.Red);    // P1数字, 大
            }
        }


        private void SetLotteryLh(System.Drawing.Graphics g, int x_start_pos, int y_start_pos, Font font, string strLh)
        {
            float tmp_rx = x_start_pos;
            if (strLh == "龙")
            {
                DrawText(g, strLh, x_start_pos, y_start_pos, font, Color.Red);    // P1数字, 大
            }
            else if (strLh == "虎")
            {
                DrawText(g, strLh, x_start_pos, y_start_pos, font, Color.Black);    // P1数字, 大
            }
        }

        /// <summary>
        ///     启动用户服务
        ///     offline = 离线, 默认 = false
        /// </summary>
        /// <returns></returns>
        public bool userServicesBegin(WxGroup group, Func<bool, bool> FunStateChanged, bool offline = false)
        {
            try
            {
                if (group == null)
                    throw new Exception("绑定群数据不能为空!");

                if (_groupBind != null)
                {
                    if(_groupBind.wxid != group.wxid)
                    {
                        _v2Memberbindlite.Clear();
                    }
                }
                this.BindGroup(group, offline);
                this.SetRunningState(true, FunStateChanged);
            }
            catch
            {

            }
            finally
            {

            }
            

            return true;
        }

        /// <summary>
        ///     停止用户服务
        /// </summary>
        /// <returns></returns>
        public void userServicesStop(Func<bool, bool> FunStateChanged)
        {
            this.SetRunningState(false, FunStateChanged);
        }

        //bool func_11032(WxHelper helper, string group_wxid, string member_list)
        //{
        //    if(groupBind.wxid == group_wxid)
        //    {
        //        var members = JsonConvert.DeserializeObject<V2MemberBindlite>(member_list);
        //        if(members != null)
        //            if(members.Count > 0)
        //            {
        //                foreach(var member in members)
        //                {
        //                    v2Memberbindlite.Add(group_wxid, member, new Func<V2Member, MemBerState>((p) => {
        //                        if(p.wxid == this.wxHelper.wxid)
        //                        {
        //                            return MemBerState.管理;
        //                        }
        //                        var contancts = wxHelper.wxContacts.FirstOrDefault((x)=>x.wxid == p.wxid);
        //                        if(contancts != null)
        //                        {
        //                            return MemBerState.会员;
        //                        }
        //                        return MemBerState.非会员;
        //                    }));
        //                }
        //            }
        //    }
        //    return true;
        //}



        /// <summary>
        ///     timeCallback 调用的时间
        /// </summary>
        /// <param name="timeCallback"></param>
        //保存最后一个加入进来的时间
        DateTime dt_group_add_last;
        private void TimerUpdataGroupMember(object main_wxid)
        {
            // _timer_updata_group_member = new System.Threading.Timer(TimerUpdataGroupMember, null, 2, 1000);
            Debug.WriteLine("更新定时器正在执行!");
            while(true)
            {
                Thread.Sleep(1000);
                try
                {
                    DateTime dt_now = DateTime.Now;
                    var ts = dt_now - dt_group_add_last;
                    if (ts.TotalSeconds > 5)
                    {
                        Sub刷新((string)main_wxid);
                        break;
                    }
                    else
                    {
                        Debug.WriteLine("更新定时器::延时等待中!");
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"更新定时器::异常!{ex.Message}");
                }
            }
            try
            {

                if (_timer_updata_group_member != null)
                {
                    _timer_updata_group_member.Dispose();
                    _timer_updata_group_member = null;
                }
            }
            catch
            {

            }

               
            Debug.WriteLine("更新定时器正在执行完成!");
        }

        /// <summary>
        ///     投注测试
        /// </summary>
        public void BetingTest()
        {
            int issueid = this.IssueidCur;
            var data = new List<V2MemberOrder>();
            data.Add(new V2MemberOrder(null, issueid, LxTimestampHelper.GetTimeStampToInt32(), "1大20", "1大20", 1, 20));
            this.BetOrder(issueid, data);
        }

        //检测重复消息

        private int _last_recv_data_hashcode = 0;
        /// <summary>
        ///     消息处理, 要区分是群，还是私人消息, 做转发处理
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        void OnReceiveCallback(string data)
        {
            Debug.WriteLine($"接收到消息:{data}");
            try
            {
                //ChatRoomMsg msg = JsonConvert.DeserializeObject<ChatRoomMsg>(data);
                int code = data.GetHashCode();
                if(_last_recv_data_hashcode != code)
                {
                    _last_recv_data_hashcode = code;
                    JObject jdata = JObject.Parse(data);
                    if (jdata.ContainsKey("content"))
                    {
                        string main_wxid = jdata["main_wxid"].ToString();
                        string from_wxid = jdata["from_wxid"].ToString();
                        string content = jdata["content"].ToString();
                        string timestamp = jdata["createTime"].ToString();
                        timestamp = timestamp.Substring(0, timestamp.Length - 3);
                        int createTime = Conversion.ToInt32(timestamp);

                        //判断来源：如果是系统给的消息，那么两个ID相同
                        if (main_wxid == from_wxid)
                        {
                            //系统群消息
                            if (main_wxid.IndexOf("@chatroom") >= 0)
                            {
                                if (content.IndexOf("加入了群聊 ") != -1)
                                {
                                    //有新人加入群聊
                                    //启动定时器
                                    if (_timer_updata_group_member == null)
                                    {
                                        Debug.WriteLine("创建了新的更新定时器!");
                                        dt_group_add_last = DateTime.Now;
                                        _timer_updata_group_member = new System.Threading.Timer(TimerUpdataGroupMember, main_wxid, 1000, 0);
                                    }
                                    else
                                    {
                                        Debug.WriteLine("更新了新的更新定时器!");
                                        dt_group_add_last = DateTime.Now;
                                    }

                                }
                                else if (content.IndexOf("移出了群聊") != -1)
                                {
                                    //移除群聊
                                }
                            }
                        }
                        else
                        {
                            //成员群消息
                            var msgPack = new BoterWxGroupMessage(main_wxid, from_wxid, content, createTime);
                            if (msgPack.IsGroupMessage())
                            {
                                //群消息处理。里面会判断是否绑定的群
                                func_11046(this.wxHelper, msgPack);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"OnReceiveCallback::异常::{ex.Message}");
            }
        }

        //群消息接收处理
        //消息过来的时候, 要给消息包装一层, 消息对应的期号, 这样会安全点, 这里未处理
        bool func_11046(WxHelper helper, BoterWxGroupMessage groupMsg)
        {
            if (groupMsg.IsGroupMessage())
            {
                if (groupBind.wxid == groupMsg.room_wxid)
                {
                    var m = v2Memberbindlite.FirstOrDefault((p) => p.wxid == groupMsg.from_wxid);
                    bool bManger = false;
                    if (m != null)
                    {
                        if (m.State != MemBerState.管理 && groupMsg.from_wxid != this.wxHelper.wxid)
                        {
                            if (!string.IsNullOrEmpty(groupMsg.from_wxid))
                            {//如果是别人发出的消息, 这个id不会为空

                                int lexret = -1;
                                if (this.LexicalMember上下分(groupMsg, m) != -1)
                                    return true;

                                if (this.LexicalMember查流水(groupMsg, m) != -1)
                                    return true;

                                lock (_lockStatus)
                                {
                                    try
                                    {
                                        int issueid = _IssueidCur;
                                        if (!RunningStatus)
                                        {
                                            wxHelper.CallSendText_11036(groupMsg.room_wxid, $"@{m.nickname} 暂停中");
                                            return true;
                                        }

                                        if (this.LexicalMemberBet(groupMsg, issueid, m) != -1)
                                            return true;

                                        if (this.LexicalMemberOrderCencel(groupMsg, issueid, m) != -1)
                                            return true;
                                    }
                                    catch (Exception ex)
                                    {//记录错误数据, 首字母带#说明是自己处理过的异常消息, 可以直接输出

                                        return false;
                                    }
                                }

                                if (lexret == -1)
                                {
                                    wxHelper.CallSendText_11036(groupMsg.room_wxid, $"@{m.nickname} {Reply_收单失败}");
                                }
                                //wxHelper.CallSendText_11036(groupMsg.room_wxid, $"@{m.nickname} 暂未开始!请稍等...");
                            }

                        }
                        else
                        {
                           
                            bManger = true;
                        }
                    }
                    if(groupMsg.from_wxid == "NULL")
                    {
                        bManger = true;
                    }
                    if(bManger)
                    {
                        //管理指令
                        if (LexicalAdmin上下分(groupMsg) != -1)
                            return true;
                        if (LexicalAdmin刷新(groupMsg) != -1)
                            return true;
                    }
                }
            }
            //词法分析
            //下单
            //回复
            //开奖
            return true;
        }


        //进群消息
        private bool func_11098(WxHelper helper, GroupMembersWxPacket packaget)
        {
            bool response = true;
            try
            {
                //得到绑定群数据
                if (groupBind == null)
                    return false;

                if(groupBind.wxid == packaget.room_wxid)
                {
                    foreach(var pm in packaget.member_list)
                    {
                        //合成会员
                        var member_result = wxHelper.wxContacts.FirstOrDefault((x) => x.wxid == pm.wxid);
                        if(member_result == null)
                        {
                            member_result = new WxContacts()
                            {
                                 wxid = pm.wxid,
                                 nickname = pm.nickname,
                            };
                        }

                        v2Memberbindlite.Add(packaget.room_wxid, new V2Member(member_result), new Func<V2Member, MemBerState>((p) => {
                            if (p.wxid == this.wxHelper.wxid)
                            {
                                return MemBerState.管理;
                            }
                            var contancts = wxHelper.wxContacts.FirstOrDefault((x) => x.wxid == p.wxid);
                            if (contancts != null)
                            {
                                return MemBerState.会员;
                            }
                            return MemBerState.非会员;
                        }));
                    }
                }
            }
            catch(Exception ex)
            {
                response = false;
            }

            return response;
        }


       //退群消息
        private bool func_11099(WxHelper helper, GroupMembersWxPacket packaget)
        {
            bool response = true;
            try
            {
                if (groupBind == null)
                    return false;

                if (groupBind.wxid == packaget.room_wxid)
                {
                    foreach(var pm in packaget.member_list)
                    {
                        var member = v2Memberbindlite.FirstOrDefault(p => p.wxid == pm.wxid);
                        if(member != null)
                        {
                            member.State = MemBerState.已退群;
                            loglite.Add(Log.Create($"WxMessage::退群::{groupBind.nickname}", $"{pm.wxid}::{pm.nickname}::已退群"));
                        }
                    }
                }
            }
            catch
            {
                response = false;
            }

            return response;
        }


        public int LexicalMember查流水(BoterWxGroupMessage msgpack, V2Member m)
        {
            //string regexStr = @"([\u4e00-\u9fa5]+)(\d*)([^#]*)";
            //(查|流水)*
            string regexStr = "(查|流水){1}(\\d*)([^#]*)";
            //string content = "上1000.0";

            string msg = msgpack.msg;
            if(msg == "查" || msg == "流水" || msg == "货单")
            {
                var member = v2Memberbindlite.FirstOrDefault(p=>p.wxid == msgpack.from_wxid);
                string sendTxt = $"@{member.nickname}\r流~~记录\r";
                sendTxt += $"今日/本轮进货:{member.BetToday}/{member.BetCur}\r";
                sendTxt += $"今日上/下:{member.CreditToday}/{member.WithdrawToday}\r";
                sendTxt = sendTxt + $"今日盈亏:" + (this._appSetting.Zsxs ? ((int)member.IncomeToday).ToString() : member.IncomeToday.ToString("F2")) + "\r";
                wxHelper.CallSendText_11036(member.GroupWxId, sendTxt);
                return 0;
            }

            return -1;
        }


        //订单取消
        private int LexicalMemberOrderCencel(BoterWxGroupMessage msgpack, int issueid, V2Member m)
        {
            //string regexStr = "(取消|全部取消){1}";
            //bool bret = Regex.IsMatch(msgpack.msg, regexStr, RegexOptions.IgnoreCase);
            //var items = Regex.Match(msgpack.msg, regexStr, RegexOptions.IgnoreCase);
            //var st0 = items.Groups[0].Value;
            //var st1 = items.Groups[1].Value;    //中文(玩法)  只有上或者下
            try
            {
                string msg = msgpack.msg;
                if (!string.IsNullOrEmpty(msg))
                {
                    if (msg == "取消")
                    {
                        //查找订单
                        var orders = v2memberOderbindlite.Where(p => p.IssueId == issueid
                         && p.wxid == m.wxid
                         && p.OrderStatus != OrderStatusEnum.已取消
                         && p.OrderStatus != OrderStatusEnum.已完成
                         ).ToList();

                        if(orders != null)
                        {
                            if(orders.Count > 0)
                            {
                                var ods = orders.Last();    //一定不可能是空
                                if (_status == BoterStatus.开盘中)
                                {
                                    if (OrderCancel(ods, m))
                                    {
                                        OnOrderMoneyLimitCacel(issueid, ods);
                                        wxHelper.CallSendText_11036(ods.GroupWxId, $"@{m.nickname} {ods.BetContentOriginal}\r已取消!\r+{ods.AmountTotal}|留:{(int)m.Balance}");
                                    }
                                }
                                else
                                {
                                    wxHelper.CallSendText_11036(ods.GroupWxId, $"@{m.nickname} 时间到!不能取消!");
                                }
                            }
                        }

                        return 0;
                    }
                    else if(msg == "全部取消" || msg == "取消全部")
                    {
                        //查找订单
                        var orders = v2memberOderbindlite.Where(p => p.IssueId == issueid
                         && p.wxid == m.wxid
                         && p.OrderStatus != OrderStatusEnum.已取消
                         && p.OrderStatus != OrderStatusEnum.已完成
                         ).ToList();

                        if (orders != null)
                        {
                            if (orders.Count > 0)
                            {
                                if (_status == BoterStatus.开盘中)
                                {
                                    StringBuilder sbTxt = new StringBuilder(32);
                                    sbTxt.Append($"@{m.nickname} ");
                                    string groupid = "";
                                    float money = 0f;
                                    foreach (var ods in orders)
                                    {
                                        if (string.IsNullOrEmpty(groupid))
                                            groupid = ods.GroupWxId;

                                        if (OrderCancel(ods, m))
                                        {
                                            OnOrderMoneyLimitCacel(issueid, ods);
                                            money = money + ods.AmountTotal;
                                            sbTxt.Append($"{ods.BetContentOriginal}|+{ods.AmountTotal} 已取消\r");
                                        }
                                    }
                                    wxHelper.CallSendText_11036(groupid, sbTxt.ToString());
                                }
                                else
                                {
                                    wxHelper.CallSendText_11036(msgpack.room_wxid, $"{m.nickname}\r时间到!不能取消!");
                                }
                                // var ods = orders.LastOrDefault();    //一定不可能是空
                            }
                        }

                        return 0;
                    }
                }
            }
            catch(Exception ex)
            {
                loglite.Add(Log.Create($"LexicalMemberOrderCencel::异常", $"{msgpack.msg}"));
            }
            return -1;
        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="ods">一定要是窗口里面的 订单对象, 不能是自己生成的</param>
      /// <param name="m">这个也一定要是窗口里面的对象, 不能是自己生成的 </param>
      /// <returns></returns>
        private bool OrderCancel(V2MemberOrder ods, V2Member m)
        {
            /*
             *             float money = member_order.AmountTotal;
                           this.Balance = this.Balance - money;
                           this.BetCur = this.BetCur + money;      //本期投注.. 这个在期号变更时候, 清0
                           this.BetToday = this.BetToday + money;  //今日投注
                           this.BetTotal = this.BetTotal + money;  //总下注
             */
            ods.OrderStatus = OrderStatusEnum.已取消;
            m.Balance += ods.AmountTotal;
            m.BetCur -= ods.AmountTotal;
            m.BetToday -= ods.AmountTotal;
            m.BetTotal -= ods.AmountTotal;
            OnMemberOrderCancel(ods);
            return true;
        }

        /// <summary>
        ///     收单
        ///     会员词法分析.投注
        ///     错误码 600100
        ///     这个函数外部是带状态锁的，可以安全获取状态 _status
        /// </summary>
        private int LexicalMemberBet(BoterWxGroupMessage msgpack, int issueid, V2Member m)
        {
            //错误代码 10080开头

            // strBet = "上500.00啊"; 
            //  strBet = "合单500";
            //string regexStr = @"\b大\b"; //检测字符串边界
            //  string regexStr = @"(\d*)([\u4e00-\u9fa5]+)(\d*)([^#]*)";
            //string regexStr = "(\\d*)([大小单双]?|[合单]{0,2}|[合双]{0,2}|[大单]){1}(\\d*)";

            //var items = Regex.Match(strBet, regexStr, RegexOptions.IgnoreCase);
            //var st0 = items.Groups[0].Value;
            //var st1 = items.Groups[1].Value;    //车号, 有可能是空, 代表6车
            //var st2 = items.Groups[2].Value;    //中文(玩法)
            //var st3 = items.Groups[3].Value;    //金额
            //var st4 = items.Groups[4].Value;    //多余检测, 这个不为空, 就不去处理, 可以有别的其他非法内容

            //匹配校验规则
            // 6|总|合  为和值下注
            //123大单100|123小单100|123小双100|123大双(对应:123大, 123单, 共6注)
            //123大100|123小100|123单100|123双100
            //单500(对应:总单500)
            //6单100, 总和单100, 合单100, 6单100, 总和双100 => 都对应标准的  6单100  
            //6大100|总和大100|合大100(对应都表示:总双100)
            //龙100
            //虎100
            //123尾大100|123尾小100(123个车都下)
            //支持语法-----------------------
            //12大100小50单100
            //123大单双100
            //123大单4双100 (老版机器人语法)
            //1大3小4双10  = 1大3,1小4,双10(新机器人语法)


            LexicalMemberResult result = null;      //解析成注单
            string strContentMsg = msgpack.msg.Trim();      //进行空格验证::清除左右两边空格
            if (strContentMsg.Contains(' '))
            {
                var member = v2Memberbindlite.FirstOrDefault(p => p.wxid == msgpack.from_wxid);
                if (member != null)
                    wxHelper.CallSendText_11036(member.GroupWxId
                        , $"@{member.nickname} {Reply_收单失败},10080");

                //未处理返回 -1, 好给其他关键字匹配处理
                return -1;
            }

            //订单转换
            //123大单100, 和大100, 转换成 1大100,2大100,3大100,1单10,2单100,
            //string msg = "1123大单龙4小5单尾大100";    //测试使用
            BoterBetContents betcontents = new BoterBetContents(issueid, msgpack.msg);
            if (betcontents.code != 0)
            {
                if(betcontents.code != -1)
                    On收单失败(msgpack, m, betcontents.lasterror);
                //关键字匹配, 处理了, 返回>0的错误码, 这样出去后其他关键字就不匹配了
                //其实只要是没匹配失败, 恒定返回-1
                return betcontents.code;
            }
            
            var member_order = new V2MemberOrder(
                  m
                , issueid
                , msgpack.timestamp
                , betcontents.msg_origin
                , betcontents.ToStandarString()
                , betcontents.GetCount()
                , betcontents.GetAmountTatol()
                )
            {
                OrderStatus = OrderStatusEnum.待处理,
                GroupWxId = m.GroupWxId
            };
            //校验订单金额是否在合理范围
            

            if (m.Balance >= member_order.AmountTotal)
            {  
                if(_status == BoterStatus.开盘中)
                {
                    //检验订单金额是否在许可范围内.检查订单是否超过允许的购买量。超过多人购买的限额
                    if(!OnOrderMoneyLimitCheck(issueid, member_order))
                    {
                        return 0;
                    }

                    //把单子放入订单表中
                    //下面3个方法应该要封装成, 下单函数, 方便其他地方调用，以后待定
                    member_order.BetFronMoney = m.Balance;
                    m.AddOrder(member_order);   //添加订单.  开奖后,要进行订单结算
                    member_order.BetAfterMoney = m.Balance;
                    if (m.State == MemBerState.托)
                    {
                        member_order.OrderType = OrderTypeEnum.托;
                    }

                    if(v2memberOderbindlite.Add(member_order, OnMemberOrderCreate))    //解析用户订单, 添加订单
                    {
                        wxHelper.CallSendText_11036(msgpack.room_wxid, $"@{m.nickname}\r已进仓{member_order.Nums}\r{betcontents.ToReplyString()}|扣:{member_order.AmountTotal}|留:{(int)m.Balance}");
                    }
                    else
                    {
                        OrderCancel(member_order, m); //记录日志
                        wxHelper.CallSendText_11036(msgpack.room_wxid, $"@{m.nickname}\r未进仓!订单重复!如需重复订单,请稍后一秒后再次下单!{member_order.Nums}\r扣:0|留:{(int)m.Balance}");
                        _loglite.Add(Log.Create($"订单重复::订单信息::{member_order.IssueId}", JsonConvert.SerializeObject(member_order)));
                        _loglite.Add(Log.Create($"订单重复::会员信息恢复::{member_order.IssueId}", JsonConvert.SerializeObject(m)));
                    }
                }
                else
                {
                    wxHelper.CallSendText_11036(msgpack.room_wxid, $"{m.nickname}\r时间未到!不收货!");
                }
            }
            else
            {
                wxHelper.CallSendText_11036(msgpack.room_wxid, $"@{m.nickname} {Reply_余额不足}");
            }


            return 0;
        }

        Dictionary<string, int> _OrderLimitDic = new Dictionary<string, int>();
        private bool OnOrderMoneyLimitCheck(int issueid, V2MemberOrder memberOrder)
        {
            //_OrderLimitDic.Clear();
            BoterBetContents bets = new BoterBetContents(issueid, memberOrder.BetContentStandar);
            bool response = true;   //默认返回真
            Dictionary<string, int> dicTmp = new Dictionary<string, int>();
            foreach (var betitem in bets.boterItems)
            {
                string key = $"{betitem.car}{betitem.play}";
                var dicObj = _OrderLimitDic.FirstOrDefault(p=>p.Key == key);
                if(string.IsNullOrEmpty(dicObj.Key))
                {
                    if (betitem.moneySum < this._appSetting.wxMinBet)
                    {
                        wxHelper.CallSendText_11036(memberOrder.GroupWxId, $"@{memberOrder.nickname} 进仓失败!{key}不能小于{this._appSetting.wxMinBet}");
                        response = false;
                        break;
                    }
                    if(betitem.moneySum > this._appSetting.wxMaxBet)
                    {
                        wxHelper.CallSendText_11036(memberOrder.GroupWxId, $"@{memberOrder.nickname} 进仓失败!{key}超限,当前{betitem.moneySum},剩:{this._appSetting.wxMinBet}");
                        response = false;
                        break;
                    }
                    _OrderLimitDic.Add(key, betitem.moneySum);
                }
                else
                {
                    if (betitem.moneySum < this._appSetting.wxMinBet)
                    {
                        wxHelper.CallSendText_11036(memberOrder.GroupWxId, $"@{memberOrder.nickname} 进仓失败!{key}不能小于{this._appSetting.wxMinBet}");
                        response = false;
                        break;
                    }
                    int maxLimit = this._appSetting.wxMaxBet - dicObj.Value;
                    if (betitem.moneySum > maxLimit)
                    {
                        wxHelper.CallSendText_11036(memberOrder.GroupWxId, $"@{memberOrder.nickname} 进仓失败!{key}超限,当前{betitem.moneySum},剩余:{maxLimit}");
                        response = false;
                        break;
                    }

                    _OrderLimitDic[key] = _OrderLimitDic[key] + betitem.moneySum;
                }

                //记录金额
                string keyvalue = key;
                var dtmp = dicTmp.FirstOrDefault(p=>p.Key == keyvalue);
                if (string.IsNullOrEmpty(dtmp.Key))
                    dicTmp.Add(key, betitem.moneySum);
                else
                    dicTmp[keyvalue] = dicTmp[keyvalue] + betitem.moneySum;
            }

            if(!response)
            {
                //把额度还原
                try
                {
                    foreach (var item in dicTmp)
                    {
                        _OrderLimitDic[item.Key] = _OrderLimitDic[item.Key] - item.Value;
                    }
                }
                catch(Exception ex)
                {
                    _loglite.Add(Log.Create($"订单校验::额度还原异常", ex.Message));
                }
            }

            return response;
        }

        /// <summary>
        ///     恢复订单额度, 订单取消时候, 恢复额度
        /// </summary>
        /// <param name="issueid"></param>
        /// <param name="memberOrder"></param>
        /// <returns></returns>
        private bool OnOrderMoneyLimitCacel(int issueid, V2MemberOrder memberOrder)
        {
            bool response = false;
            try
            {
                if(memberOrder.OrderStatus == OrderStatusEnum.已取消 && issueid == memberOrder.IssueId)
                {
                    BoterBetContents bets = new BoterBetContents(issueid, memberOrder.BetContentStandar);
                    foreach (var betitem in bets.boterItems)
                    {
                        string key = $"{betitem.car}{betitem.play}";
                        var dicObj = _OrderLimitDic.FirstOrDefault(p => p.Key == key);
                        if (string.IsNullOrEmpty(dicObj.Key))
                        {
                            //不可能没有。这里抛出异常。并且记录。
                        }   
                        else
                        {
                            _OrderLimitDic[key] = _OrderLimitDic[key] - betitem.moneySum;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                response = false;
            }
            return response;
        }

        /// <summary>
        ///     默认播放sound里面的mp3文件, 参数是文件名
        /// </summary>
        public void PlayMp3(string fileName)
        {
            MP3Play cm = new MP3Play();   //播放上
            cm.FileName = $"{Environment.CurrentDirectory}\\sound\\{fileName}";
            cm.play();
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="func"></param>
        /// <returns>-1表示不匹配关键字, 未处理，继续下个处理逻辑</returns>
        ///    0表示已处理, 并且处理成功, 完成
        ///    1表示处理失败, >0的表示错误代码
        public int LexicalMember上下分(BoterWxGroupMessage msgpack, V2Member m)
        {
            //string regexStr = @"([\u4e00-\u9fa5]+)(\d*)([^#]*)";
            string regexStr = "(上|下){1}(\\d*)(分*){1}";
            //string content = "上1000.0";
            bool bret = Regex.IsMatch(msgpack.msg, regexStr, RegexOptions.IgnoreCase);
            if (bret)
            {
                var items = Regex.Match(msgpack.msg, regexStr, RegexOptions.IgnoreCase);
                var st0 = items.Groups[0].Value;
                var st1 = items.Groups[1].Value;    //中文(玩法)  上分,下分
                var st2 = items.Groups[2].Value;    //金额
                var st3 = items.Groups[3].Value;

                int money = 0;
                V2MemberPayAction payAction = V2MemberPayAction.未知;

                try
                {
                    money = Convert.ToInt32(st2);
                }
                catch { return 400; }
                try
                {
                    if (st1 == "上")
                        st1 = "上分";
                    else if (st1 == "下")
                        st1 = "下分";
                    payAction = (V2MemberPayAction)Enum.Parse(typeof(V2MemberPayAction), st1);
                }
                catch { return 401; }

                if (payAction == V2MemberPayAction.上分)
                    PlayMp3("mp3_shang.mp3");       //播放mp3
                else if (payAction == V2MemberPayAction.下分)
                    PlayMp3("mp3_xia.mp3");

                //添加上下分订单。
                var coinsOrder = new V2MemberCoinsBuySell(m, msgpack.room_wxid, msgpack.timestamp, money, payAction, V2MemberPayStatus.等待处理);
                _v2MemberCoinsBuySellbindlite.Add(coinsOrder);
                _v2MemberCoinsBuySellbindliteUnProcessed.Add(coinsOrder);
                wxHelper.CallSendText_11036(msgpack.room_wxid, $"@{m.nickname}\r[{m.id}]请等待");

                return 0;
            }

            return -1;
        }

        private int LexicalAdmin刷新(BoterWxGroupMessage msgpack)
        {
            int response = -1;
            try
            {
                //确定是绑定的群再操作
                if (msgpack.msg.Replace(" ", "") == "刷新")
                {
                    Sub刷新(msgpack.room_wxid);
                    wxHelper.CallSendText_11036(msgpack.room_wxid, $"^刷新完成");
                }

            }
            catch (Exception ex)
            {

            }
            return response;
        }

        private void Sub刷新(string room_wxid)
        {
            if (this._groupBind.wxid == room_wxid)
            {
                //得到当前会员表
                var wret = wxHelper.wxServices.GetMemberList(room_wxid);
                if (wret.data != null)
                {
                    if (!string.IsNullOrEmpty(wret.data.members))
                    {
                        string memstr = wret.data.members.Replace("^G", "|");
                        var memarry = memstr.Split('|');
                        foreach (var mem in memarry)
                        {
                            Debug.WriteLine("ID:"+ mem);
                            var memonline = v2Memberbindlite.FirstOrDefault(p => p.GroupWxId == room_wxid && p.wxid == mem);
                            if (memonline == null)
                            {
                                Debug.WriteLine("找到新加入的成员::ID" + mem);
                                //有新会员进入
                                //查找是否是联系人
                                var m = wxHelper.wxContacts.FirstOrDefault(p => p.wxid == mem);
                                if (m == null)
                                {
                                    m = new WxContacts();
                                    m.wxid = mem;
                                    m.remark = "非会员";
                                }
                                else
                                {
                                    m.remark = "会员";
                                }
                                if (string.IsNullOrEmpty(m.nickname))
                                {
                                    var ss = wxHelper.wxServices.GetMemberNickname(m.wxid);
                                    m.nickname = ss.data;
                                }
                                //检查是否有名字
                                if (string.IsNullOrEmpty(m.nickname))
                                {
                                    var ss = wxHelper.wxServices.GetMemberNickname(m.wxid);
                                    m.nickname = ss.data;
                                }
                                //添加
                                _view.Invoke(new Action(() => {
                                    v2Memberbindlite.Add(room_wxid, new V2Member(m), new Func<V2Member, MemBerState>((p) => {
                                        if (p.wxid == this.wxHelper.wxid)
                                        {
                                            return MemBerState.管理;
                                        }
                                        if (p.remark == "会员")
                                            return MemBerState.会员;
                                        if (p.remark == "非会员")
                                            return MemBerState.非会员;

                                        return MemBerState.非会员;
                                    }));
                                }));

                                //得到ID
                                var memok = v2Memberbindlite.FirstOrDefault(p => p.GroupWxId == room_wxid && p.wxid == mem);

                                //系统回复
                                wxHelper.CallSendText_11036(room_wxid, $"^欢迎:[{memok.id}]{m.nickname}");
                            }
                            else
                            {

                            }
                          
                        }
                            // wxHelper.CallSendText_11036(room_wxid, $"^刷新完成");
                    }
                }
            }
        }

        //管理上下分
        private int LexicalAdmin上下分(BoterWxGroupMessage msgpack)
        {
            int response = -1;
            try
            {
                //"@飞扬🐠 上10000"
                //string msg = "@飞 扬  下1000.00";
                string regexStr = @"@([^ ]+).(上|下){1}(\d+)(.*)";
                bool brgx = Regex.IsMatch(msgpack.msg, regexStr);
                if(brgx)
                {
                    response = 201; //默认异常, 没意料到的
                    var ss = Regex.Match(msgpack.msg, regexStr);
                    string s1 = ss.Groups[1].Value; //名字
                    string s2 = ss.Groups[2].Value; //动作:上|下
                    string s3 = ss.Groups[3].Value; //金额
                    string s4 = ss.Groups[4].Value; //出错的字符, 这个有值，则不处理改命令

                    if (s4.Contains("留") || s4.Contains("余"))
                    {
                        return 0;
                    }

                    var items = v2Memberbindlite.Where(p => p.nickname == s1).ToList();
                    if(items == null)
                    {
                        response = 500;
                        //wxHelper.CallSendText_11036(msgpack.room_wxid, $"@{s1} {this.Reply_收单失败},{response}");
                        throw new Exception($"#[警告]没找到,{s1}");
                    }
                    if(items.Count > 1)
                    {
                        response = 501;
                        throw new Exception($"#[警告]重名,{s1}");
                    }

                    //金额
                    int money = 0;
                    try
                    {
                        money = Convert.ToInt32(s3);
                    }
                    catch { response = 502;  throw new Exception("#[警告]金额错误"); }


                    if(s2 == "上")
                    {
                        //添加一个订单。然后自动调用处理。
                        V2MemberCoinsBuySell order = new V2MemberCoinsBuySell(items[0], msgpack.room_wxid, msgpack.timestamp, money, V2MemberPayAction.上分, V2MemberPayStatus.等待处理);
                        order.Note = $"管理直上:{s1}";
                        //_v2MemberCoinsBuySellbindlite.Add(order);
                        this.OnActionMemberCredit(order);
                        //items[0].上分(money, OnMemberCreditSuccess);
                    }
                    else if(s2 == "下")
                    {
                        //items[0].下分(money, OnMemberWithdrawSuccess);
                        V2MemberCoinsBuySell order = new V2MemberCoinsBuySell(items[0], msgpack.room_wxid, msgpack.timestamp, money, V2MemberPayAction.下分, V2MemberPayStatus.等待处理);
                        order.Note = $"管理直下:{s1}";
                        //_v2MemberCoinsBuySellbindlite.Add(order);
                        this.OnActionMemberWithdraw(order);
                    }
                    else
                    {
                        //出错了.按道理讲是不可能错误的, 因为正则匹配过了
                        response = 404;
                        throw new Exception("#无效动作!");
                    }

                    wxHelper.CallSendText_11036(msgpack.room_wxid, $"@{items[0].nickname}\r{items[0].id}{s2}{money}|余:{items[0].Balance}");
                    return 0;
                }

                //第二种格式
                regexStr = @"(\d+)(上|下){1}(\d+)(.*)";
                brgx = Regex.IsMatch(msgpack.msg, regexStr);
                if (brgx)
                {
                    response = 201; //默认异常, 没意料到的
                    var ss = Regex.Match(msgpack.msg, regexStr);
                    string s1 = ss.Groups[1].Value;  //7
                    string s2 = ss.Groups[2].Value;  //上
                    string s3 = ss.Groups[3].Value;  //1000
                    string s4 = ss.Groups[4].Value;

                    int id = Convert.ToInt32(s1);
                    var item = v2Memberbindlite.FirstOrDefault(p=>p.id == id);
                    if(item == null)
                    {
                        throw new Exception("#[警告]ID错误, ID不存在");
                    }

                    //金额
                    int money = 0;
                    try
                    {
                        money = Convert.ToInt32(s3);
                    }
                    catch { response = 502; throw new Exception("#[警告]金额错误"); }

                    if (s2 == "上")
                    {
                        //item.上分(money, OnMemberCreditSuccess);
                        V2MemberCoinsBuySell order = new V2MemberCoinsBuySell(item, msgpack.room_wxid, msgpack.timestamp, money, V2MemberPayAction.上分, V2MemberPayStatus.等待处理);
                        order.Note = $"管理直上:{id}";
                        //_v2MemberCoinsBuySellbindlite.Add(order);
                        this.OnActionMemberCredit(order);
                    }
                    else if (s2 == "下")
                    {
                        V2MemberCoinsBuySell order = new V2MemberCoinsBuySell(item, msgpack.room_wxid, msgpack.timestamp, money, V2MemberPayAction.下分, V2MemberPayStatus.等待处理);
                        order.Note = $"管理直下:{id}";
                        //_v2MemberCoinsBuySellbindlite.Add(order);
                        bool order_ok = this.OnActionMemberWithdraw(order);
                        if(!order_ok)
                        {
                            return 0;
                        }
                        //item.下分(money, OnMemberWithdrawSuccess);
                    }
                    else
                    {
                        //出错了.按道理讲是不可能错误的, 因为正则匹配过了
                        response = 404;
                        throw new Exception("#无效动作!");
                    }

                    wxHelper.CallSendText_11036(msgpack.room_wxid, $"@{item.nickname}\r{item.id}{s2}{money}|余:{item.Balance}");
                    return 0;
                }

            }
            catch(Exception ex)
            {
                //记录到日志里..
                if (ex.Message[0] == '#')
                {
                    wxHelper.CallSendText_11036(msgpack.room_wxid, $"处理失败{response},{ex.Message}");
                }
            }
            return response;
        }


        private void On收单失败(BoterWxGroupMessage msgpack, V2Member m, string lasterror)
        {
            wxHelper.CallSendText_11036(msgpack.room_wxid, $"@{m.nickname} {lasterror}");
        }

        /// <summary>
        ///     新补分::0702新加入的, 以这里补分为准,其他的要慢慢移动过来
        ///     返回值
        ///         返回处理成功的字符串  成功|会员-订单信息-盈利-余额
        ///                               失败|原因
        /// </summary>
        public bool On补分(string member_wxid, int order_id, BgLotteryData bgData, float odds, out string msg)
        {
            msg = "";
            bool response = true;
            try
            {
                var order = this.v2memberOderbindlite.FirstOrDefault(p => p.id == order_id);
                if (order == null)
                    throw new Exception($"没找到单号:{order_id}");

                var member = this.v2Memberbindlite.FirstOrDefault(p=>p.wxid == member_wxid && p.GroupWxId == groupBind.wxid);
                string member_name = "";
                if (member == null)
                    member_name = "无";
                else
                    member_name = member.nickname;

                   // throw new Exception($"没找到会员");
                order.OpenLottery(bgData, odds, _appSetting.Zsjs);
                int member_last_money = (int)order.BetAfterMoney + (int)order.Profit;
                msg = $"成功|{member_name},{order.BetContentOriginal}, 始于:{(int)order.BetFronMoney}, 得:{order.NetProfit}, 余:{member_last_money}";
            }
            catch(Exception ex)
            {
                response = false;
                msg = $"失败|{ex.Message}";
            }

            return response;
        }
    }
}
