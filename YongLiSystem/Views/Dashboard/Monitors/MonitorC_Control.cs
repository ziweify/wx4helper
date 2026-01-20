using System.Threading.Tasks;

namespace YongLiSystem.Views.Dashboard.Monitors
{
    /// <summary>
    /// 监控C控件
    /// </summary>
    public partial class MonitorC_Control : MonitorControlBase
    {
        protected override string MonitorName => "监控C";

        public MonitorC_Control()
        {
            InitializeComponent();
            InitializeUI(); // 初始化UI控件
        }
    }
}
