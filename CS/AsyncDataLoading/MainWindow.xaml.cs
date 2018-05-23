using AsyncDataLoading.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace AsyncDataLoading {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        ServiceReference1.OrderServiceClient srv;
        public MainWindow() {
            srv = new ServiceReference1.OrderServiceClient();
            srv.GetRecordsCountCompleted += new EventHandler<GetRecordsCountCompletedEventArgs>(srv_GetRecordsCountCompleted);
            srv.GetRecordsCompleted += new EventHandler<GetRecordsCompletedEventArgs>(srv_GetRecordsCompleted);
            InitializeComponent();
        }
        void OnRequestCount(RequestCountArgs request, RequestCountResult feedback) {
            srv.GetRecordsCountAsync(new GetRecordsCountRequest(), feedback);
        }
        void srv_GetRecordsCountCompleted(object sender, GetRecordsCountCompletedEventArgs e) {
            RequestCountResult feedback = e.UserState as RequestCountResult;
            if (feedback == null) return;
            feedback.RaiseFeedback(e.Result.GetRecordsCountResult);
        }

        void OnRequestData(RequestDataArgs request, RequestDataResult feedback) {
            srv.GetRecordsAsync(new GetRecordsRequest(request.SkipCount, request.TakeCount), feedback);
        }
        void srv_GetRecordsCompleted(object sender, GetRecordsCompletedEventArgs e) {
            RequestDataResult feedback = e.UserState as RequestDataResult;
            if (feedback == null) return;
            feedback.RaiseFeedback(e.Result.GetRecordsResult);
        }
    }
}
