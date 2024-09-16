using Caro_Online.Helper;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Caro_Online
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private static readonly HttpClient client = new HttpClient();

        private bool isGameReset;
        ObservableCollection<OptionDetail> _Maps;
        public ObservableCollection<OptionDetail> Maps
        {
            get { return _Maps; }
            set { _Maps = value; OnPropertyChanged(); }
        }

        string _StatusMessage;
        public string StatusMessage
        {
            get { return _StatusMessage; }
            set { _StatusMessage = value; OnPropertyChanged(); }
        }

        string _NotiStatusMessage;
        public string NotiStatusMessage
        {
            get { return _NotiStatusMessage; }
            set { _NotiStatusMessage = value; OnPropertyChanged(); }
        }

        string _RoomMessage;
        public string RoomMessage
        {
            get
            {
                return _RoomMessage;
            }
            set
            {
                _RoomMessage = value; OnPropertyChanged();
            }
        }

        EStatus currentTurn;
        HubConnection connection;
        string connectionId;
        int roomId;

        async Task ConnectToSignalR()
        {
            connection = new HubConnectionBuilder()
                .WithUrl(MySetting.API_URL + "/chathub") // Your server URL
                .Build();

            // Handle incoming messages
            connection.On<int, EStatus>("MoveToPosition", (pos, status) =>
            {
                currentTurn = status;
                MakeAMove(Maps[pos]);
            });

            connection.On<string>("RoomFull", (message) =>
            {
                RoomMessage = message;
            });

            connection.On<string, int>("UserJoined", (connectionId, roomId) =>
            {
                RoomMessage = "Participant: " + connectionId;
            });

            connection.On<int, EStatus>("CheckWin", (pos, checkTurn) =>
            {
                CheckWin(pos, checkTurn);
                currentTurn = checkTurn;
            });

            connection.On<EStatus>("NotiStatus", (message) =>
            {
                NotiStatusMessage = "Nguoi choi danh: " + message.ToString();
            });

            connection.On<int>("ChangeTurn", (turn) =>
            {
                currentTurn = (EStatus)turn;
                UpdateStatus();
            });

            connection.On<string>("ReceiveConnectionId", (id) =>
            {
                connectionId = id;  // Store the ConnectionId
                RoomMessage = $"Your Connection ID is: {connectionId}";
            });

            connection.On<string>("ResetGame", (message) =>
            {
                //MessageBox.Show(message);  // Notify the user that the game has been reset
                FirstLoad();  // Reset the game map on the client side
            });

            await connection.StartAsync();
            await connection.SendAsync("ReceiveConnectionId");
        }

        #region call API
        public async Task JoinRoomViaApi()
        {
            try
            {
                var content = new StringContent($"\"{connectionId}\"", Encoding.UTF8, "application/json");  // Pass the stored ConnectionId

                // Call the API to join the room
                var response = await client.PostAsync(MySetting.API_URL + $"/api/rooms/join/{roomId}", content);

                response.EnsureSuccessStatusCode();  // Ensure the request was successful

                var responseBody = await response.Content.ReadAsStringAsync();
                RoomMessage = responseBody;  // Update UI or handle response
                FirstLoad();
            }
            catch (HttpRequestException e)
            {
                RoomMessage = $"Error joining room: {e.Message}";
            }
        }

        // Method to call the CreateRooms API
        public async Task CreateRoomsFromApi(int roomCapacity = 10)
        {
            try
            {
                // Replace with your actual API endpoint
                var response = await client.GetAsync(MySetting.API_URL + $"/api/Rooms/create?roomCapacity={roomCapacity}");

                response.EnsureSuccessStatusCode();  // Ensure the request was successful

                var responseBody = await response.Content.ReadAsStringAsync();
                //MessageBox.Show(responseBody);  // Update UI with the response
            }
            catch (HttpRequestException e)
            {
                RoomMessage = $"Error creating rooms: {e.Message}";
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            CreateRoomsFromApi();
            ConnectToSignalR();
            //FirstLoad();
        }

        void FirstLoad()
        {
            Maps = new ObservableCollection<OptionDetail>();
            for (int i = 0; i < 100; i++)
            {
                Maps.Add(new OptionDetail() { Status = EStatus.none });
            }
            currentTurn = EStatus.X;
            UpdateStatus();
            isGameReset = true; // Set flag to true on reset
        }

        void MakeAMove(OptionDetail data)
        {
            if (data.Status == EStatus.none)
            {
                data.Status = currentTurn;
            }
        }

        void UpdateStatus()
        {
            StatusMessage = currentTurn == EStatus.X ? "Den luot X" : "Den luot O";
        }

        #region check win algorithm
        //void CheckWinHorizontal(OptionDetail data)
        //{
        //    var currentIndex = Maps.IndexOf(data);
        //    var checkIndex = currentIndex;
        //    int cnt = 1;

        //    while (true)
        //    {
        //        checkIndex--;
        //        if (checkIndex % (int)Math.Sqrt(Maps.Count()) == (int)Math.Sqrt(Maps.Count()) - 1)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            if (Maps[checkIndex].Status == data.Status)
        //            {
        //                cnt++;
        //            }
        //            else { break; }
        //        }
        //    }
        //    checkIndex = currentIndex;
        //    while (true)
        //    {
        //        checkIndex++;
        //        if (checkIndex % (Math.Sqrt(Maps.Count())) == 0)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            if (Maps[checkIndex].Status == data.Status)
        //            {
        //                cnt++;
        //            }
        //            else { break; }
        //        }
        //    }
        //    if (cnt >= 5)
        //    {
        //        AnnounceWinner();
        //    }
        //}

        //void CheckWinVertical(OptionDetail data)
        //{
        //    var currentIndex = Maps.IndexOf(data);
        //    var checkIndex = currentIndex;
        //    int cnt = 1;

        //    while (true)
        //    {
        //        checkIndex -= (int)Math.Sqrt(Maps.Count());
        //        if (checkIndex < 0)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            if (Maps[checkIndex].Status == data.Status)
        //            {
        //                cnt++;
        //            }
        //            else { break; }
        //        }
        //    }
        //    checkIndex = currentIndex;
        //    while (true)
        //    {
        //        checkIndex += (int)Math.Sqrt(Maps.Count());
        //        if (checkIndex >= Maps.Count())
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            if (Maps[checkIndex].Status == data.Status)
        //            {
        //                cnt++;
        //            }
        //            else { break; }
        //        }
        //    }
        //    if (cnt >= 5)
        //    {
        //        AnnounceWinner();
        //    }
        //}

        //void CheckWin45Diognal(OptionDetail data)
        //{
        //    var currentIndex = Maps.IndexOf(data);
        //    var checkIndex = currentIndex;
        //    int cnt = 1;
        //    while (true)
        //    {
        //        checkIndex -= ((int)Math.Sqrt(Maps.Count()) - 1);
        //        if (checkIndex < 0 || checkIndex % ((int)Math.Sqrt(Maps.Count())) == 0)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            if (Maps[checkIndex].Status == data.Status)
        //            {
        //                cnt++;
        //            }
        //            else { break; }
        //        }
        //    }
        //    checkIndex = currentIndex;
        //    while (true)
        //    {
        //        checkIndex += ((int)Math.Sqrt(Maps.Count()) - 1);
        //        if (checkIndex > Maps.Count() || checkIndex % ((int)Math.Sqrt(Maps.Count())) == ((int)Math.Sqrt(Maps.Count())) - 1)
        //        // 
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            if (Maps[checkIndex].Status == data.Status)
        //            {
        //                cnt++;
        //            }
        //            else { break; }
        //        }
        //    }

        //    if (cnt >= 5) { AnnounceWinner(); }
        //}

        //void CheckWin135Diognal(OptionDetail data)
        //{
        //    var currentIndex = Maps.IndexOf(data);
        //    var checkIndex = currentIndex;
        //    int cnt = 1;
        //    while (true)
        //    {
        //        checkIndex -= ((int)Math.Sqrt(Maps.Count()) + 1);
        //        if (checkIndex < 0 || checkIndex % ((int)Math.Sqrt(Maps.Count())) == ((int)Math.Sqrt(Maps.Count())) - 1)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            if (Maps[checkIndex].Status == data.Status)
        //            {
        //                cnt++;
        //            }
        //            else { break; }
        //        }
        //    }
        //    checkIndex = currentIndex;
        //    while (true)
        //    {
        //        checkIndex += ((int)Math.Sqrt(Maps.Count()) + 1);
        //        if (checkIndex > Maps.Count() || checkIndex % ((int)Math.Sqrt(Maps.Count())) == 0)
        //        // 
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            if (Maps[checkIndex].Status == data.Status)
        //            {
        //                cnt++;
        //            }
        //            else { break; }
        //        }
        //    }

        //    if (cnt >= 5) { AnnounceWinner(); }
        //}
        #endregion

        #region check win block both sides algorithm

        void CheckWinHorizontalBothSides(int pos, EStatus checkTurn)
        {
            var currentIndex = pos; // Current position in the 1D array
            var checkIndex = currentIndex;
            int size = (int)Math.Sqrt(Maps.Count); // Grid size (number of columns)
            var currentRow = currentIndex / size;
            var checkStatus = checkTurn == EStatus.X ? EStatus.O : EStatus.X;
            var cnt = 1;

            while (true)
            {
                checkIndex--;
                if (checkIndex / size != currentRow || checkIndex < 0)
                {
                    break;
                }
                else
                {
                    if (Maps[checkIndex].Status == checkStatus)
                    {
                        var anotherSide = checkIndex + 6;
                        if (anotherSide / size != currentRow)
                        {
                            break;
                        }
                        else if (Maps[anotherSide].Status == checkStatus)
                        {
                            return;
                        }
                    }
                    else if (Maps[checkIndex].Status == checkTurn)
                    {
                        cnt++;
                    }
                    else break;
                }
            }
            checkIndex = currentIndex;
            while (true)
            {
                checkIndex++;
                if (checkIndex / size != currentRow || checkIndex > Maps.Count())
                {
                    break;
                }
                else
                {
                    if (Maps[checkIndex].Status == checkTurn)
                    {
                        cnt++;
                    }
                    else break;
                }
            }
            if (cnt >= 5)
            {
                AnnounceWinner(checkTurn);
            }
        }

        void CheckWinVerticalBothSides(int pos, EStatus checkTurn)
        {
            var currentIndex = pos; // Current position in the 1D array
            var checkIndex = currentIndex;
            int size = (int)Math.Sqrt(Maps.Count); // Grid size (number of columns)
            var currentRow = currentIndex / size;
            var checkStatus = checkTurn == EStatus.X ? EStatus.O : EStatus.X;
            var cnt = 1;

            while (true)
            {
                checkIndex -= size;
                if (checkIndex < 0)
                {
                    break;
                }
                else
                {
                    if (Maps[checkIndex].Status == checkStatus)
                    {
                        var anotherSide = checkIndex + (6 * size);
                        if (anotherSide > Maps.Count())
                        {
                            break;
                        }
                        else if (Maps[anotherSide].Status == checkStatus)
                        {
                            return;
                        }
                    }
                    else if (Maps[checkIndex].Status == checkTurn)
                    {
                        cnt++;
                    }
                    else break;
                }
            }
            checkIndex = currentIndex;
            while (true)
            {
                checkIndex += size;
                if (checkIndex > Maps.Count())
                {
                    break;
                }
                else
                {
                    if (Maps[checkIndex].Status == checkTurn)
                    {
                        cnt++;
                    }
                    else break;
                }
            }
            if (cnt >= 5)
            {
                AnnounceWinner(checkTurn);
            }
        }

        void CheckWin45DiognalBothSides(int pos, EStatus checkTurn)
        {
            var currentIndex = pos; // Current position in the 1D array
            var checkIndex = currentIndex;
            int size = (int)Math.Sqrt(Maps.Count); // Grid size (number of columns)
            var currentRow = currentIndex / size;
            var checkStatus = checkTurn == EStatus.X ? EStatus.O : EStatus.X;
            var cnt = 1;
            var checkRow = currentRow;

            while (true)
            {
                checkIndex -= (size - 1);
                checkRow -= 1;
                if (checkIndex < 0 || (checkIndex / size) != checkRow)
                {
                    break;
                }
                else
                {
                    if (Maps[checkIndex].Status == checkStatus)
                    {
                        var anotherSide = checkIndex + (6 * (size - 1));
                        if (anotherSide >= Maps.Count() || (anotherSide / size) != (checkRow + 6))
                        {
                            break;
                        }
                        else if (Maps[anotherSide].Status == checkStatus)
                        {
                            return;
                        }
                    }
                    else if (Maps[checkIndex].Status == checkTurn)
                    {
                        cnt++;
                    }
                    else break;
                }
            }
            checkIndex = currentIndex;
            checkRow = currentRow;
            while (true)
            {
                checkIndex += (size - 1);
                checkRow += 1;
                if (checkIndex >= Maps.Count() || (checkIndex / size) != checkRow)
                {
                    break;
                }
                else
                {
                    if (Maps[checkIndex].Status == checkTurn)
                    {
                        cnt++;
                    }
                    else break;
                }
            }
            if (cnt >= 5)
            {
                AnnounceWinner(checkTurn);
            }
        }

        void CheckWin135DiognalBothSides(int pos, EStatus checkTurn)
        {
            var currentIndex = pos; // Current position in the 1D array
            var checkIndex = currentIndex;
            int size = (int)Math.Sqrt(Maps.Count); // Grid size (number of columns)
            var currentRow = currentIndex / size;
            var checkStatus = checkTurn == EStatus.X ? EStatus.O : EStatus.X;
            var cnt = 1;
            var checkRow = currentRow;

            while (true)
            {
                checkIndex -= (size + 1);
                checkRow -= 1;
                if (checkIndex < 0 || (checkIndex / size) != checkRow)
                {
                    break;
                }
                else
                {
                    if (Maps[checkIndex].Status == checkStatus)
                    {
                        var anotherSide = checkIndex + (6 * (size + 1));
                        if (anotherSide > Maps.Count() || (anotherSide / size) != (checkRow + 6))
                        {
                            break;
                        }
                        else if (Maps[anotherSide].Status == checkStatus)
                        {
                            return;
                        }
                    }
                    else if (Maps[checkIndex].Status == checkTurn)
                    {
                        cnt++;
                    }
                    else break;
                }
            }
            checkIndex = currentIndex;
            checkRow = currentRow;
            while (true)
            {
                checkIndex += (size + 1);
                checkRow += 1;
                if (checkIndex > Maps.Count() || (checkIndex / size) != checkRow)
                {
                    break;
                }
                else
                {
                    if (Maps[checkIndex].Status == checkTurn)
                    {
                        cnt++;
                    }
                    else break;
                }
            }
            if (cnt >= 5)
            {
                AnnounceWinner(checkTurn);
            }
        }
        #endregion

        async void AnnounceWinner(EStatus checkTurn)
        {
            _StatusMessage = checkTurn == EStatus.X ? "X" : "O";
            string announce = $"{_StatusMessage} win";
            MessageBox.Show(announce);
            await connection.SendAsync("ResetMap", roomId);
        }

        void CheckWin(int pos, EStatus checkTurn)
        {
            //CheckWinHorizontal(data);
            //CheckWinVertical(data);
            //CheckWin45Diognal(data);
            //CheckWin135Diognal(data);

            CheckWinHorizontalBothSides(pos, checkTurn);
            CheckWinVerticalBothSides(pos, checkTurn);
            CheckWin45DiognalBothSides(pos, checkTurn);
            CheckWin135DiognalBothSides(pos, checkTurn);
        }

        #region event handler
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as OptionDetail;
            var position = Maps.IndexOf(data);
            Task.Run(async () =>
            {
                await connection.SendAsync("Click", roomId, position);
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            roomId = Int32.Parse(RoomKeyTextBox.Text) ;
            JoinRoomViaApi();
        }
        #endregion
    }

    public class OptionDetail : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        EStatus _Status;
        public EStatus Status
        {
            get { return _Status; }
            set
            {
                _Status = value; OnPropertyChanged();
                //Content = _Status == EStatus.none ? "" : _Status == EStatus.X ? "X" : "O";
                Img = _Status == EStatus.none ? "" : _Status == EStatus.X ? (AppDomain.CurrentDomain.BaseDirectory + $"/imgs/x.png") : (AppDomain.CurrentDomain.BaseDirectory + $"/imgs/o.png");
            }
        }

        //string _Content;
        //public string Content
        //{
        //    get { return _Content; }
        //    set { _Content = value; OnPropertyChanged(); }
        //}

        string _Img;
        public string Img
        {
            get { return _Img; }
            set { _Img = value; OnPropertyChanged(); }
        }
    }

    public enum EStatus
    {
        none,
        X,
        O
    }
}