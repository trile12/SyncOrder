using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ToolSyncOrder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<LogInfo> orderInfos;
        private List<LogInfo> filteredOrders;

        public MainWindow()
        {
            InitializeComponent();
            orderInfos = new ObservableCollection<LogInfo>();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Log Files|*.log";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                Refresh();
                foreach (string filePath in openFileDialog.FileNames)
                {
                    ReadLogFile(filePath);
                }
            }
        }

        private void ReadLogFile(string filePath)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    ParseLogLine(line);
                }

                var checkOrders = orderInfos.Where(x => !string.IsNullOrEmpty(x.OrderId)).ToList();
                orderInfos = new ObservableCollection<LogInfo>(checkOrders);
                logListView.ItemsSource = orderInfos;
                NumberOrderTextBlock.Text = orderInfos.Count().ToString();
                filteredOrders = new List<LogInfo>(orderInfos);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading log file: " + ex.Message);
            }
        }

        string authorizationPattern = @"Authorization: (\S+)";
        string endpointPattern = @"(https://\S+?\.(livedevs|eposdata)\.com/api)";

        private void ParseLogLine(string line)
        {
            if (line.Contains("/api"))
            {
                var orderInfo = new LogInfo();
                string authorizationToken = ExtractValue(line, authorizationPattern);
                string endpointUrl = ExtractValue(line, endpointPattern);

                orderInfo.AuthorizationToken = authorizationToken;
                orderInfo.Endpoint = endpointUrl;
                orderInfos.Add(orderInfo);
            }

            if (line.Contains("RequestOrderSync") && line.Contains("[ORDER] Sync Request Data"))
            {
                int startIndex = line.IndexOf("{");
                int endIndex = line.LastIndexOf("}");

                if (startIndex != -1 && endIndex != -1)
                {
                    string jsonData = line.Substring(startIndex, endIndex - startIndex + 1);

                    try
                    {
                        var requestData = JsonConvert.DeserializeObject<JObject>(jsonData);

                        string orderId = requestData["data"]["id"].ToString();
                        string orderNo = requestData["data"]["attributes"]["order_no"].ToString();
                        //string tenant = requestData["data"]["attributes"]["session_token"].ToString();
                        string orderStatus = requestData["data"]["attributes"]["status"].ToString();
                        string shiftId = requestData["data"]["attributes"]["shift_id"].ToString();

                        var order = orderInfos.LastOrDefault(x => string.IsNullOrEmpty(x.OrderId));
                        if (order != null)
                        {
                            order.OrderId = orderId;
                            order.OrderNo = orderNo;
                            order.JsonData = requestData.ToString();
                            order.ShiftId = shiftId;
                            order.OrderStatus = orderStatus;
                            order.IsVoidOrder = orderStatus == "void";
                            order.Endpoint += "/operations/orders";
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine("Error parsing JSON: " + ex.Message);
                    }
                }
            }

        }

        private string ExtractValue(string input, string pattern)
        {
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        static List<LogInfo> FilterStrings(ObservableCollection<LogInfo> inputList, List<string> numbersToFilter)
        {
            List<LogInfo> filteredList = new List<LogInfo>();
            foreach (var input in inputList)
            {
                if (numbersToFilter.Any(number => input.OrderId.Contains(number) || input.OrderNo.Contains(number)))
                {
                    filteredList.Add(input);
                }
            }

            return filteredList;
        }

        private void orderIdFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty((e.OriginalSource as TextBox)?.Text))
            {
                filteredOrders = orderInfos.ToList();
                return;
            }
            List<string> filterInput = orderIdFilterTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            filteredOrders = FilterStrings(orderInfos, filterInput);
            NumberOrderTextBlock.Text = filteredOrders.Count().ToString();
            logListView.ItemsSource = filteredOrders;
        }

        private async Task<bool> RestCreateOrderAsync(LogInfo logInfo)
        {
            if (logInfo == null)
                return false;

            var client = new RestClient(logInfo.Endpoint);
            var request = new RestRequest();

            request.Method = Method.Post;

            request.AddHeader("Authorization", logInfo.AuthorizationToken);
            request.AddHeader("Content-Type", "application/vnd.api+json");
            request.AddHeader("Accept", "application/vnd.api+json");
            request.AddParameter("application/vnd.api+json", logInfo.JsonData, ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);
            return response.IsSuccessful;
        }

        private async void SendOrder_Click(object sender, RoutedEventArgs e)
        {
            foreach (var order in filteredOrders)
            {
                var isSuccess = await RestCreateOrderAsync(order);
                order.IsSuccessful = isSuccess;
            }
        }

        private void Refresh()
        {
            orderInfos?.Clear();
            filteredOrders?.Clear();
            logListView.ItemsSource = null;
            orderIdFilterTextBox.Text = "";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateManager.GetVersionInfo();
            var currentVersion = UpdateManager.GetCurrentVersion();

            if (UpdateManager.IsUpdateAvailable(currentVersion))
            {               
                UpdateManager.PromptForUpdate();
            }
        }
    }

    public class LogInfo : INotifyPropertyChanged
    {
        private bool _isSuccessful;
        private bool _isVoidOrder;

        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public string AuthorizationToken { get; set; }
        public string Endpoint { get; set; }
        public string JsonData { get; set; }
        public string OrderStatus { get; set; }
        public string ShiftId { get; set; }

        public bool IsVoidOrder
        {
            get { return _isVoidOrder; }
            set
            {
                if (_isVoidOrder != value)
                {
                    _isVoidOrder = value;
                    OnPropertyChanged(nameof(_isVoidOrder));
                }
            }
        }

        public bool IsSuccessful
        {
            get { return _isSuccessful; }
            set
            {
                if (_isSuccessful != value)
                {
                    _isSuccessful = value;
                    OnPropertyChanged(nameof(IsSuccessful));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
