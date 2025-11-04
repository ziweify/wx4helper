namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 会员状态
    /// </summary>
    public enum MemberState
    {
        已删除 = -1,
        非会员 = 0,
        会员 = 1,
        托 = 2,
        管理 = 3,
        已退群 = 4
    }

    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
    {
        未知 = -1,
        待处理 = 0,     // 订单进来了，但是没有投递进网盘
        待结算 = 1,
        已完成 = 2,     // 结算完成
        已取消 = 3
    }

    /// <summary>
    /// 订单类型
    /// </summary>
    public enum OrderType
    {
        待定 = 0,
        盘内 = 1,       // 进盘的，就是盘内
        盘外 = 2,       // 没进盘的，就是盘外
        托 = 3          // 就是不打进盘的
    }

    /// <summary>
    /// 浏览器状态
    /// </summary>
    public enum BrowserState
    {
        未初始化 = 0,
        已初始化 = 1,
        已登录 = 2,
        登录失败 = 3,
        连接断开 = 4
    }

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        文本 = 1,
        图片 = 2,
        语音 = 3,
        视频 = 4,
        文件 = 5,
        系统 = 99
    }
}

