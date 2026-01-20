using System.Threading.Tasks;

namespace 永利系统.Views.Dashboard.Monitors
{
    /// <summary>
    /// 监控A控件 - 台湾彩票监控
    /// </summary>
    public partial class MonitorA_Control : MonitorControlBase
    {
        protected override string MonitorName => "监控A";

        public MonitorA_Control()
        {
            InitializeComponent();
            InitializeUI(); // 初始化UI控件
        }
    }
}
