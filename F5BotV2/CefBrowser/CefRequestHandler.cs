using CefSharp.Handler;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.CefBrowser
{

    public delegate void CefCallBack<T>(object sender, T args);

    public class CefRequestHandler
        : IRequestHandler
    {
        private IRequestHandler _instance;
        private CefCallBack<ResponseEventArgs> _ResponseCompletionCallBack;
        public CefRequestHandler(CefCallBack<ResponseEventArgs> responseCompletion)
        {
            this._ResponseCompletionCallBack = responseCompletion;
            _instance = new RequestHandler();
        }

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            return _instance.GetAuthCredentials(chromiumWebBrowser, browser, originUrl, isProxy, host, port, realm, scheme, callback);
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            //数据加载, js请求, 都走这里得到数据的
            //var source = _instance.GetResourceRequestHandler(chromiumWebBrowser, browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
            //这里可以增加一个事件
            return new CefResourceRequestHandler(_ResponseCompletionCallBack);
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            return _instance.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
        }

        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            return _instance.OnCertificateError(chromiumWebBrowser, browser, errorCode, requestUrl, sslInfo, callback);
        }

        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            _instance.OnDocumentAvailableInMainFrame(chromiumWebBrowser, browser);
        }

        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return _instance.OnOpenUrlFromTab(chromiumWebBrowser, browser, frame, targetUrl, targetDisposition, userGesture);
        }

        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
        {
            _instance.OnRenderProcessTerminated(chromiumWebBrowser, browser, status);
        }

        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            _instance.OnRenderViewReady(chromiumWebBrowser, browser);
        }

        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return _instance.OnSelectClientCertificate(chromiumWebBrowser, browser, isProxy, host, port, certificates, callback);
        }
    }
}
