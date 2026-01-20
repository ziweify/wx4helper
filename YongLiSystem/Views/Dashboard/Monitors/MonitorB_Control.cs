using System.Threading.Tasks;

namespace YongLiSystem.Views.Dashboard.Monitors
{
    /// <summary>
    /// 监控B控件
    /// </summary>
    public partial class MonitorB_Control : MonitorControlBase
    {
        protected override string MonitorName => "监控B";

        public MonitorB_Control()
        {
            InitializeComponent();
            InitializeUI(); // 初始化UI控件
        }
    }
}
